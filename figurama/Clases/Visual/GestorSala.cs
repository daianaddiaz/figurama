using Godot;
using System;
using System.Collections.Generic;

public enum RolRed { Jugador = 0, Espectador = 1 }
public enum EstadoSala { Esperando, EnCurso }

// Autoload: existe durante toda la sesión, en todas las PCs
public partial class GestorSala : Node
{
    public const int MaxJugadores = 4;
    public const int MaxEspectadores = 2;
    public const int MaxClientes = 5; // el host + 5 clientes = 6 conexiones
    public const double SegundosParaVolver = 60.0;

    public static GestorSala Instancia { get; private set; }

    // Eventos del servidor
    public event Action<long> JugadorAceptado;
    public event Action<long> JugadorSalio;
    public event Action<int> JugadorAusente;
    public event Action<int> JugadorVolvio;
    public event Action<int> JugadorExpulsado;

    // Eventos del cliente
    public event Action<int> RolAsignado;
    public event Action<string> EntradaRechazada;

    public EstadoSala Estado { get; private set; } = EstadoSala.Esperando;
    public bool SalaAbierta { get; private set; } = false;

    private readonly Dictionary<long, RolRed> _roles = new Dictionary<long, RolRed>();
    private readonly Dictionary<int, double> _ausentes = new Dictionary<int, double>();
    private AnuncioSala _anuncio;
    private string _nombreSala = "";
    private string _idSala = "";
    private int _puerto = DescubrimientoSalas.PuertoJuegoBase;

    public int CantidadJugadores => CantidadConRol(RolRed.Jugador);
    public int CantidadEspectadores => CantidadConRol(RolRed.Espectador);

    public bool EsJugador(long id)
    {
        return _roles.TryGetValue(id, out RolRed rol) && rol == RolRed.Jugador;
    }

    public bool EstaAusente(int indice)
    {
        return _ausentes.ContainsKey(indice);
    }

    // Jugadores ausentes y los segundos que les quedan para volver
    public void ObtenerAusencias(out int[] indices, out int[] segundos)
    {
        indices = new int[_ausentes.Count];
        segundos = new int[_ausentes.Count];

        int i = 0;
        foreach (var par in _ausentes)
        {
            indices[i] = par.Key;
            segundos[i] = (int)Math.Ceiling(Math.Max(0.0, par.Value));
            i++;
        }
    }

    private int CantidadConRol(RolRed rol)
    {
        int cantidad = 0;
        foreach (var par in _roles)
        {
            if (par.Value == rol) cantidad++;
        }
        return cantidad;
    }

    public override void _Ready()
    {
        Instancia = this;
        Multiplayer.PeerConnected += OnPeerConectado;
        Multiplayer.PeerDisconnected += OnPeerDesconectado;
        Multiplayer.ConnectedToServer += OnConectadoAlServidor;
    }

    public override void _Process(double delta)
    {
        if (!SalaAbierta) return;

        // Si el host cerró la conexión (por ejemplo, volvió al menú), la sala se cierra sola
        if (Multiplayer.MultiplayerPeer == null || Multiplayer.MultiplayerPeer is OfflineMultiplayerPeer)
        {
            CerrarSala();
            return;
        }

        _anuncio?.Actualizar(delta, ConstruirInfo());
        ActualizarAusencias(delta);
    }

    // ---------- Servidor: abrir y cerrar la sala ----------

    public void AbrirSala(string nombre, int puerto)
    {
        _roles.Clear();
        _ausentes.Clear();
        _roles[1] = RolRed.Jugador; // el host siempre es jugador

        _nombreSala = nombre;
        _idSala = Guid.NewGuid().ToString();
        _puerto = puerto;
        Estado = EstadoSala.Esperando;
        SalaAbierta = true;
        DatosPartida.NombreSala = nombre;

        _anuncio?.Detener();
        _anuncio = new AnuncioSala();
        GD.Print($"[Sala] Abierta: {_nombreSala} (puerto {_puerto})");
    }

    public void CerrarSala()
    {
        if (_anuncio != null)
        {
            _anuncio.Detener();
            _anuncio = null;
        }

        if (SalaAbierta) GD.Print("[Sala] Cerrada.");

        _roles.Clear();
        _ausentes.Clear();
        SalaAbierta = false;
        Estado = EstadoSala.Esperando;
    }

    public void MarcarEnCurso()
    {
        Estado = EstadoSala.EnCurso;
    }

    private SalaInfo ConstruirInfo()
    {
        return new SalaInfo
        {
            Id = _idSala,
            Nombre = _nombreSala,
            Jugadores = CantidadJugadores + _ausentes.Count,
            MaxJugadores = MaxJugadores,
            Espectadores = CantidadEspectadores,
            MaxEspectadores = MaxEspectadores,
            Puerto = _puerto,
            Estado = Estado == EstadoSala.Esperando ? "esperando" : "en curso"
        };
    }

    // ---------- Servidor: tiempo de gracia de los jugadores ausentes ----------

    private void ActualizarAusencias(double delta)
    {
        if (_ausentes.Count == 0) return;

        var vencidos = new List<int>();
        foreach (int indice in new List<int>(_ausentes.Keys))
        {
            _ausentes[indice] -= delta;
            if (_ausentes[indice] <= 0) vencidos.Add(indice);
        }

        foreach (int indice in vencidos)
        {
            _ausentes.Remove(indice);
            GD.Print($"[Sala] Jugador {indice} eliminado por ausencia.");
            JugadorExpulsado?.Invoke(indice);
        }
    }

    // ---------- Servidor: decidir el rol de cada conexión ----------

    private void OnPeerConectado(long id)
    {
        if (!SalaAbierta || !Multiplayer.IsServer()) return;

        if (Estado == EstadoSala.Esperando)
        {
            if (CantidadJugadores < MaxJugadores) Aceptar(id, RolRed.Jugador);
            else Rechazar(id, "Sala llena.");
            return;
        }

        // Partida en curso: el cliente tiene que identificarse (¿vuelve un jugador o es un espectador?)
        EsperarIdentificacion(id);
    }

    private async void EsperarIdentificacion(long id)
    {
        await ToSignal(GetTree().CreateTimer(5.0), SceneTreeTimer.SignalName.Timeout);

        if (!SalaAbierta || _roles.ContainsKey(id)) return;

        if (Multiplayer.MultiplayerPeer != null && Array.IndexOf(Multiplayer.GetPeers(), (int)id) >= 0)
        {
            GD.Print($"[Sala] {id} no se identificó a tiempo: se lo desconecta.");
            Multiplayer.MultiplayerPeer.DisconnectPeer((int)id);
        }
    }

    private void OnPeerDesconectado(long id)
    {
        if (!SalaAbierta || !Multiplayer.IsServer()) return;
        if (!_roles.TryGetValue(id, out RolRed rol)) return;

        _roles.Remove(id);
        if (rol != RolRed.Jugador) return;

        if (Estado == EstadoSala.Esperando)
        {
            JugadorSalio?.Invoke(id);
            return;
        }

        // Partida en curso: el jugador queda ausente y tiene un tiempo para volver
        int indice = DatosPartida.IdsJugadores.IndexOf(id);
        if (indice < 0) return;

        DatosPartida.IdsJugadores[indice] = 0;
        _ausentes[indice] = SegundosParaVolver;
        GD.Print($"[Sala] Jugador {indice} ausente: tiene {SegundosParaVolver} s para volver.");
        JugadorAusente?.Invoke(indice);
    }

    private int[] PersonajesComoInt()
    {
        var personajes = new int[DatosPartida.PersonajesElegidos.Count];
        for (int i = 0; i < personajes.Length; i++)
        {
            personajes[i] = (int)DatosPartida.PersonajesElegidos[i];
        }
        return personajes;
    }

    private void Aceptar(long id, RolRed rol)
    {
        _roles[id] = rol;
        GD.Print($"[Sala] {id} entró como {rol}");

        if (rol == RolRed.Jugador)
        {
            RpcId(id, MethodName.RecibirRol, (int)rol, _idSala, _nombreSala);
            JugadorAceptado?.Invoke(id);
            return;
        }

        // Espectador: se le manda lo necesario para armar la partida en su PC
        RpcId(id, MethodName.EntrarComoEspectador,
            DatosPartida.NombresJugadores.ToArray(),
            PersonajesComoInt(),
            DatosPartida.IdsJugadores.ToArray(),
            _nombreSala);
    }

    private void Reconectar(long id, int indice)
    {
        _ausentes.Remove(indice);
        _roles[id] = RolRed.Jugador;
        DatosPartida.IdsJugadores[indice] = id;
        GD.Print($"[Sala] {id} volvió a la partida como jugador {indice}.");

        RpcId(id, MethodName.EntrarComoJugador,
            DatosPartida.NombresJugadores.ToArray(),
            PersonajesComoInt(),
            DatosPartida.IdsJugadores.ToArray(),
            indice,
            _nombreSala,
            _idSala);

        JugadorVolvio?.Invoke(indice);
    }

    private async void Rechazar(long id, string motivo)
    {
        GD.Print($"[Sala] Entrada rechazada ({id}): {motivo}");
        RpcId(id, MethodName.Rechazado, motivo);

        // Un momento para que el mensaje llegue antes de cortar la conexión
        await ToSignal(GetTree().CreateTimer(0.5), SceneTreeTimer.SignalName.Timeout);

        if (Multiplayer.MultiplayerPeer != null && Array.IndexOf(Multiplayer.GetPeers(), (int)id) >= 0)
        {
            Multiplayer.MultiplayerPeer.DisconnectPeer((int)id);
        }
    }

    // El cliente se identifica con su código apenas se conecta
    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    public void Identificarse(string token)
    {
        if (!SalaAbierta || !Multiplayer.IsServer()) return;
        if (Estado != EstadoSala.EnCurso) return;

        long id = Multiplayer.GetRemoteSenderId();
        if (id == 0 || _roles.ContainsKey(id)) return;

        int indice = DatosPartida.TokensJugadores.IndexOf(token);
        if (indice >= 0 && _ausentes.ContainsKey(indice))
        {
            Reconectar(id, indice);
            return;
        }

        if (CantidadEspectadores < MaxEspectadores) Aceptar(id, RolRed.Espectador);
        else Rechazar(id, "Sin lugar para espectar.");
    }

    // ---------- Cliente ----------

    private void OnConectadoAlServidor()
    {
        RpcId(1, MethodName.Identificarse, DatosPartida.TokenLocal);
    }

    [Rpc(MultiplayerApi.RpcMode.Authority)]
    public void RecibirRol(int rol, string idSala, string nombreSala)
    {
        DatosPartida.SalaReconexionId = idSala;
        DatosPartida.NombreSala = nombreSala;
        RolAsignado?.Invoke(rol);
    }

    [Rpc(MultiplayerApi.RpcMode.Authority)]
    public void Rechazado(string motivo)
    {
        EntradaRechazada?.Invoke(motivo);
    }

    [Rpc(MultiplayerApi.RpcMode.Authority)]
    public void EntrarComoEspectador(string[] nombres, int[] personajes, long[] ids, string nombreSala)
    {
        DatosPartida.NombresJugadores = new List<string>(nombres);
        DatosPartida.PersonajesElegidos = new List<TipoHabilidad>();
        foreach (int p in personajes)
        {
            DatosPartida.PersonajesElegidos.Add((TipoHabilidad)p);
        }

        DatosPartida.IdsJugadores = new List<long>(ids);
        DatosPartida.IndiceJugadorLocal = -1;
        DatosPartida.EsMultijugador = true;
        DatosPartida.EsEspectador = true;
        DatosPartida.NombreSala = nombreSala;

        Controller.GetInstance().PrepararReplica();
        GetTree().ChangeSceneToFile("res://Objetos/tableroMP.tscn");
    }

    // Un jugador que vuelve a la partida: recupera su lugar y el estado actual
    [Rpc(MultiplayerApi.RpcMode.Authority)]
    public void EntrarComoJugador(string[] nombres, int[] personajes, long[] ids, int indice, string nombreSala, string idSala)
    {
        DatosPartida.NombresJugadores = new List<string>(nombres);
        DatosPartida.PersonajesElegidos = new List<TipoHabilidad>();
        foreach (int p in personajes)
        {
            DatosPartida.PersonajesElegidos.Add((TipoHabilidad)p);
        }

        DatosPartida.IdsJugadores = new List<long>(ids);
        DatosPartida.IndiceJugadorLocal = indice;
        DatosPartida.EsMultijugador = true;
        DatosPartida.EsEspectador = false;
        DatosPartida.NombreSala = nombreSala;
        DatosPartida.SalaReconexionId = idSala;

        Controller.GetInstance().PrepararReplica();
        GetTree().ChangeSceneToFile("res://Objetos/tableroMP.tscn");
    }
}