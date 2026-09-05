using System;
using System.Collections.Generic;

public partial class MazoFiguras
{
    private MazoFiguras() { InicializarMazo(); }

    public static MazoFiguras _instance;

    public List<CartaFigura> cartas { get; set; } = new List<CartaFigura>();

    public static MazoFiguras GetInstance()
    {
        if (_instance == null)
        {
            _instance = new MazoFiguras();
        }
        return _instance;
    }

    public void InicializarMazo()
    {
        cartas.Add(new FiguraCruz());
        cartas.Add(new FiguraL());
        cartas.Add(new FiguraLinea());
        cartas.Add(new FiguraCuadrado());
    }

    public List<CartaFigura> generarMano(int cantidad)
    {
        List<CartaFigura> mano = new List<CartaFigura>();
        Random rand = new Random();

        for (int i = 0; i < cantidad; i++)
        {
            int indiceAleatorio = rand.Next(cartas.Count);
            mano.Add(cartas[indiceAleatorio]);
        }

        return mano;
    }
}