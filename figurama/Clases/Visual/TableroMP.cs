using Godot;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

public partial class TableroMP : Node3D
{
    [Export] public PackedScene FichaScene;
    [Export] public PackedScene JugadorScene;
    [Export] public PackedScene EspectadorScene;

    [Signal] public delegate void FiguraCompletadaEventHandler(Ficha ficha);
    [Signal] public delegate void BloqueadaEventHandler(Ficha ficha);
    [Signal] public delegate void DesbloqueadaEventHandler(Ficha ficha);
    [Signal] public delegate void ComodinActivadoEventHandler(Ficha ficha);
    [Signal] public delegate void ComodinDesactivadoEventHandler(Ficha ficha);

    public event System.Action<int> AccionAplicada;
    public event System.Action<int> RerollAplicado;
    public event System.Action FinTurnoAplicado;
    public event System.Action<bool, string> RespuestaServidor;

    private const float SizeCelda = 0.8f;

    private readonly TableroReglas _reglas = new TableroReglas();
    private readonly GestorFichas _gestorFichas = new GestorFichas();
    private readonly List<EventoRed> _eventosPendientes = new List<EventoRed>();
    private readonly List<long> _espectadoresEsperando = new List<long>();
    private Node _sonidos;
    private Label _labelAviso;
    private int[] _coloresIniciales;
    private int _clientesListos = 0;
    private bool _tableroEnviado = false;
    private bool _tableroCreado = false;
    private bool _partidaCortada = false;
    private bool _pausadoPorAusencia = false;
    private float _acumuladorTiempo = 0f;

    public List<Ficha> Fichas { get; private set; } = new List<Ficha>();
    public TableroReglas Reglas => _reglas;

    public override void _Ready()
    {
        _sonidos = GetNode<Node>("Sonidos");
        CrearLabelAviso();

        // El reloj arranca recién cuando el tablero está creado
        Controller.GetInstance().PausarTemporizador(true);

        Multiplayer.ServerDisconnected += OnServidorDesconectado;
        GetNode<Button>("UITemporal/PanelVictoria/BotonVolverMenu").Pressed += OnVolverMenuPresionado;
        
        // El botón de pausa es del tablero: se conecta una sola vez (si ya tenía una conexión del editor, se respeta)
        var botonPausa = GetNodeOrNull<BaseButton>("UITemporal/BotonPausa");

        if (botonPausa != null && botonPausa.GetSignalConnectionList(BaseButton.SignalName.Pressed).Count == 0)
        {
            botonPausa.Pressed += OnPausaPresionada;
        }

        if (Multiplayer.IsServer())
        {
            GestorSala.Instancia.JugadorAusente += OnJugadorAusente;
            GestorSala.Instancia.JugadorVolvio += OnJugadorVolvio;
            GestorSala.Instancia.JugadorExpulsado += OnJugadorExpulsado;
            IntentarEnviarTablero();
        }
        else
        {
            RpcId(1, MethodName.ClienteListo);
        }
    }

    public override void _ExitTree()
    {
        Multiplayer.ServerDisconnected -= OnServidorDesconectado;

        if (GestorSala.Instancia != null)
        {
            GestorSala.Instancia.JugadorAusente -= OnJugadorAusente;
            GestorSala.Instancia.JugadorVolvio -= OnJugadorVolvio;
            GestorSala.Instancia.JugadorExpulsado -= OnJugadorExpulsado;
        }

        if (Controller._instance != null)
        {
            var controller = Controller.GetInstance();
            controller.FiguraEncontrada -= OnFiguraCompletada;
            controller.FichaBloqueadaEvent -= OnFichaBloqueada;
            controller.FichaDesbloqueadaEvent -= OnFichaDesbloqueada;
            controller.FichaComodinDesactivada -= OnFichaComodinDesactivada;
            controller.UltimoColorCambiado -= OnUltimoColorCambiado;
            controller.Victoria -= OnVictoria;
            controller.TurnoCambiado -= OnTurnoCambiado;
            controller.TiempoAgotado -= OnTiempoAgotado;
            controller.FinTurnoPorTiempo -= OnFinTurnoPorTiempo;
        }
    }

    public override void _Process(double delta)
    {
        // El servidor manda el tiempo restante y el aviso de ausencias cada segundo
        if (!_tableroCreado || _partidaCortada) return;
        if (!Red.EsServidor(Multiplayer)) return;

        _acumuladorTiempo += (float)delta;
        if (_acumuladorTiempo < 1f) return;
        _acumuladorTiempo = 0f;

        var controller = Controller.GetInstance();
        string aviso = ConstruirAviso();
        MostrarAviso(aviso);

        if (Multiplayer.GetPeers().Length > 0)
        {
            Rpc(MethodName.RecibirTiempo, controller.TiempoRestante, controller.TemporizadorDetenido, aviso);
        }
    }

    // ---------- Aviso de jugadores ausentes ----------

    private void CrearLabelAviso()
    {
        _labelAviso = new Label
        {
            Visible = false,
            MouseFilter = Control.MouseFilterEnum.Ignore,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        _labelAviso.AddThemeFontSizeOverride("font_size", 24);
        _labelAviso.AddThemeColorOverride("font_color", new Color(1f, 0.85f, 0.3f));
        _labelAviso.AddThemeColorOverride("font_outline_color", Colors.Black);
        _labelAviso.AddThemeConstantOverride("outline_size", 6);

        GetNode<Node>("UITemporal").AddChild(_labelAviso);
        _labelAviso.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.CenterTop);
        _labelAviso.GrowHorizontal = Control.GrowDirection.Both;
        _labelAviso.OffsetTop = 90;
    }

    private void MostrarAviso(string texto)
    {
        if (_labelAviso == null) return;

        _labelAviso.Text = texto;
        _labelAviso.Visible = !string.IsNullOrEmpty(texto);
    }

    // Solo en el servidor: arma el texto con los jugadores ausentes y el tiempo que les queda
    private string ConstruirAviso()
    {
        var controller = Controller.GetInstance();
        if (controller.JuegoTerminado || controller.Jugadores() == null) return "";

        GestorSala.Instancia.ObtenerAusencias(out int[] indices, out int[] segundos);
        if (indices.Length == 0) return "";

        Jugador[] jugadores = controller.Jugadores();
        var texto = new StringBuilder();
        for (int i = 0; i < indices.Length; i++)
        {
            texto.Append($"{jugadores[indices[i]].nombre} se desconectó: {segundos[i]} s para volver");
            if (indices[i] == controller.JugadorActual) texto.Append(" (partida en pausa)");
            if (i < indices.Length - 1) texto.AppendLine();
        }
        return texto.ToString();
    }

    [Rpc(MultiplayerApi.RpcMode.Authority)]
    public void RecibirTiempo(float tiempo, bool detenido, string aviso)
    {
        if (Multiplayer.IsServer() || !_tableroCreado) return;

        Controller.GetInstance().AplicarTiempoRemoto(tiempo, detenido);
        MostrarAviso(aviso);
    }

    // ---------- Creación del tablero ----------

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    public void ClienteListo()
    {
        if (!Multiplayer.IsServer()) return;

        long emisor = EmisorActual();

        // Entrada tardía (espectador o jugador que vuelve): se le manda el estado actual
        if (_tableroCreado)
        {
            EnviarTableroA(emisor);
            return;
        }

        if (DatosPartida.IdsJugadores.Contains(emisor))
        {
            _clientesListos++;
            IntentarEnviarTablero();
        }
        else
        {
            _espectadoresEsperando.Add(emisor);
        }
    }

    private void IntentarEnviarTablero()
    {
        if (_tableroEnviado) return;
        if (_clientesListos < DatosPartida.IdsJugadores.Count - 1) return;

        _tableroEnviado = true;
        _coloresIniciales = _gestorFichas.GenerarIndicesColores();
        string estadoJson = JsonSerializer.Serialize(ConstruirSnapshot());

        // Solo los jugadores reciben el tablero inicial; los demás lo piden al entrar
        foreach (long id in DatosPartida.IdsJugadores)
        {
            if (id > 1) RpcId(id, MethodName.CrearTablero, _coloresIniciales, estadoJson);
        }
        CrearTablero(_coloresIniciales, estadoJson);

        foreach (long espectador in _espectadoresEsperando)
        {
            EnviarTableroA(espectador);
        }
        _espectadoresEsperando.Clear();
    }

    // Servidor: manda el tablero con su estado actual a quien entra a mitad de partida
    private void EnviarTableroA(long id)
    {
        string estadoJson = JsonSerializer.Serialize(ConstruirSnapshot());
        RpcId(id, MethodName.CrearTablero, _coloresIniciales, estadoJson);

        // Que la música del que entra tarde siga a la del host en vez de arrancar de cero
        var musica = GetNodeOrNull<AudioStreamPlayer>("Musica/MusicaPrincipal");
        if (musica != null && musica.Playing)
        {
            RpcId(id, MethodName.SincronizarMusica, musica.GetPlaybackPosition());
        }
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
    public void CrearTablero(int[] colores, string estadoJson)
    {
        Fichas = _gestorFichas.CrearFichasConColores(FichaScene, this, _reglas, SizeCelda, colores);
        GD.Print($"Tablero creado con {Fichas.Count} fichas. Colores: {string.Join(",", colores)}");

        if (!Multiplayer.IsServer())
        {
            // El cliente no calcula nada: carga el estado que mandó el servidor
            var snap = JsonSerializer.Deserialize<PartidaSnapshot>(estadoJson);
            Controller.GetInstance().ImportarEstado(snap, FichaPorId);
            AplicarTablero(snap.Fichas);

            if (snap.UltimoColor >= 0)
            {
                Controller.GetInstance().DispararUltimoColor((ColorFicha)snap.UltimoColor);
            }
        }

        var controller = Controller.GetInstance();
        controller.FiguraEncontrada += OnFiguraCompletada;
        controller.FichaBloqueadaEvent += OnFichaBloqueada;
        controller.FichaDesbloqueadaEvent += OnFichaDesbloqueada;
        controller.FichaComodinDesactivada += OnFichaComodinDesactivada;
        controller.UltimoColorCambiado += OnUltimoColorCambiado;
        controller.Victoria += OnVictoria;
        controller.TurnoCambiado += OnTurnoCambiado;
        controller.TiempoAgotado += OnTiempoAgotado;
        controller.FinTurnoPorTiempo += OnFinTurnoPorTiempo;

        foreach (Ficha ficha in Fichas)
        {
            Connect(SignalName.FiguraCompletada, new Callable(ficha, "_on_figura_completada"));
            Connect(SignalName.Bloqueada, new Callable(ficha, "_on_ficha_bloqueada"));
            Connect(SignalName.Desbloqueada, new Callable(ficha, "_on_ficha_desbloqueada"));
            Connect(SignalName.ComodinActivado, new Callable(ficha, "_on_ficha_comodin_activado"));
            Connect(SignalName.ComodinDesactivado, new Callable(ficha, "_on_ficha_comodin_desactivado"));
        }

        // Si entré con la partida avanzada, muestro los efectos que ya estaban activos
        foreach (Ficha ficha in Fichas)
        {
            if (ficha.Datos.EsComodin) EmitSignal(SignalName.ComodinActivado, ficha);
            if (ficha.Datos.Bloqueada) EmitSignal(SignalName.Bloqueada, ficha);
        }

        PackedScene escenaLocal = DatosPartida.EsEspectador ? EspectadorScene : JugadorScene;
        AddChild(escenaLocal.Instantiate<Node3D>());

        _tableroCreado = true;
        if (Multiplayer.IsServer())
        {
            Controller.GetInstance().PausarTemporizador(false);
        }
    }

    // ---------- Utilidades ----------

    public Ficha GetFichaEnPosicion(int fila, int columna)
    {
        foreach (Ficha ficha in Fichas)
        {
            if (ficha.Datos.Fila == fila && ficha.Datos.Columna == columna)
            {
                return ficha;
            }
        }
        return null;
    }

    private FichaData FichaPorId(int id)
    {
        return (id >= 0 && id < Fichas.Count) ? Fichas[id].Datos : null;
    }

    private int IdDeFicha(FichaData datos)
    {
        return Fichas.FindIndex(f => f.Datos == datos);
    }

    private bool EnTablero(int fila, int columna)
    {
        return fila >= 0 && fila < TableroReglas.Filas && columna >= 0 && columna < TableroReglas.Columnas;
    }

    private void ActualizarPosicionVisual(Ficha ficha)
    {
        Vector3 nuevaPosicion = new Vector3(ficha.Datos.Columna * SizeCelda, 0, ficha.Datos.Fila * SizeCelda);
        ficha.AnimarCambio(nuevaPosicion);
    }

    private long EmisorActual()
    {
        long emisor = Multiplayer.GetRemoteSenderId();
        return emisor == 0 ? 1 : emisor; // 0 = llamada local del host
    }

    // ---------- Reacción a los eventos de la lógica (en servidor y en clientes) ----------

    private void OnFiguraCompletada(List<(int fila, int columna)> celdas)
    {
        _sonidos.GetNode<AudioStreamPlayer>("FiguraSFX").Play();

        var datos = new List<int>();
        foreach (var celda in celdas)
        {
            datos.Add(celda.fila);
            datos.Add(celda.columna);

            Ficha ficha = GetFichaEnPosicion(celda.fila, celda.columna);
            if (ficha != null)
            {
                EmitSignal(SignalName.FiguraCompletada, ficha);
            }
        }

        RegistrarEvento(TipoEventoRed.FiguraEncontrada, datos.ToArray());
    }

    private void OnFichaBloqueada(FichaData datos)
    {
        Ficha ficha = GetFichaEnPosicion(datos.Fila, datos.Columna);
        if (ficha != null) EmitSignal(SignalName.Bloqueada, ficha);

        RegistrarEvento(TipoEventoRed.FichaBloqueada, new[] { IdDeFicha(datos) });
    }

    private void OnFichaDesbloqueada(FichaData datos)
    {
        Ficha ficha = GetFichaEnPosicion(datos.Fila, datos.Columna);
        if (ficha != null) EmitSignal(SignalName.Desbloqueada, ficha);

        RegistrarEvento(TipoEventoRed.FichaDesbloqueada, new[] { IdDeFicha(datos) });
    }

    private void OnFichaComodinDesactivada(FichaData datos)
    {
        Ficha ficha = GetFichaEnPosicion(datos.Fila, datos.Columna);
        if (ficha != null) EmitSignal(SignalName.ComodinDesactivado, ficha);

        RegistrarEvento(TipoEventoRed.ComodinDesactivado, new[] { IdDeFicha(datos) });
    }

    private void OnUltimoColorCambiado(ColorFicha color)
    {
        RegistrarEvento(TipoEventoRed.UltimoColor, new[] { (int)color });
    }

    private void OnVictoria(string nombreGanador)
    {
        MostrarVictoria(nombreGanador);
        RegistrarEvento(TipoEventoRed.Victoria, null, nombreGanador);
    }

    private void OnTurnoCambiado(int jugadorActual)
    {
        if (Multiplayer.IsServer())
        {
            // El reloj se reinició con el turno: si le toca a un ausente, se vuelve a frenar
            _pausadoPorAusencia = false;
            if (GestorSala.Instancia.EstaAusente(jugadorActual))
            {
                PausarPorAusencia();
            }
        }

        RegistrarEvento(TipoEventoRed.TurnoCambiado, new[] { jugadorActual });
        EnviarResultado(-1, false);
    }

    private void OnTiempoAgotado()
    {
        RegistrarEvento(TipoEventoRed.TiempoAgotado);
        EnviarResultado(-1, false);
    }

    // ---------- Tiempo y victoria ----------

    private void OnFinTurnoPorTiempo()
    {
        // Solo el servidor decide cuándo termina el turno por tiempo
        if (!Multiplayer.IsServer()) return;

        Controller.GetInstance().TerminarTurno();
        EnviarResultado(-1, false);
    }

    private void MostrarVictoria(string nombreGanador)
    {
        GetNode<Label>("UITemporal/PanelVictoria/LabelGanador").Text = $"¡{nombreGanador} ganó la partida!";
        Estadisticas.Mostrar(GetNodeOrNull<Label>("UITemporal/PanelVictoria/LabelEstadisticas"));
        GetNode<Control>("UITemporal/PanelVictoria").Show();
    }

    private void OnVolverMenuPresionado()
    {
        if (Multiplayer.MultiplayerPeer != null)
        {
            Multiplayer.MultiplayerPeer.Close();
        }
        Multiplayer.MultiplayerPeer = new OfflineMultiplayerPeer();
        GetTree().ChangeSceneToFile("res://Objetos/menuPrincipal.tscn");
    }

    private void OnPausaPresionada()
    {
        GD.Print("[Pausa] clic recibido en el botón");
        // MenuPausa puede estar en esta interfaz o en otra capa: si no está acá, se busca por nombre
        MenuPausa menu = GetNodeOrNull<MenuPausa>("UITemporal/MenuPausa")
                         ?? GetTree().Root.FindChild("MenuPausa", true, false) as MenuPausa;

        if (menu == null)
        {
            GD.PrintErr("[TableroMP] No se encontró el nodo 'MenuPausa'.");
            return;
        }

        menu.TogglePausa();
    }
}