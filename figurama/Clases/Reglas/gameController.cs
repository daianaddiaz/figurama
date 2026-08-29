using System;

public sealed class Controller
{
    private static readonly Controller instance = new Controller();
    public static Controller Instance => instance;

    private Controller() { /*flujoDeJuego();*/ }

    TableroReglas tablero = new TableroReglas();
    Jugador[] jugadores = CrearJugadores();
    Juego juego = new Juego();
    bool condicionDeVictoria = false;

    public int JugadorActual { get; private set; } = 0;
    public event Action<int> TurnoCambiado;

    private static Jugador[] CrearJugadores()
    {
        var resultado = new Jugador[4];
        for (int i = 0; i < resultado.Length; i++)
        {
            string nombre = i < DatosPartida.NombresJugadores.Count ? DatosPartida.NombresJugadores[i] : $"Jugador {i + 1}";
            resultado[i] = new Jugador { nombre = nombre };
        }
        return resultado;
    }

    public string NombreJugadorActual() => jugadores[JugadorActual].nombre;

    public void TerminarTurno()
    {
        if (condicionDeVictoria) return;

        JugadorActual = (JugadorActual + 1) % jugadores.Length;
        TurnoCambiado?.Invoke(JugadorActual);
    }

    public void flujoDeJuego()
    {
        while(!condicionDeVictoria)
        {
            juego.jugarRonda(jugadores);
        }
    }
}
