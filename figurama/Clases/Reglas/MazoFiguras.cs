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

    public CartaFigura ObtenerSiguienteCarta()
    {
        if (cartas.Count == 0)
        {
            // Si el mazo se vacía, volvemos a generar cartas
            InicializarMazo();
        }

        Random rand = new Random();
        int indiceAleatorio = rand.Next(cartas.Count);
        
        CartaFigura cartaExtraida = cartas[indiceAleatorio];
        cartas.RemoveAt(indiceAleatorio);

        return cartaExtraida;
    }

    public void InicializarMazo()
    {
        cartas.Add(new FiguraCruz());
        cartas.Add(new FiguraL());
        cartas.Add(new FiguraLinea());
        cartas.Add(new FiguraCuadrado());
        cartas.Add(new FiguraZ());
        cartas.Add(new FiguraTDer());
        cartas.Add(new FiguraB());
        cartas.Add(new FiguraZigZag());
        cartas.Add(new FiguraTIzq());
        cartas.Add(new FiguraL5());
        cartas.Add(new FiguraF());
        cartas.Add(new FiguraV());
        cartas.Add(new FiguraU());
        cartas.Add(new FiguraT());
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