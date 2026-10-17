using Godot;
using System.Collections.Generic;

public partial class TableroMP : Node3D
{
    [Export] public PackedScene FichaScene;
    [Export] public PackedScene JugadorScene;

    [Signal] public delegate void FiguraCompletadaEventHandler(Ficha ficha);
    [Signal] public delegate void BloqueadaEventHandler(Ficha ficha);
    [Signal] public delegate void DesbloqueadaEventHandler(Ficha ficha);
    [Signal] public delegate void ComodinActivadoEventHandler(Ficha ficha);
    [Signal] public delegate void ComodinDesactivadoEventHandler(Ficha ficha);

    public event System.Action<int> AccionAplicada;
    public event System.Action<int> RerollAplicado;
    public event System.Action FinTurnoAplicado;

    private const float SizeCelda = 0.8f;

    private readonly TableroReglas _reglas = new TableroReglas();
    private readonly GestorFichas _gestorFichas = new GestorFichas();
    private Node _sonidos;
    private int _clientesListos = 0;
    private bool _tableroEnviado = false;
    private bool _partidaCortada = false;

    public List<Ficha> Fichas { get; private set; } = new List<Ficha>();
    public TableroReglas Reglas => _reglas;

    public override void _Ready()
    {
        _sonidos = GetNode<Node>("Sonidos");

        // El reloj arranca recién cuando el tablero está creado
        Controller.GetInstance().PausarTemporizador(true);
        Controller.GetInstance().Victoria += MostrarVictoria;
        Controller.GetInstance().FinTurnoPorTiempo += OnFinTurnoPorTiempo;
        Multiplayer.PeerDisconnected += OnPeerDesconectado;
        Multiplayer.ServerDisconnected += OnServidorDesconectado;

        GetNode<Button>("UITemporal/PanelVictoria/BotonVolverMenu").Pressed += OnVolverMenuPresionado;

        if (Multiplayer.IsServer())
        {
            IntentarEnviarTablero();
        }
        else
        {
            RpcId(1, MethodName.ClienteListo);
        }
    }

    public override void _ExitTree()
    {
        Multiplayer.PeerDisconnected -= OnPeerDesconectado;
        Multiplayer.ServerDisconnected -= OnServidorDesconectado;

        if (Controller._instance != null)
        {
            Controller.GetInstance().FiguraEncontrada -= OnFiguraCompletada;
            Controller.GetInstance().FichaBloqueadaEvent -= OnFichaBloqueada;
            Controller.GetInstance().FichaDesbloqueadaEvent -= OnFichaDesbloqueada;
            Controller.GetInstance().FichaComodinDesactivada -= OnFichaComodinDesactivada;
            Controller.GetInstance().Victoria -= MostrarVictoria;
            Controller.GetInstance().FinTurnoPorTiempo -= OnFinTurnoPorTiempo;
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    public void ClienteListo()
    {
        if (!Multiplayer.IsServer()) return;

        _clientesListos++;
        IntentarEnviarTablero();
    }

    private void IntentarEnviarTablero()
    {
        if (_tableroEnviado) return;
        if (_clientesListos < Multiplayer.GetPeers().Length) return;

        _tableroEnviado = true;
        int[] colores = _gestorFichas.GenerarIndicesColores();
        Rpc(MethodName.CrearTablero, colores);
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
    public void CrearTablero(int[] colores)
    {
        Fichas = _gestorFichas.CrearFichasConColores(FichaScene, this, _reglas, SizeCelda, colores);
        GD.Print($"Tablero creado con {Fichas.Count} fichas. Colores: {string.Join(",", colores)}");

        Controller.GetInstance().FiguraEncontrada += OnFiguraCompletada;
        Controller.GetInstance().FichaBloqueadaEvent += OnFichaBloqueada;
        Controller.GetInstance().FichaDesbloqueadaEvent += OnFichaDesbloqueada;
        Controller.GetInstance().FichaComodinDesactivada += OnFichaComodinDesactivada;

        foreach (Ficha ficha in Fichas)
        {
            Connect(SignalName.FiguraCompletada, new Callable(ficha, "_on_figura_completada"));
            Connect(SignalName.Bloqueada, new Callable(ficha, "_on_ficha_bloqueada"));
            Connect(SignalName.Desbloqueada, new Callable(ficha, "_on_ficha_desbloqueada"));
            Connect(SignalName.ComodinActivado, new Callable(ficha, "_on_ficha_comodin_activado"));
            Connect(SignalName.ComodinDesactivado, new Callable(ficha, "_on_ficha_comodin_desactivado"));
        }

        AddChild(JugadorScene.Instantiate<Node3D>());

        Controller.GetInstance().PausarTemporizador(false);
    }

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

    private void ActualizarPosicionVisual(Ficha ficha)
    {
        ficha.Position = new Vector3(ficha.Datos.Columna * SizeCelda, 0, ficha.Datos.Fila * SizeCelda);
    }

    private void OnFiguraCompletada(List<(int fila, int columna)> celdas)
    {
        _sonidos.GetNode<AudioStreamPlayer>("FiguraSFX").Play();
        foreach (var celda in celdas)
        {
            Ficha ficha = GetFichaEnPosicion(celda.fila, celda.columna);
            if (ficha != null)
            {
                EmitSignal(SignalName.FiguraCompletada, ficha);
            }
        }
    }

    private void OnFichaBloqueada(FichaData datos)
    {
        Ficha ficha = GetFichaEnPosicion(datos.Fila, datos.Columna);
        if (ficha != null) EmitSignal(SignalName.Bloqueada, ficha);
    }

    private void OnFichaDesbloqueada(FichaData datos)
    {
        Ficha ficha = GetFichaEnPosicion(datos.Fila, datos.Columna);
        if (ficha != null) EmitSignal(SignalName.Desbloqueada, ficha);
    }

    private void OnFichaComodinDesactivada(FichaData datos)
    {
        Ficha ficha = GetFichaEnPosicion(datos.Fila, datos.Columna);
        if (ficha != null) EmitSignal(SignalName.ComodinDesactivado, ficha);
    }

    // ---------- Tiempo y victoria ----------

    private void OnFinTurnoPorTiempo()
    {
        // Solo el host decide cuándo termina el turno por tiempo
        if (Multiplayer.IsServer())
        {
            Rpc(MethodName.AplicarFinTurno);
        }
    }

    private void MostrarVictoria(string nombreGanador)
    {
        GetNode<Label>("UITemporal/PanelVictoria/LabelGanador").Text = $"¡{nombreGanador} ganó la partida!";
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

    private void OnPeerDesconectado(long id)
    {
        TerminarPorDesconexion("Un jugador se desconectó.");
    }

    private void OnServidorDesconectado()
    {
        TerminarPorDesconexion("Se perdió la conexión con el host.");
    }

    private void TerminarPorDesconexion(string motivo)
    {
        if (_partidaCortada) return;
        if (Controller.GetInstance().JuegoTerminado) return;

        _partidaCortada = true;
        Controller.GetInstance().CortarPartida();

        GetNode<Label>("UITemporal/PanelVictoria/LabelGanador").Text = $"{motivo}\nLa partida terminó.";
        GetNode<Control>("UITemporal/PanelVictoria").Show();

        // Refresca los botones del jugador (quedan deshabilitados)
        AccionAplicada?.Invoke(-1);
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

        long emisor = Multiplayer.GetRemoteSenderId();
        if (emisor == 0) emisor = 1;

        var controller = Controller.GetInstance();
        int indiceEmisor = DatosPartida.IdsJugadores.IndexOf(emisor);
        if (controller.JuegoTerminado || controller.TurnoEnTransicion || indiceEmisor != controller.JugadorActual) return;

        var jugador = controller.Jugadores()[indiceEmisor];
        if (indiceCarta < 0 || indiceCarta >= jugador.manoCartas.Count) return;

        CartaMovimiento carta = jugador.manoCartas[indiceCarta];
        if (carta == null) return;

        if (filaA < 0 || filaA >= TableroReglas.Filas || colA < 0 || colA >= TableroReglas.Columnas) return;
        if (filaB < 0 || filaB >= TableroReglas.Filas || colB < 0 || colB >= TableroReglas.Columnas) return;

        FichaData fichaA = _reglas.ObtenerFicha(filaA, colA);
        FichaData fichaB = _reglas.ObtenerFicha(filaB, colB);
        if (fichaA == null || fichaB == null || fichaA.Bloqueada || fichaB.Bloqueada) return;

        if (!carta.EsValido(_reglas, filaA, colA, filaB, colB)) return;

        Rpc(MethodName.AplicarMovimiento, indiceEmisor, indiceCarta, filaA, colA, filaB, colB);
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
    public void AplicarMovimiento(int indiceJugador, int indiceCarta, int filaA, int colA, int filaB, int colB)
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

        long emisor = Multiplayer.GetRemoteSenderId();
        if (emisor == 0) emisor = 1;

        var controller = Controller.GetInstance();
        int indiceEmisor = DatosPartida.IdsJugadores.IndexOf(emisor);
        if (controller.JuegoTerminado || controller.TurnoEnTransicion || indiceEmisor != controller.JugadorActual) return;
        if (!controller.Jugadores()[indiceEmisor].RealizoAccionEsteTurno) return;

        Rpc(MethodName.AplicarFinTurno);
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
    public void AplicarFinTurno()
    {
        Controller.GetInstance().TerminarTurno();
        FinTurnoAplicado?.Invoke();
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

        long emisor = Multiplayer.GetRemoteSenderId();
        if (emisor == 0) emisor = 1;

        var controller = Controller.GetInstance();
        int indiceEmisor = DatosPartida.IdsJugadores.IndexOf(emisor);
        if (controller.JuegoTerminado || controller.TurnoEnTransicion || indiceEmisor != controller.JugadorActual) return;
        if (!controller.PuedeUsarHabilidad()) return;

        var jugador = controller.Jugadores()[indiceEmisor];
        if ((int)jugador.PersonajeAsignado.Tipo != tipo) return;

        var tipoHabilidad = (TipoHabilidad)tipo;

        if (tipoHabilidad == TipoHabilidad.Lobizon || tipoHabilidad == TipoHabilidad.Mulanima)
        {
            if (fila < 0 || fila >= TableroReglas.Filas || columna < 0 || columna >= TableroReglas.Columnas) return;

            FichaData ficha = _reglas.ObtenerFicha(fila, columna);
            if (ficha == null || ficha.Bloqueada) return;
        }

        if (tipoHabilidad == TipoHabilidad.LuzMala)
        {
            if (indiceObjetivo < 0 || indiceObjetivo >= controller.Jugadores().Length || indiceObjetivo == indiceEmisor) return;
        }

        Rpc(MethodName.AplicarHabilidad, indiceEmisor, tipo, fila, columna, indiceObjetivo);
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
    public void AplicarHabilidad(int indiceJugador, int tipo, int fila, int columna, int indiceObjetivo)
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

        bool activada = controller.ActivarHabilidad(tipoHabilidad, ficha, objetivo);
        if (!activada)
        {
            GD.PrintErr($"La habilidad {tipoHabilidad} no se pudo activar en esta PC.");
            return;
        }

        if (tipoHabilidad == TipoHabilidad.Lobizon)
        {
            EmitSignal(SignalName.ComodinActivado, nodoFicha);

            var celdaComodin = new HashSet<(int fila, int columna)>
            {
                (ficha.Fila, ficha.Columna)
            };
            controller.ChequearFigurasCompletadas(_reglas, celdaComodin);
        }

        GD.Print($"Habilidad aplicada: jugador {indiceJugador}, {tipoHabilidad}");
        AccionAplicada?.Invoke(indiceJugador);
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

        long emisor = Multiplayer.GetRemoteSenderId();
        if (emisor == 0) emisor = 1;

        var controller = Controller.GetInstance();
        int indiceEmisor = DatosPartida.IdsJugadores.IndexOf(emisor);
        if (controller.JuegoTerminado || controller.TurnoEnTransicion || indiceEmisor != controller.JugadorActual) return;

        var jugador = controller.Jugadores()[indiceEmisor];
        if (jugador.RealizoAccionEsteTurno || !jugador.RerollDisponible) return;

        Rpc(MethodName.AplicarReroll, indiceEmisor);
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
    public void AplicarReroll(int indiceJugador)
    {
        Controller.GetInstance().RerollearManoActual();

        GD.Print($"Reroll aplicado: jugador {indiceJugador}");
        RerollAplicado?.Invoke(indiceJugador);
        AccionAplicada?.Invoke(indiceJugador);
    }
}