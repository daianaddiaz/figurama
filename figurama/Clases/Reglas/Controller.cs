using System;
using System.Collections.Generic;

public partial class Controller
{
    public static Controller _instance;

    public List<string> NombresJugadores { get; set; } = new List<string>();
    public int CantidadCartasMovimiento { get; set; } = 3;
    public int CantidadFigurasPorJugador { get; set; } = 3;

    private TableroReglas tablero;
    private Jugador[] jugadores;
    private Juego juego;
    private bool condicionDeVictoria = false;

    public int JugadorActual { get; private set; } = 0;
    public CartaMovimiento CartaSeleccionada { get; set; } = null;
    public event System.Action<int> TurnoCambiado;
    public event System.Action<string> Victoria;

    public bool JuegoTerminado => condicionDeVictoria;

    private Controller() { }

    public static Controller GetInstance()
    {
        if (_instance == null)
        {
            _instance = new Controller();
            _instance.InicializarController();
        }
        return _instance;
    }

    public void InicializarController()
    {
        tablero = new TableroReglas();
        juego = new Juego();
    }

    public bool RerollearManoActual()
    {
        if (condicionDeVictoria) return false;
        if (jugadores == null || jugadores.Length == 0) return false;

        Jugador jugador = jugadores[JugadorActual];
        if (!jugador.RerollDisponible) return false;

        jugador.manoCartas = MazoMovimiento.GetInstance().generarMano();
        jugador.RerollDisponible = false;
        CartaSeleccionada = null;

        TerminarTurno();
        return true;
    }

    public void InicializarJugadores()
    {
        TurnoCambiado = null;
        Victoria = null;

        int cantidad = NombresJugadores.Count > 0 ? NombresJugadores.Count : 4;
        jugadores = new Jugador[cantidad];

        for (int i = 0; i < cantidad; i++)
        {
            string nombre = i < NombresJugadores.Count ? NombresJugadores[i] : $"Jugador {i + 1}";

            var figuras = new List<FiguraAsignada>();
            foreach (CartaFigura figura in MazoFiguras.GetInstance().generarMano(CantidadFigurasPorJugador))
            {
                figuras.Add(new FiguraAsignada { Figura = figura, Completada = false });
            }

            jugadores[i] = new Jugador
            {
                nombre = nombre,
                manoCartas = MazoMovimiento.GetInstance().generarMano(),
                figurasAArmar = figuras
            };
        }

        JugadorActual = 0;
        condicionDeVictoria = false;
    }

    public void CambiarCartaSeleccionada(CartaMovimiento carta)
    {
        CartaSeleccionada = carta;
    }

    public Jugador[] Jugadores() => jugadores;

    public string NombreJugadorActual() => jugadores != null && jugadores.Length > JugadorActual ? jugadores[JugadorActual].nombre : "";

    public List<CartaMovimiento> ManoJugadorActual() => jugadores != null && jugadores.Length > JugadorActual ? jugadores[JugadorActual].manoCartas : new List<CartaMovimiento>();

    public List<FiguraAsignada> FigurasJugadorActual() => jugadores != null && jugadores.Length > JugadorActual ? jugadores[JugadorActual].figurasAArmar : new List<FiguraAsignada>();

    public CartaMovimiento MovimientoActual() => CartaSeleccionada;

    public List<CartaFigura> ChequearFigurasCompletadas(TableroReglas tableroVisual, HashSet<(int fila, int columna)> celdasMovidas)
    {
        var completadasAhora = new List<CartaFigura>();
        Jugador jugador = jugadores[JugadorActual];

        foreach (FiguraAsignada asignada in jugador.figurasAArmar)
        {
            if (asignada.Completada) continue;

            if (tableroVisual.BuscarFigura(asignada.Figura, celdasMovidas) != null)
            {
                asignada.Completada = true;
                jugador.Puntuacion += asignada.Figura.CantidadFichas;
                completadasAhora.Add(asignada.Figura);
            }
        }

        if (jugador.figurasAArmar.TrueForAll(f => f.Completada))
        {
            condicionDeVictoria = true;
            Victoria?.Invoke(jugador.nombre);
        }

        return completadasAhora;
    }

    public void TerminarTurno()
    {
        if (condicionDeVictoria || jugadores == null || jugadores.Length == 0) return;

        JugadorActual = (JugadorActual + 1) % jugadores.Length;
        jugadores[JugadorActual].RerollDisponible = true;
        TurnoCambiado?.Invoke(JugadorActual);
    }
}