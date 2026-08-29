using System;

public partial class Juego
{
    public void jugarRonda(Jugador[] jugadores)
    {
        for(int i = 0; i < jugadores.Length; i++)
        {
            jugadores[i].jugarTurno();
        }
    }
}
