using System;
using System.Collections.Generic;


public partial class Controller
{
    // Constructor de Singleton
    private Controller() {InicializarController();}

    public static Controller _instance;

    // Nombres desde el Menu
    public List<string> NombresJugadores { get; set; } = new List<string>();

    //Estados del Juego
    private TableroReglas tablero;
    private Jugador[] jugadores;
    private Juego juego;
    private bool condicionDeVictoria = false;

    public int JugadorActual { get; private set; } = 0;
    public CartaMovimiento CartaSeleccionada { get; set; } = null;
    public event System.Action<int> TurnoCambiado;

    public static Controller GetInstance()
    {
        if (_instance == null)
        {
            _instance = new Controller();
        }
        return _instance;
    }

    public void InicializarController()
    {
        tablero = new TableroReglas();
        juego = new Juego();
        InicializarJugadores();
    }

    public bool RerollearManoActual()
    {
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
        int cantidad = NombresJugadores.Count > 0 ? NombresJugadores.Count : 4;
        jugadores = new Jugador[cantidad];

        for (int i = 0; i < cantidad; i++)
        {
            string nombre = i < NombresJugadores.Count ? NombresJugadores[i] : $"Jugador {i + 1}";
            jugadores[i] = new Jugador { nombre = nombre, manoCartas = MazoMovimiento.GetInstance().generarMano() };
        }
    }

    public void CambiarCartaSeleccionada(CartaMovimiento carta)
    {
        CartaSeleccionada = carta;
    }

    public Jugador[] Jugadores() => jugadores;

    public string NombreJugadorActual() => jugadores[JugadorActual].nombre;

    public List<CartaMovimiento> ManoJugadorActual() => jugadores[JugadorActual].manoCartas;

    public CartaMovimiento MovimientoActual() => CartaSeleccionada;

    public void TerminarTurno()
    {
        if (condicionDeVictoria || jugadores == null || jugadores.Length == 0) return;

        int JugadorAnterior = JugadorActual;
        JugadorActual = (JugadorActual + 1) % jugadores.Length;
        jugadores[JugadorActual].RerollDisponible = true;
        TurnoCambiado?.Invoke(JugadorActual);
    }
}