using System;
using System.Collections.Generic;

public partial class Controller
{
    public static Controller _instance;

    // Nombres desde el Menu
    public List<string> NombresJugadores { get; set; } = new List<string>();

    // Propiedad por defecto dificultad "Normal".
    public int CantidadCartasMovimiento { get; set; } = 3; 

    // Estados del Juego
    private TableroReglas tablero;
    private Jugador[] jugadores;
    private Juego juego;
    private bool condicionDeVictoria = false;

    public int JugadorActual { get; private set; } = 0;
    public CartaMovimiento CartaSeleccionada { get; set; } = null;
    public event System.Action<int> TurnoCambiado;

    // Constructor privado SIMPLE 
    private Controller() 
    {
    }

    public static Controller GetInstance()
    {
        if (_instance == null)
        {
            _instance = new Controller();
            // Se asigna la instancia PRIMERO y LUEGO se inicializa
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

    public string NombreJugadorActual() => jugadores != null && jugadores.Length > JugadorActual ? jugadores[JugadorActual].nombre : "";

    public List<CartaMovimiento> ManoJugadorActual() => jugadores != null && jugadores.Length > JugadorActual ? jugadores[JugadorActual].manoCartas : new List<CartaMovimiento>();

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

// Habia problema de bucle infinito en instancias
// Corregido