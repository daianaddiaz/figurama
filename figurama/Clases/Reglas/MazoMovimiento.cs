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
        cartas.Add(new MovimientoLateralAlBorde{});
    }

    public CartaMovimiento ObtenerSiguienteCarta()
{
    if (cartas.Count == 0)
    {
        return null;
    }

    CartaMovimiento cartaExtraida = cartas[0];
    cartas.RemoveAt(0);

    return cartaExtraida;
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
            CartaMovimiento cartaBase = cartas[indiceAleatorio];

            CartaMovimiento nuevaCarta = cartaBase switch
            {
                MovimientoEnL => new MovimientoEnL
                {
                    DireccionPermitida = MovimientoEnL.TipoDireccion.Cualquiera
                },

                MovimientoLateralConEspacio => new MovimientoLateralConEspacio(),
                MovimientoLateralContiguo => new MovimientoLateralContiguo(),
                MovimientoDiagonalContiguo => new MovimientoDiagonalContiguo(),
                MovimientoDiagonalConEspacio => new MovimientoDiagonalConEspacio(),
                MovimientoLateralAlBorde => new MovimientoLateralAlBorde(),

                _ => throw new Exception(
                    $"Tipo de carta no reconocido: {cartaBase.GetType().Name}"
                )
            };

            mano.Add(nuevaCarta);
        }

        return mano;
    }

    public CartaMovimiento ObtenerCartaAlAzar()
    {
        if (cartas.Count == 0) return null;

        Random rand = new Random();
        CartaMovimiento cartaBase = cartas[rand.Next(cartas.Count)];

        return cartaBase switch
        {
            MovimientoEnL => new MovimientoEnL { DireccionPermitida = MovimientoEnL.TipoDireccion.Cualquiera },
            MovimientoLateralConEspacio => new MovimientoLateralConEspacio(),
            MovimientoLateralContiguo => new MovimientoLateralContiguo(),
            MovimientoDiagonalContiguo => new MovimientoDiagonalContiguo(),
            MovimientoDiagonalConEspacio => new MovimientoDiagonalConEspacio(),
            MovimientoLateralAlBorde => new MovimientoLateralAlBorde(),
            _ => null
        };
    }
}
