using Godot;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

// Datos de la sala que el host anuncia por la red local
public class SalaInfo
{
    public string Juego { get; set; } = "Figurama";
    public string Id { get; set; } = "";
    public string Nombre { get; set; } = "";
    public int Jugadores { get; set; }
    public int MaxJugadores { get; set; } = 4;
    public int Espectadores { get; set; }
    public int MaxEspectadores { get; set; } = 2;
    public int Puerto { get; set; } = 7777;
    public string Estado { get; set; } = "esperando";
}

// Una sala tal como la ve el cliente
public class SalaVista
{
    public SalaInfo Info { get; set; }
    public string Ip { get; set; } = "";
    public double SegundosSinAnuncio { get; set; }
}

public static class DescubrimientoSalas
{
    // Puertos de las partidas: cada host usa el primero que encuentre libre
    public const int PuertoJuegoBase = 7777;
    public const int CantidadPuertosJuego = 10;

    // Puertos de escucha de los anuncios: cada ventana cliente usa el primero libre
    public const int PuertoBase = 7800;
    public const int CantidadPuertos = 10;
}

// Lado del host: manda la información de la sala cada segundo
public class AnuncioSala
{
    private readonly PacketPeerUdp _udp = new PacketPeerUdp();
    private double _acumulado = 1.0; // el primer anuncio sale enseguida

    public AnuncioSala()
    {
        _udp.SetBroadcastEnabled(true);
    }

    public void Actualizar(double delta, SalaInfo info)
    {
        _acumulado += delta;
        if (_acumulado < 1.0) return;
        _acumulado = 0;

        byte[] datos = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(info));

        // Por loopback (otras ventanas de esta PC) y por broadcast (otras PCs de la red)
        string[] destinos = { "127.0.0.1", "255.255.255.255" };
        foreach (string destino in destinos)
        {
            for (int i = 0; i < DescubrimientoSalas.CantidadPuertos; i++)
            {
                _udp.SetDestAddress(destino, DescubrimientoSalas.PuertoBase + i);
                _udp.PutPacket(datos);
            }
        }
    }

    public void Detener()
    {
        _udp.Close();
    }
}

// Lado del cliente: escucha los anuncios y recuerda todas las salas vistas
public class BuscadorSalas
{
    private PacketPeerUdp _udp;
    private readonly Dictionary<string, SalaVista> _salas = new Dictionary<string, SalaVista>();

    public List<SalaVista> Salas { get; private set; } = new List<SalaVista>();
    public bool Escuchando => _udp != null;

    public void Iniciar()
    {
        if (_udp != null) return;

        for (int i = 0; i < DescubrimientoSalas.CantidadPuertos; i++)
        {
            var udp = new PacketPeerUdp();
            int puerto = DescubrimientoSalas.PuertoBase + i;

            if (udp.Bind(puerto) == Error.Ok)
            {
                _udp = udp;
                GD.Print($"[Buscador] Escuchando en el puerto {puerto}");
                return;
            }
        }

        GD.PrintErr("[Buscador] No se pudo abrir ningún puerto de escucha.");
    }

    public void Actualizar(double delta)
    {
        if (_udp == null) return;

        foreach (SalaVista v in _salas.Values)
        {
            v.SegundosSinAnuncio += delta;
        }

        while (_udp.GetAvailablePacketCount() > 0)
        {
            byte[] datos = _udp.GetPacket();
            string ip = _udp.GetPacketIP();

            try
            {
                var info = JsonSerializer.Deserialize<SalaInfo>(Encoding.UTF8.GetString(datos));
                if (info == null || info.Juego != "Figurama" || string.IsNullOrEmpty(info.Id)) continue;

                if (!_salas.TryGetValue(info.Id, out SalaVista vista))
                {
                    vista = new SalaVista();
                    _salas[info.Id] = vista;
                    GD.Print($"[Buscador] Sala encontrada: {info.Nombre} en {ip}:{info.Puerto}");
                }

                vista.Info = info;
                vista.Ip = ip;
                vista.SegundosSinAnuncio = 0;
            }
            catch (System.Exception)
            {
                // Paquete que no es de Figurama: se ignora
            }
        }

        // Las salas que dejaron de anunciarse se sacan de la lista
        var vencidas = new List<string>();
        foreach (var par in _salas)
        {
            if (par.Value.SegundosSinAnuncio > 3.0) vencidas.Add(par.Key);
        }
        foreach (string id in vencidas)
        {
            _salas.Remove(id);
        }

        var lista = new List<SalaVista>(_salas.Values);
        lista.Sort((a, b) => a.Info.Puerto.CompareTo(b.Info.Puerto));
        Salas = lista;
    }

    public void Detener()
    {
        if (_udp != null)
        {
            _udp.Close();
            _udp = null;
        }
        _salas.Clear();
        Salas = new List<SalaVista>();
    }
}