using Godot;
using System;
using System.Collections.Generic;


public partial class Controller : Node
{
    // Singleton de Godot
    public static Controller Instance { get; private set; }

    // Nombres desde el Menu
    public List<string> NombresJugadores { get; set; } = new List<string>();

    //Estados del Juego
    private TableroReglas tablero;
    private Jugador[] jugadores;
    private Juego juego;
    private bool condicionDeVictoria = false;

    public int JugadorActual { get; private set; } = 0;
    public event System.Action<int> TurnoCambiado;

    public override void _Ready()
    {
        // Configuración del Singleton
        if (Instance != null && Instance != this)
        {
            QueueFree();
            return;
        }

        Instance = this;

        tablero = new TableroReglas();
        juego = new Juego();
        InicializarJugadores();
    }

    public void InicializarJugadores()
    {
        int cantidad = NombresJugadores.Count > 0 ? NombresJugadores.Count : 4;
        jugadores = new Jugador[cantidad];

        for (int i = 0; i < cantidad; i++)
        {
            string nombre = i < NombresJugadores.Count ? NombresJugadores[i] : $"Jugador {i + 1}";
            jugadores[i] = new Jugador { nombre = nombre };
        }
    }

    public string NombreJugadorActual() => jugadores[JugadorActual].nombre;

    public void TerminarTurno()
    {
        if (condicionDeVictoria || jugadores == null || jugadores.Length == 0) return;

        JugadorActual = (JugadorActual + 1) % jugadores.Length;
        TurnoCambiado?.Invoke(JugadorActual);
    }
}