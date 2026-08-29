using System;

public class TableroReglas
{
    public const int Filas = 6;
    public const int Columnas = 6;

    private FichaData[,] _grilla = new FichaData[Filas, Columnas];

    public void ColocarFicha(FichaData ficha, int fila, int columna)
    {
        _grilla[fila, columna] = ficha;
        ficha.Fila = fila;
        ficha.Columna = columna;
    }

    public FichaData ObtenerFicha(int fila, int columna)
    {
        return _grilla[fila, columna];
    }

    public void IntercambiarFichas(int filaA, int columnaA, int filaB, int columnaB)
    {
        FichaData fichaA = ObtenerFicha(filaA, columnaA);
        FichaData fichaB = ObtenerFicha(filaB, columnaB);

        ColocarFicha(fichaA, filaB, columnaB);
        ColocarFicha(fichaB, filaA, columnaA);
    }    
}