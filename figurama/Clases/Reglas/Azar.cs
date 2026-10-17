using System;

public static class Azar
{
    private static Random _rng = new Random();

    public static void Iniciar(int semilla)
    {
        _rng = new Random(semilla);
    }

    public static void Reiniciar()
    {
        _rng = new Random();
    }

    public static int Siguiente(int maximo)
    {
        return _rng.Next(maximo);
    }
}