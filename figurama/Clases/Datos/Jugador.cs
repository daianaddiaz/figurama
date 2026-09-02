using System;
using System.Collections.Generic;
public partial class Jugador
{

    public string nombre { get; set; }
    public List<CartaMovimiento> manoCartas { get; set; } = new List<CartaMovimiento>();
    public bool RerollDisponible { get; set; } = true;


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
