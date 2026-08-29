using System;

public partial class Jugador
{

    public string nombre { get; set; }

    public Jugador()
    {
        //inputNombre();
    }

    public void inputNombre()
    {
        Console.WriteLine("Ingrese el nombre del jugador:");
        nombre = Console.ReadLine();
    }

    public void jugarTurno()
    {
        // Lógica para que el jugador juegue su turno
    }
}
