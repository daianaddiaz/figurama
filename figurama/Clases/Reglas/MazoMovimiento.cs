using System;
using System.Collections.Generic;

public partial class MazoMovimiento
{

    private MazoMovimiento() {InicializarMazo();}

    public static MazoMovimiento _instance;
    
    public List<CartaMovimiento> cartas { get; set; } = new List<CartaMovimiento>();

    public static MazoMovimiento GetInstance()
    {
        if (_instance == null)
        {
            _instance = new MazoMovimiento();
        }
        return _instance;
    }

    public void InicializarMazo()
    {
        // Inicializar el mazo con cartas de movimiento
        cartas.Add(new MovimientoEnL{});
        cartas.Add(new MovimientoLateralConEspacio{});
        cartas.Add(new MovimientoLateralContiguo{});
        cartas.Add(new MovimientoDiagonalContiguo{});
        cartas.Add(new MovimientoDiagonalConEspacio{});
    }

    public List<CartaMovimiento> generarMano()
    {
        List<CartaMovimiento> mano = new List<CartaMovimiento>();
        Random rand = new Random();

        int cantidadCartas = 3;
            if (Controller._instance != null)
            {
             cantidadCartas = Controller._instance.CantidadCartasMovimiento;
            }

        for (int i = 0; i < cantidadCartas; i++)
        {
            int indiceAleatorio = rand.Next(cartas.Count);
            mano.Add(cartas[indiceAleatorio]);
        }

        return mano;
    }
}
