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
        if (!_tableroCreado || !Multiplayer.IsServer() || _partidaCortada) return;

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

    // ---------- Snapshot y eventos (servidor -> clientes) ----------

    private PartidaSnapshot ConstruirSnapshot()
    {
        var snap = Controller.GetInstance().ExportarEstado(IdDeFicha);

        if (Fichas.Count == 0)
        {
            // Tablero todavía no creado: posiciones iniciales
            for (int i = 0; i < TableroReglas.Filas * TableroReglas.Columnas; i++)
            {
                snap.Fichas.Add(new FichaSnapshot { Posicion = i });
            }
        }
        else
        {
            foreach (Ficha ficha in Fichas)
            {
                snap.Fichas.Add(new FichaSnapshot
                {
                    Posicion = ficha.Datos.Fila * TableroReglas.Columnas + ficha.Datos.Columna,
                    EsComodin = ficha.Datos.EsComodin,
                    Bloqueada = ficha.Datos.Bloqueada
                });
            }
        }

        return snap;
    }

    private void RegistrarEvento(TipoEventoRed tipo, int[] datos = null, string texto = "")
    {
        if (!Multiplayer.IsServer()) return;

        _eventosPendientes.Add(new EventoRed
        {
            Tipo = (int)tipo,
            Datos = datos != null ? new List<int>(datos) : new List<int>(),
            Texto = texto
        });
    }

    // Manda a los clientes el estado actual y los eventos acumulados desde el último envío
    private void EnviarResultado(int indiceActor, bool fueReroll)
    {
        if (!Multiplayer.IsServer()) return;

        string snapshotJson = JsonSerializer.Serialize(ConstruirSnapshot());
        string eventosJson = JsonSerializer.Serialize(_eventosPendientes);
        _eventosPendientes.Clear();

        if (Multiplayer.GetPeers().Length == 0) return;

        Rpc(MethodName.RecibirResultado, snapshotJson, eventosJson, indiceActor, fueReroll);
    }

    // Solo la reciben los clientes. No calculan nada: aplican lo que decidió el servidor.
    [Rpc(MultiplayerApi.RpcMode.Authority)]
    public void RecibirResultado(string snapshotJson, string eventosJson, int indiceActor, bool fueReroll)
    {
        if (Multiplayer.IsServer() || !_tableroCreado) return;

        var snap = JsonSerializer.Deserialize<PartidaSnapshot>(snapshotJson);
        var eventos = JsonSerializer.Deserialize<List<EventoRed>>(eventosJson);

        Controller.GetInstance().ImportarEstado(snap, FichaPorId);

        bool huboMovimiento = AplicarTablero(snap.Fichas);
        if (huboMovimiento)
        {
            _sonidos.GetNode<AudioStreamPlayer>("CambioSFX").Play();
        }

        foreach (EventoRed evento in eventos)
        {
            AplicarEvento(evento);
        }

        if (fueReroll) RerollAplicado?.Invoke(indiceActor);
        AccionAplicada?.Invoke(indiceActor);
    }

    private bool AplicarTablero(List<FichaSnapshot> snaps)
    {
        var movidas = new List<Ficha>();

        for (int i = 0; i < Fichas.Count && i < snaps.Count; i++)
        {
            FichaData datos = Fichas[i].Datos;
            int fila = snaps[i].Posicion / TableroReglas.Columnas;
            int columna = snaps[i].Posicion % TableroReglas.Columnas;

            if (datos.Fila != fila || datos.Columna != columna)
            {
                movidas.Add(Fichas[i]);
            }

            datos.EsComodin = snaps[i].EsComodin;
            datos.Bloqueada = snaps[i].Bloqueada;
            _reglas.ColocarFicha(datos, fila, columna);
        }

        foreach (Ficha ficha in movidas)
        {
            ActualizarPosicionVisual(ficha);
        }

        return movidas.Count > 0;
    }

    // Vuelve a disparar, en el cliente, los eventos que emitió la lógica del servidor
    private void AplicarEvento(EventoRed evento)
    {
        var controller = Controller.GetInstance();

        switch ((TipoEventoRed)evento.Tipo)
        {
            case TipoEventoRed.FiguraEncontrada:
                var celdas = new List<(int fila, int columna)>();
                for (int i = 0; i + 1 < evento.Datos.Count; i += 2)
                {
                    celdas.Add((evento.Datos[i], evento.Datos[i + 1]));
                }
                controller.DispararFiguraEncontrada(celdas);
                break;

            case TipoEventoRed.FichaBloqueada:
                controller.DispararFichaBloqueada(FichaPorId(evento.Datos[0]));
                break;

            case TipoEventoRed.FichaDesbloqueada:
                controller.DispararFichaDesbloqueada(FichaPorId(evento.Datos[0]));
                break;

            case TipoEventoRed.ComodinActivado:
                EmitSignal(SignalName.ComodinActivado, Fichas[evento.Datos[0]]);
                break;

            case TipoEventoRed.ComodinDesactivado:
                controller.DispararFichaComodinDesactivada(FichaPorId(evento.Datos[0]));
                break;

            case TipoEventoRed.UltimoColor:
                controller.DispararUltimoColor((ColorFicha)evento.Datos[0]);
                break;

            case TipoEventoRed.Victoria:
                controller.DispararVictoria(evento.Texto);
                break;

            case TipoEventoRed.TurnoCambiado:
                controller.DispararTurnoCambiado(evento.Datos[0]);
                break;

            case TipoEventoRed.TiempoAgotado:
                controller.DispararTiempoAgotado();
                break;
        }
    }

    // Respuesta del servidor al jugador que mandó una acción (válida o inválida)
    private void Responder(long emisor, bool ok, string mensaje)
    {
        RpcId(emisor, MethodName.RecibirRespuesta, ok, mensaje);
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
    public void RecibirRespuesta(bool ok, string mensaje)
    {
        RespuestaServidor?.Invoke(ok, mensaje);
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

    // ---------- Ausencias, eliminación y fin de partida ----------

    private void PausarPorAusencia()
    {
        _pausadoPorAusencia = true;
        Controller.GetInstance().PausarTemporizador(true);
    }

    private void OnJugadorAusente(int indice)
    {
        var controller = Controller.GetInstance();
        if (controller.JuegoTerminado) return;

        // Si el que se fue está en su turno, el juego espera a que vuelva
        if (indice == controller.JugadorActual && !controller.TurnoEnTransicion)
        {
            PausarPorAusencia();
        }
    }

    private void OnJugadorVolvio(int indice)
    {
        var controller = Controller.GetInstance();
        if (controller.JuegoTerminado) return;

        if (_pausadoPorAusencia && indice == controller.JugadorActual)
        {
            _pausadoPorAusencia = false;
            controller.PausarTemporizador(false);
        }
    }

    private void OnJugadorExpulsado(int indice)
    {
        var controller = Controller.GetInstance();
        if (controller.JuegoTerminado) return;

        bool eraSuTurno = indice == controller.JugadorActual;

        // Libera su comodín y sus bloqueos, y lo marca como eliminado
        controller.EliminarJugador(indice);

        if (controller.JugadoresActivos() < 2)
        {
            string motivo = "Quedan menos de 2 jugadores.\nLa partida terminó sin ganador.";
            if (Multiplayer.GetPeers().Length > 0)
            {
                Rpc(MethodName.RecibirFinPartida, motivo);
            }
            FinalizarPartida(motivo);
            return;
        }

        if (eraSuTurno && !controller.TurnoEnTransicion)
        {
            _pausadoPorAusencia = false;
            controller.TerminarTurno();
        }

        EnviarResultado(-1, false);
    }

    [Rpc(MultiplayerApi.RpcMode.Authority)]
    public void RecibirFinPartida(string motivo)
    {
        if (Multiplayer.IsServer()) return;
        FinalizarPartida(motivo);
    }

    private void OnServidorDesconectado()
    {
        FinalizarPartida("Se perdió la conexión con el host.\nLa partida terminó.");
    }

    private void FinalizarPartida(string motivo)
    {
        if (_partidaCortada) return;
        if (Controller.GetInstance().JuegoTerminado) return;

        _partidaCortada = true;
        Controller.GetInstance().CortarPartida();

        MostrarAviso("");
        GetNode<Label>("UITemporal/PanelVictoria/LabelGanador").Text = motivo;
        Estadisticas.Mostrar(GetNodeOrNull<Label>("UITemporal/PanelVictoria/LabelEstadisticas"));
        GetNode<Control>("UITemporal/PanelVictoria").Show();

        // Refresca los botones del jugador (quedan deshabilitados)
        AccionAplicada?.Invoke(-1);
    }

    // ---------- Validación (servidor) ----------

    private string ValidarTurno(long emisor, out int indiceEmisor)
    {
        var controller = Controller.GetInstance();
        indiceEmisor = DatosPartida.IdsJugadores.IndexOf(emisor);

        if (indiceEmisor < 0) return "No sos parte de esta partida.";
        if (controller.Jugadores()[indiceEmisor].Eliminado) return "Fuiste eliminado de la partida.";
        if (controller.JuegoTerminado) return "La partida ya terminó.";
        if (controller.TurnoEnTransicion) return "El turno está cambiando, esperá un momento.";
        if (indiceEmisor != controller.JugadorActual) return "No es tu turno.";
        return null;
    }

    private string ValidarMovimiento(int indiceJugador, int indiceCarta, int filaA, int colA, int filaB, int colB)
    {
        var jugador = Controller.GetInstance().Jugadores()[indiceJugador];

        if (indiceCarta < 0 || indiceCarta >= jugador.manoCartas.Count) return "Carta inválida.";

        CartaMovimiento carta = jugador.manoCartas[indiceCarta];
        if (carta == null) return "Esa carta ya fue usada.";

        if (!EnTablero(filaA, colA) || !EnTablero(filaB, colB)) return "Posición fuera del tablero.";

        FichaData fichaA = _reglas.ObtenerFicha(filaA, colA);
        FichaData fichaB = _reglas.ObtenerFicha(filaB, colB);
        if (fichaA == null || fichaB == null) return "Ficha inexistente.";
        if (fichaA.Bloqueada || fichaB.Bloqueada) return "No podés mover una ficha bloqueada.";

        if (!carta.EsValido(_reglas, filaA, colA, filaB, colB)) return "Ese movimiento no es válido para la carta elegida.";
        return null;
    }

    private string ValidarHabilidad(int indiceJugador, int tipo, int fila, int columna, int indiceObjetivo)
    {
        var controller = Controller.GetInstance();

        if (!controller.PuedeUsarHabilidad()) return "La habilidad todavía se está recargando.";

        var jugador = controller.Jugadores()[indiceJugador];
        if ((int)jugador.PersonajeAsignado.Tipo != tipo) return "Esa no es tu habilidad.";

        var tipoHabilidad = (TipoHabilidad)tipo;

        if (tipoHabilidad == TipoHabilidad.Lobizon || tipoHabilidad == TipoHabilidad.Mulanima)
        {
            if (!EnTablero(fila, columna)) return "Posición fuera del tablero.";

            FichaData ficha = _reglas.ObtenerFicha(fila, columna);
            if (ficha == null) return "Ficha inexistente.";
            if (ficha.Bloqueada) return "No podés usar la habilidad sobre una ficha bloqueada.";
        }

        if (tipoHabilidad == TipoHabilidad.LuzMala)
        {
            if (indiceObjetivo < 0 || indiceObjetivo >= controller.Jugadores().Length || indiceObjetivo == indiceJugador)
            {
                return "Jugador objetivo inválido.";
            }
            if (controller.Jugadores()[indiceObjetivo].Eliminado) return "Ese jugador ya fue eliminado.";
        }

        return null;
    }

    // ---------- Mover una ficha ----------

    public void SolicitarMovimiento(int indiceCarta, int filaA, int colA, int filaB, int colB)
    {
        RpcId(1, MethodName.PedirMovimiento, indiceCarta, filaA, colA, filaB, colB);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public void PedirMovimiento(int indiceCarta, int filaA, int colA, int filaB, int colB)
    {
        if (!Multiplayer.IsServer()) return;

        long emisor = EmisorActual();
        string error = ValidarTurno(emisor, out int indice);
        if (error == null) error = ValidarMovimiento(indice, indiceCarta, filaA, colA, filaB, colB);

        if (error != null)
        {
            Responder(emisor, false, error);
            return;
        }

        EjecutarMovimiento(indice, indiceCarta, filaA, colA, filaB, colB);
        Responder(emisor, true, "Movimiento realizado.");
        EnviarResultado(indice, false);
    }

    private void EjecutarMovimiento(int indiceJugador, int indiceCarta, int filaA, int colA, int filaB, int colB)
    {
        var controller = Controller.GetInstance();
        CartaMovimiento carta = controller.Jugadores()[indiceJugador].manoCartas[indiceCarta];

        Ficha nodoA = GetFichaEnPosicion(filaA, colA);
        Ficha nodoB = GetFichaEnPosicion(filaB, colB);

        carta.Ejecutar(_reglas, filaA, colA, filaB, colB);
        _sonidos.GetNode<AudioStreamPlayer>("CambioSFX").Play();

        ActualizarPosicionVisual(nodoA);
        ActualizarPosicionVisual(nodoB);

        var celdasMovidas = new HashSet<(int fila, int columna)>
        {
            (nodoA.Datos.Fila, nodoA.Datos.Columna),
            (nodoB.Datos.Fila, nodoB.Datos.Columna)
        };

        controller.ChequearFigurasCompletadas(_reglas, celdasMovidas);
        controller.RegistrarMovimientoRealizado(carta);

        GD.Print($"Movimiento aplicado: jugador {indiceJugador}, carta {carta.Nombre}, ({filaA},{colA}) -> ({filaB},{colB})");
        AccionAplicada?.Invoke(indiceJugador);
    }

    // ---------- Finalizar turno ----------

    public void SolicitarFinTurno()
    {
        RpcId(1, MethodName.PedirFinTurno);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public void PedirFinTurno()
    {
        if (!Multiplayer.IsServer()) return;

        long emisor = EmisorActual();
        string error = ValidarTurno(emisor, out int indice);
        if (error == null && !Controller.GetInstance().Jugadores()[indice].RealizoAccionEsteTurno)
        {
            error = "Tenés que realizar al menos una acción antes de terminar el turno.";
        }

        if (error != null)
        {
            Responder(emisor, false, error);
            return;
        }

        Controller.GetInstance().TerminarTurno();
        FinTurnoAplicado?.Invoke();
        Responder(emisor, true, "Turno finalizado.");
        EnviarResultado(-1, false);
    }

    // ---------- Habilidades ----------

    public void SolicitarHabilidad(TipoHabilidad tipo, int fila, int columna, int indiceObjetivo)
    {
        RpcId(1, MethodName.PedirHabilidad, (int)tipo, fila, columna, indiceObjetivo);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public void PedirHabilidad(int tipo, int fila, int columna, int indiceObjetivo)
    {
        if (!Multiplayer.IsServer()) return;

        long emisor = EmisorActual();
        string error = ValidarTurno(emisor, out int indice);
        if (error == null) error = ValidarHabilidad(indice, tipo, fila, columna, indiceObjetivo);
        if (error == null) error = EjecutarHabilidad(indice, tipo, fila, columna, indiceObjetivo);

        if (error != null)
        {
            Responder(emisor, false, error);
            return;
        }

        Responder(emisor, true, "Habilidad activada.");
        EnviarResultado(indice, false);
    }

    private string EjecutarHabilidad(int indiceJugador, int tipo, int fila, int columna, int indiceObjetivo)
    {
        var controller = Controller.GetInstance();
        var tipoHabilidad = (TipoHabilidad)tipo;

        Ficha nodoFicha = null;
        FichaData ficha = null;
        if (tipoHabilidad == TipoHabilidad.Lobizon || tipoHabilidad == TipoHabilidad.Mulanima)
        {
            nodoFicha = GetFichaEnPosicion(fila, columna);
            ficha = nodoFicha.Datos;
        }

        Jugador objetivo = null;
        if (tipoHabilidad == TipoHabilidad.LuzMala)
        {
            objetivo = controller.Jugadores()[indiceObjetivo];
        }

        if (!controller.ActivarHabilidad(tipoHabilidad, ficha, objetivo))
        {
            return "No se pudo activar la habilidad.";
        }

        if (tipoHabilidad == TipoHabilidad.Lobizon)
        {
            EmitSignal(SignalName.ComodinActivado, nodoFicha);
            RegistrarEvento(TipoEventoRed.ComodinActivado, new[] { IdDeFicha(ficha) });

            var celdaComodin = new HashSet<(int fila, int columna)>
            {
                (ficha.Fila, ficha.Columna)
            };
            controller.ChequearFigurasCompletadas(_reglas, celdaComodin);
        }

        GD.Print($"Habilidad aplicada: jugador {indiceJugador}, {tipoHabilidad}");
        AccionAplicada?.Invoke(indiceJugador);
        return null;
    }

    // ---------- Reroll ----------

    public void SolicitarReroll()
    {
        RpcId(1, MethodName.PedirReroll);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public void PedirReroll()
    {
        if (!Multiplayer.IsServer()) return;

        long emisor = EmisorActual();
        string error = ValidarTurno(emisor, out int indice);
        if (error == null)
        {
            var jugador = Controller.GetInstance().Jugadores()[indice];
            if (jugador.RealizoAccionEsteTurno) error = "Solo podés rerollear antes de realizar una acción.";
            else if (!jugador.RerollDisponible) error = "Ya usaste el reroll este turno.";
        }

        if (error != null)
        {
            Responder(emisor, false, error);
            return;
        }

        Controller.GetInstance().RerollearManoActual();
        RerollAplicado?.Invoke(indice);
        AccionAplicada?.Invoke(indice);

        Responder(emisor, true, "Mano rerolleada.");
        EnviarResultado(indice, true);
    }
}