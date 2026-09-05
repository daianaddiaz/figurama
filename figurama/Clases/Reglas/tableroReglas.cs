using System;
using System.Collections.Generic;

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

    public List<(int fila, int columna)> BuscarFigura(CartaFigura figura, HashSet<(int fila, int columna)> celdasMovidas)
    {
        var celdasDisparadoras = new HashSet<(int fila, int columna)>(celdasMovidas);

        int[] deltaFila = { -1, 1, 0, 0 };
        int[] deltaColumna = { 0, 0, -1, 1 };

        foreach (var celda in celdasMovidas)
        {
            for (int i = 0; i < 4; i++)
            {
                int filaVecina = celda.fila + deltaFila[i];
                int columnaVecina = celda.columna + deltaColumna[i];

                if (filaVecina < 0 || filaVecina >= Filas || columnaVecina < 0 || columnaVecina >= Columnas)
                    continue;

                celdasDisparadoras.Add((filaVecina, columnaVecina));
            }
        }

        for (int fila = 0; fila < Filas; fila++)
        {
            for (int columna = 0; columna < Columnas; columna++)
            {
                if (figura.EsValida(this, fila, columna, out List<(int fila, int columna)> celdas))
                {
                    foreach (var celda in celdas)
                    {
                        if (celdasDisparadoras.Contains(celda))
                        {
                            return celdas;
                        }
                    }
                }
            }
        }

        return null;
    }
}