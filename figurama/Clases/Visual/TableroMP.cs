using Godot;
using System.Collections.Generic;

public partial class TableroMP : Node3D
{
    [Export] public PackedScene FichaScene;
    [Export] public PackedScene JugadorScene;

    [Signal] public delegate void FiguraCompletadaEventHandler(Ficha ficha);

    public event System.Action<int> AccionAplicada;

    private const float SizeCelda = 0.8f;

    private readonly TableroReglas _reglas = new TableroReglas();
    private readonly GestorFichas _gestorFichas = new GestorFichas();
    private Node _sonidos;
    private int _clientesListos = 0;
    private bool _tableroEnviado = false;

    public List<Ficha> Fichas { get; private set; } = new List<Ficha>();
    public TableroReglas Reglas => _reglas;

    public override void _Ready()
    {
        _sonidos = GetNode<Node>("Sonidos");

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
        if (Controller._instance != null)
        {
            Controller.GetInstance().FiguraEncontrada -= OnFiguraCompletada;
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
        foreach (Ficha ficha in Fichas)
        {
            Connect(SignalName.FiguraCompletada, new Callable(ficha, "_on_figura_completada"));
        }

        AddChild(JugadorScene.Instantiate<Node3D>());
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
        if (controller.JuegoTerminado || indiceEmisor != controller.JugadorActual) return;

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
        if (controller.JuegoTerminado || indiceEmisor != controller.JugadorActual) return;
        if (!controller.Jugadores()[indiceEmisor].RealizoAccionEsteTurno) return;

        Rpc(MethodName.AplicarFinTurno);
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
    public void AplicarFinTurno()
    {
        Controller.GetInstance().TerminarTurno();
    }


    public void SolicitarComodin(int fila, int columna)
    {
        //código para convertir una ficha en comodin con RPC
    }
}