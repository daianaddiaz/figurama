using Godot;
using System.Collections.Generic;

public partial class LobbyRed : VBoxContainer
{
    // Mismo orden que el enum TipoHabilidad
    private static readonly string[] NombresPersonajes = { "Lobizón", "Luz Mala", "El Pomberito", "Mulánima" };

    private Button _botonHost;
    private Label _labelSala;
    private Label _labelMensaje;
    private VBoxContainer _listaSalas;
    private LineEdit _campoIP;
    private Button _botonUnirse;
    private LineEdit _campoNombre;
    private OptionButton _selectorPersonaje;
    private OptionButton _selectorDificultad;
    private Button _botonListo;
    private VBoxContainer _listaJugadores;
    private Button _botonIniciar;
    private MultiplayerSpawner _spawner;

    private readonly BuscadorSalas _buscador = new BuscadorSalas();
    private string _firmaSalas = "";
    private bool _enRed = false;
    private int _versionMensaje = 0;

    // Solo los usa el host
    private readonly List<long> _ordenIds = new List<long>();
    private readonly Dictionary<long, string> _nombres = new Dictionary<long, string>();
    private readonly Dictionary<long, int> _personajes = new Dictionary<long, int>();
    private readonly Dictionary<long, string> _tokens = new Dictionary<long, string>();

    public override void _Ready()
    {
        if (GestorSala.Instancia == null)
        {
            GD.PrintErr("[LobbyRed] Falta registrar GestorSala como Autoload.");
        }

        _botonHost = GetNode<Button>("BotonHost");
        _labelSala = GetNode<Label>("LabelSala");
        _listaSalas = GetNode<VBoxContainer>("ListaSalas");
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

        // Mensajes de error, debajo del título
        _labelMensaje = new Label { Visible = false };
        _labelMensaje.AddThemeColorOverride("font_color", new Color(1f, 0.45f, 0.45f));
        AddChild(_labelMensaje);
        MoveChild(_labelMensaje, 1);

        ConfigurarSpawner();
        EstadoInicial();

        _botonHost.Pressed += CrearHost;
        _botonUnirse.Pressed += OnUnirsePorIpPresionado;
        _botonListo.Pressed += OnListoPresionado;
        _botonIniciar.Pressed += OnIniciarPresionado;

        Multiplayer.ConnectedToServer += AlConectarseAlServidor;
        Multiplayer.ConnectionFailed += AlFallarConexion;
        Multiplayer.ServerDisconnected += AlPerderServidor;

        GestorSala.Instancia.JugadorAceptado += AlJugadorAceptado;
        GestorSala.Instancia.JugadorSalio += AlJugadorSalio;
        GestorSala.Instancia.RolAsignado += AlRolAsignado;
        GestorSala.Instancia.EntradaRechazada += AlEntradaRechazada;

        VisibilityChanged += OnVisibilidadCambiada;
    }

    public override void _ExitTree()
    {
        Multiplayer.ConnectedToServer -= AlConectarseAlServidor;
        Multiplayer.ConnectionFailed -= AlFallarConexion;
        Multiplayer.ServerDisconnected -= AlPerderServidor;

        if (GestorSala.Instancia != null)
        {
            GestorSala.Instancia.JugadorAceptado -= AlJugadorAceptado;
            GestorSala.Instancia.JugadorSalio -= AlJugadorSalio;
            GestorSala.Instancia.RolAsignado -= AlRolAsignado;
            GestorSala.Instancia.EntradaRechazada -= AlEntradaRechazada;
        }

        _buscador.Detener();
    }

    public override void _Process(double delta)
    {
        if (_buscador.Escuchando)
        {
            _buscador.Actualizar(delta);
            ActualizarListaSalas();
        }
    }

    // ---------- Mensajes ----------

    private async void MostrarMensaje(string texto)
    {
        int version = ++_versionMensaje;
        _labelMensaje.Text = texto;
        _labelMensaje.Visible = true;

        await ToSignal(GetTree().CreateTimer(4.0), SceneTreeTimer.SignalName.Timeout);

        if (IsInstanceValid(_labelMensaje) && version == _versionMensaje)
        {
            _labelMensaje.Visible = false;
        }
    }

    // ---------- Lista de salas ----------

    private void OnVisibilidadCambiada()
    {
        if (IsVisibleInTree() && !_enRed)
        {
            _buscador.Iniciar();
        }
        else if (!IsVisibleInTree())
        {
            _buscador.Detener();
        }
    }

    private void LimpiarListaSalas()
    {
        foreach (Node hijo in _listaSalas.GetChildren())
        {
            _listaSalas.RemoveChild(hijo);
            hijo.QueueFree();
        }
        _firmaSalas = "";
    }

    // Solo reconstruye las filas cuando algo cambió (si no, se perderían los clics)
    private void ActualizarListaSalas()
    {
        var salas = _buscador.Salas;

        var firma = new System.Text.StringBuilder();
        foreach (SalaVista s in salas)
        {
            firma.Append($"{s.Info.Id}|{s.Info.Nombre}|{s.Info.Jugadores}|{s.Info.Espectadores}|{s.Info.Estado}|{s.Ip}|{s.Info.Puerto};");
        }

        string nuevaFirma = firma.ToString();
        if (nuevaFirma == _firmaSalas) return;

        LimpiarListaSalas();
        _firmaSalas = nuevaFirma;

        _labelSala.Text = salas.Count == 0 ? "Buscando salas..." : "Salas disponibles:";

        foreach (SalaVista sala in salas)
        {
            string ip = sala.Ip;
            int puerto = sala.Info.Puerto;
            bool enCurso = sala.Info.Estado == "en curso";

            string texto = $"{sala.Info.Nombre}  |  Jugadores {sala.Info.Jugadores}/{sala.Info.MaxJugadores}  |  {sala.Info.Estado}";
            string textoBoton;
            bool deshabilitado;

            if (!enCurso)
            {
                bool llena = sala.Info.Jugadores >= sala.Info.MaxJugadores;
                textoBoton = llena ? "Sala llena" : "Conectar";
                deshabilitado = llena;
            }
            else
            {
                texto += $"  |  Espectadores {sala.Info.Espectadores}/{sala.Info.MaxEspectadores}";

                if (sala.Info.Id == DatosPartida.SalaReconexionId)
                {
                    // Era mi partida: el servidor decide si todavía llego a tiempo
                    textoBoton = "Reconectar";
                    deshabilitado = false;
                }
                else
                {
                    bool sinLugar = sala.Info.Espectadores >= sala.Info.MaxEspectadores;
                    textoBoton = sinLugar ? "Sin lugar para espectar" : "Espectar";
                    deshabilitado = sinLugar;
                }
            }

            var fila = new HBoxContainer();
            fila.AddChild(new Label { Text = texto, SizeFlagsHorizontal = Control.SizeFlags.ExpandFill });

            var boton = new Button { Text = textoBoton, Disabled = deshabilitado };
            boton.Pressed += () => ConectarA(ip, puerto);
            fila.AddChild(boton);

            _listaSalas.AddChild(fila);
        }
    }

    private void OnUnirsePorIpPresionado()
    {
        // Respaldo manual: "ip" (puerto 7777) o "ip:puerto"
        string texto = _campoIP.Text.Trim();
        int puerto = DescubrimientoSalas.PuertoJuegoBase;

        int separador = texto.LastIndexOf(':');
        if (separador > 0 && int.TryParse(texto.Substring(separador + 1), out int puertoEscrito))
        {
            puerto = puertoEscrito;
            texto = texto.Substring(0, separador);
        }

        ConectarA(texto, puerto);
    }

    // ---------- Spawner de la lista de jugadores ----------

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

    // ---------- Estado de la interfaz ----------

    private void EstadoInicial()
    {
        _botonHost.Disabled = false;
        LimpiarListaSalas();
        _listaSalas.Visible = true;
        _labelSala.Text = "Buscando salas...";
        _campoIP.Editable = true;
        _botonUnirse.Disabled = false;
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
        _listaSalas.Visible = false;
        _botonUnirse.Disabled = true;
        _campoIP.Editable = false;
    }

    // ---------- Crear sala y conectarse ----------

    private void CrearHost()
    {
        // Cada host usa el primer puerto libre, así pueden convivir varias salas
        ENetMultiplayerPeer peer = null;
        int puertoElegido = -1;

        for (int i = 0; i < DescubrimientoSalas.CantidadPuertosJuego; i++)
        {
            int puerto = DescubrimientoSalas.PuertoJuegoBase + i;
            var candidato = new ENetMultiplayerPeer();

            if (candidato.CreateServer(puerto, GestorSala.MaxClientes) == Error.Ok)
            {
                peer = candidato;
                puertoElegido = puerto;
                break;
            }
        }

        if (peer == null)
        {
            MostrarMensaje("No se pudo crear la sala: no hay puertos libres.");
            return;
        }

        Multiplayer.MultiplayerPeer = peer;
        _enRed = true;
        _spawner.Spawn(1L);

        string nombre = string.IsNullOrWhiteSpace(_campoNombre.Text) ? "Jugador" : _campoNombre.Text;
        string nombreSala = $"Sala de {nombre}";

        _buscador.Detener();
        GestorSala.Instancia.AbrirSala(nombreSala, puertoElegido);

        BloquearConexion();
        _labelSala.Text = nombreSala;
        _botonListo.Disabled = false;
        _selectorDificultad.Visible = true;
        _botonIniciar.Visible = true;
    }

    private void ConectarA(string ip, int puerto)
    {
        var peer = new ENetMultiplayerPeer();
        Error error = peer.CreateClient(ip, puerto);
        if (error != Error.Ok)
        {
            MostrarMensaje($"No se pudo conectar a {ip}:{puerto}.");
            return;
        }

        Multiplayer.MultiplayerPeer = peer;
        _enRed = true;
        _buscador.Detener();

        BloquearConexion();
        _labelSala.Text = "Conectando...";
    }

    private void AlConectarseAlServidor()
    {
        _labelSala.Text = "Conectado, esperando confirmación...";
    }

    private void AlFallarConexion()
    {
        Desconectar();
        MostrarMensaje("No se pudo conectar a la sala.");
    }

    private void AlPerderServidor()
    {
        if (!_enRed) return;

        Desconectar();
        MostrarMensaje("Se perdió la conexión con el host.");
    }

    // ---------- Decisión del servidor ----------

    private void AlRolAsignado(int rol)
    {
        if (rol == (int)RolRed.Jugador)
        {
            _labelSala.Text = "Conectado a la sala";
            _botonListo.Disabled = false;
        }
        else
        {
            Desconectar();
            MostrarMensaje("No se pudo entrar como jugador.");
        }
    }

    private void AlEntradaRechazada(string motivo)
    {
        Desconectar();
        MostrarMensaje(motivo);
    }

    private void AlJugadorAceptado(long id)
    {
        _spawner.Spawn(id);
        ActualizarBotonIniciar();
    }

    private void AlJugadorSalio(long id)
    {
        _ordenIds.Remove(id);
        _nombres.Remove(id);
        _personajes.Remove(id);
        _tokens.Remove(id);
        _listaJugadores.GetNodeOrNull(id.ToString())?.QueueFree();
        ActualizarBotonIniciar();
    }

    // ---------- Datos de cada jugador e inicio de partida ----------

    private void OnListoPresionado()
    {
        _botonListo.Disabled = true;
        _campoNombre.Editable = false;
        _selectorPersonaje.Disabled = true;

        string nombre = string.IsNullOrWhiteSpace(_campoNombre.Text) ? "Jugador" : _campoNombre.Text;
        RpcId(1, MethodName.RecibirDatosJugador, nombre, _selectorPersonaje.Selected, DatosPartida.TokenLocal);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public void RecibirDatosJugador(string nombre, int personaje, string token)
    {
        if (!Multiplayer.IsServer()) return;

        long id = Multiplayer.GetRemoteSenderId();
        if (id == 0) id = 1; // llamada local del host

        // Solo los jugadores aceptados por el servidor pueden registrar sus datos
        if (!GestorSala.Instancia.EsJugador(id)) return;

        if (!_ordenIds.Contains(id)) _ordenIds.Add(id);
        _nombres[id] = nombre;
        _personajes[id] = personaje;
        _tokens[id] = token;

        ActualizarBotonIniciar();
    }

    private void ActualizarBotonIniciar()
    {
        if (!_enRed || !Multiplayer.IsServer()) return;

        int jugadores = GestorSala.Instancia.CantidadJugadores;
        _botonIniciar.Disabled = !(jugadores >= 2 && _ordenIds.Count == jugadores);
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
        var tokens = new List<string>();
        for (int i = 0; i < ids.Count; i++)
        {
            nombres[i] = _nombres[ids[i]];
            personajes[i] = _personajes[ids[i]];
            tokens.Add(_tokens.TryGetValue(ids[i], out string token) ? token : "");
        }

        // Los códigos de identidad los guarda solo el servidor (no se mandan a los clientes)
        DatosPartida.TokensJugadores = tokens;

        int[] cartasPorDificultad = { 4, 3, 2 };
        int cartasPorMano = cartasPorDificultad[_selectorDificultad.Selected];

        Rpc(MethodName.IniciarPartida, nombres, personajes, ids.ToArray(), cartasPorMano);
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
    public void IniciarPartida(string[] nombres, int[] personajes, long[] ids, int cartasPorMano)
    {
        DatosPartida.NombresJugadores = new List<string>(nombres);
        DatosPartida.PersonajesElegidos = new List<TipoHabilidad>();
        foreach (int p in personajes)
        {
            DatosPartida.PersonajesElegidos.Add((TipoHabilidad)p);
        }

        DatosPartida.IndiceJugadorLocal = System.Array.IndexOf(ids, (long)Multiplayer.GetUniqueId());
        DatosPartida.EsMultijugador = true;
        DatosPartida.EsEspectador = false;
        DatosPartida.IdsJugadores = new List<long>(ids);

        Controller.GetInstance().CantidadCartasMovimiento = cartasPorMano;

        if (Multiplayer.IsServer())
        {
            // La sala pasa a "en curso" y el servidor sortea y ejecuta las reglas
            GestorSala.Instancia.MarcarEnCurso();
            Azar.Reiniciar();
            Controller.GetInstance().InicializarJugadores();

            foreach (Jugador j in Controller.GetInstance().Jugadores())
            {
                string mano = string.Join(",", j.manoCartas.ConvertAll(c => c.Nombre));
                string figuras = string.Join(",", j.figurasAArmar.ConvertAll(f => f.Figura.Nombre));
                string reserva = string.Join(",", j.figurasEnReserva.ConvertAll(f => f.Figura.Nombre));
                GD.Print($"[{j.nombre}] Mano: {mano} | Figuras: {figuras} | Reserva: {reserva}");
            }
        }
        else
        {
            // Los clientes no sortean: esperan el estado que manda el servidor
            Controller.GetInstance().PrepararReplica();
        }

        GetTree().ChangeSceneToFile("res://Objetos/tableroMP.tscn");
    }

    // ---------- Salir del lobby ----------

    public void Desconectar()
    {
        _enRed = false;
        GestorSala.Instancia.CerrarSala();

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
        _tokens.Clear();
        EstadoInicial();

        if (IsVisibleInTree())
        {
            _buscador.Iniciar();
        }
    }
}