using System;

public sealed class Controller
{
    private static readonly Controller instance = new Controller();

    private Controller() { flujoDeJuego(); }

    public static Controller Instance
    {
        get { return instance; }
    }

    TableroReglas tablero = new TableroReglas();
    Jugador[] jugadores = new Jugador[] { new Jugador(), new Jugador(), new Jugador(), new Jugador() };
    Juego juego = new Juego();
    bool condicionDeVictoria = false;

    public void flujoDeJuego()
    {
        while(!condicionDeVictoria)
        {
            juego.jugarRonda(jugadores);
        }
    }
}
