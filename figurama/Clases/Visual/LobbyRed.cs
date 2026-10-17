using Godot;
using System.Collections.Generic;

public partial class LobbyRed : VBoxContainer
{
    private const int Puerto = 7777;
    private const int MaxClientes = 3; // mas el host = 4 jugadores

    // Mismo orden que el enum TipoHabilidad
    private static readonly string[] NombresPersonajes = { "Lobizón", "Luz Mala", "El Pomberito", "Mulánima" };

    private Button _botonHost;
    private LineEdit _campoIP;
    private Button _botonUnirse;
    private LineEdit _campoNombre;
    private OptionButton _selectorPersonaje;
    private OptionButton _selectorDificultad;
    private Button _botonListo;
    private VBoxContainer _listaJugadores;
    private Button _botonIniciar;
    private MultiplayerSpawner _spawner;

    // Solo los usa el host
    private readonly List<long> _ordenIds = new List<long>();
    private readonly Dictionary<long, string> _nombres = new Dictionary<long, string>();
    private readonly Dictionary<long, int> _personajes = new Dictionary<long, int>();

    public override void _Ready()
    {
        _botonHost = GetNode<Button>("BotonHost");
        _campoIP = GetNode<LineEdit>("CampoIP");
        _botonUnirse = GetNode<Button>("BotonUnirse");
        _campoNombre = GetNode<LineEdit>("CampoNombre");
        _selectorPersonaje = GetNode<OptionButton>("SelectorPersonaje");
        _botonListo = GetNode<Button>("BotonListo");
        _listaJugadores = GetNode<VBoxContainer>("ListaJugadores");
        _botonIniciar = GetNode<Button>("BotonIniciar");

        foreach (string nombrePersonaje in NombresPersonajes)
        {
            _selectorPersonaje.AddItem(nombrePersonaje);
        }
        _selectorPersonaje.Selected = 0;
        _selectorDificultad = GetNode<OptionButton>("SelectorDificultad");
        _selectorDificultad.AddItem("Fácil (4 cartas)");
        _selectorDificultad.AddItem("Normal (3 cartas)");
        _selectorDificultad.AddItem("Difícil (2 cartas)");
        _selectorDificultad.Selected = 1;

        ConfigurarSpawner();
        EstadoInicial();

        _botonHost.Pressed += CrearHost;
        _botonUnirse.Pressed += UnirseCliente;
        _botonListo.Pressed += OnListoPresionado;
        _botonIniciar.Pressed += OnIniciarPresionado;

        Multiplayer.PeerConnected += AlConectarPeer;
        Multiplayer.PeerDisconnected += AlDesconectarPeer;
        Multiplayer.ConnectedToServer += AlConectarseAlServidor;
        Multiplayer.ConnectionFailed += AlFallarConexion;
    }

    public override void _ExitTree()
    {
        Multiplayer.PeerConnected -= AlConectarPeer;
        Multiplayer.PeerDisconnected -= AlDesconectarPeer;
        Multiplayer.ConnectedToServer -= AlConectarseAlServidor;
        Multiplayer.ConnectionFailed -= AlFallarConexion;
    }

    private void ConfigurarSpawner()
    {
        _spawner = new MultiplayerSpawner();
        _spawner.Name = "SpawnerLobby";
        AddChild(_spawner);
        _spawner.SpawnPath = _spawner.GetPathTo(_listaJugadores);
        _spawner.SpawnFunction = new Callable(this, MethodName.AlSpawnearJugador);
    }

    private Node AlSpawnearJugador(Variant datos)
    {
        long id = datos.AsInt64();
        return new Label
        {
            Name = id.ToString(),
            Text = id == 1 ? "Jugador 1 (Host)" : $"Jugador {id}"
        };
    }

    private void EstadoInicial()
    {
        _botonHost.Disabled = false;
        _botonUnirse.Disabled = false;
        _campoIP.Editable = true;
        _campoNombre.Editable = true;
        _selectorPersonaje.Disabled = false;
        _botonListo.Disabled = true;
        _selectorDificultad.Visible = false;
        _botonIniciar.Visible = false;
        _botonIniciar.Disabled = true;
    }

    private void BloquearConexion()
    {
        _botonHost.Disabled = true;
        _botonUnirse.Disabled = true;
        _campoIP.Editable = false;
    }

    private void CrearHost()
    {
        var peer = new ENetMultiplayerPeer();
        Error error = peer.CreateServer(Puerto, MaxClientes);
        if (error != Error.Ok)
        {
            GD.PrintErr($"No se pudo crear el servidor: {error}");
            return;
        }

        Multiplayer.MultiplayerPeer = peer;
        _spawner.Spawn(1L);

        BloquearConexion();
        _botonListo.Disabled = false;
        _selectorDificultad.Visible = true;
        _botonIniciar.Visible = true;
    }

    private void UnirseCliente()
    {
        var peer = new ENetMultiplayerPeer();
        Error error = peer.CreateClient(_campoIP.Text, Puerto);
        if (error != Error.Ok)
        {
            GD.PrintErr($"No se pudo conectar: {error}");
            return;
        }

        Multiplayer.MultiplayerPeer = peer;
        BloquearConexion();
    }

    private void AlConectarseAlServidor()
    {
        _botonListo.Disabled = false;
    }

    private void AlFallarConexion()
    {
        GD.PrintErr("No se pudo conectar al host.");
        Desconectar();
    }

    private void AlConectarPeer(long id)
    {
        if (Multiplayer.IsServer())
        {
            _spawner.Spawn(id);
            ActualizarBotonIniciar();
        }
    }

    private void AlDesconectarPeer(long id)
    {
        if (!Multiplayer.IsServer()) return;

        _ordenIds.Remove(id);
        _nombres.Remove(id);
        _personajes.Remove(id);
        _listaJugadores.GetNodeOrNull(id.ToString())?.QueueFree();
        ActualizarBotonIniciar();
    }

    private void OnListoPresionado()
    {
        _botonListo.Disabled = true;
        _campoNombre.Editable = false;
        _selectorPersonaje.Disabled = true;

        string nombre = string.IsNullOrWhiteSpace(_campoNombre.Text) ? "Jugador" : _campoNombre.Text;
        RpcId(1, MethodName.RecibirDatosJugador, nombre, _selectorPersonaje.Selected);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public void RecibirDatosJugador(string nombre, int personaje)
    {
        if (!Multiplayer.IsServer()) return;

        long id = Multiplayer.GetRemoteSenderId();
        if (id == 0) id = 1; // llamada local del host

        if (!_ordenIds.Contains(id)) _ordenIds.Add(id);
        _nombres[id] = nombre;
        _personajes[id] = personaje;

        ActualizarBotonIniciar();
    }

    private void ActualizarBotonIniciar()
    {
        if (!Multiplayer.IsServer()) return;

        int conectados = Multiplayer.GetPeers().Length + 1;
        _botonIniciar.Disabled = !(conectados >= 2 && _ordenIds.Count == conectados);
    }

    private void OnIniciarPresionado()
    {
        var ids = new List<long>();
        if (_ordenIds.Contains(1)) ids.Add(1);
        foreach (long id in _ordenIds)
        {
            if (id != 1) ids.Add(id);
        }

        var nombres = new string[ids.Count];
        var personajes = new int[ids.Count];
        for (int i = 0; i < ids.Count; i++)
        {
            nombres[i] = _nombres[ids[i]];
            personajes[i] = _personajes[ids[i]];
        }

        int[] cartasPorDificultad = { 4, 3, 2 };
        int cartasPorMano = cartasPorDificultad[_selectorDificultad.Selected];
        int semilla = (int)(GD.Randi() % 2147483647u);

        Rpc(MethodName.IniciarPartida, nombres, personajes, ids.ToArray(), semilla, cartasPorMano);
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
    public void IniciarPartida(string[] nombres, int[] personajes, long[] ids, int semilla, int cartasPorMano)
    {
        DatosPartida.NombresJugadores = new List<string>(nombres);
        DatosPartida.PersonajesElegidos = new List<TipoHabilidad>();
        foreach (int p in personajes)
        {
            DatosPartida.PersonajesElegidos.Add((TipoHabilidad)p);
        }

        DatosPartida.IndiceJugadorLocal = System.Array.IndexOf(ids, (long)Multiplayer.GetUniqueId());
        DatosPartida.EsMultijugador = true;
        DatosPartida.IdsJugadores = new List<long>(ids);

        Azar.Iniciar(semilla);
        Controller.GetInstance().CantidadCartasMovimiento = cartasPorMano;
        Controller.GetInstance().InicializarJugadores();

        foreach (Jugador j in Controller.GetInstance().Jugadores())
        {
            string mano = string.Join(",", j.manoCartas.ConvertAll(c => c.Nombre));
            string figuras = string.Join(",", j.figurasAArmar.ConvertAll(f => f.Figura.Nombre));
            string reserva = string.Join(",", j.figurasEnReserva.ConvertAll(f => f.Figura.Nombre));
            GD.Print($"[{j.nombre}] Mano: {mano} | Figuras: {figuras} | Reserva: {reserva}");
        }

        GetTree().ChangeSceneToFile("res://Objetos/tableroMP.tscn");
    }
    public void Desconectar()
    {
        if (Multiplayer.MultiplayerPeer != null)
        {
            Multiplayer.MultiplayerPeer.Close();
        }
        Multiplayer.MultiplayerPeer = new OfflineMultiplayerPeer();

        foreach (Node hijo in _listaJugadores.GetChildren())
        {
            hijo.QueueFree();
        }

        _ordenIds.Clear();
        _nombres.Clear();
        _personajes.Clear();
        EstadoInicial();
    }
}