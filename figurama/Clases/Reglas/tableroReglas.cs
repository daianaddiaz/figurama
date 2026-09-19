using System;
using System.Collections.Generic;
using Godot;

public class TableroReglas
{
    public const int Filas = 6;
    public const int Columnas = 6;

    public event System.Action<List<(int fila, int columna)>> FiguraEncontrada;

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
        GD.Print($"BuscarFigura llamado. Figura: {figura.Nombre}. CeldasMovidas: {string.Join(" | ", celdasMovidas)}");

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

                    GD.Print($"EsValida=true en ancla ({fila},{columna}). Celdas: {string.Join(" | ", celdas)}");

                    foreach (var celda in celdas)
                    {
                        if (celdasDisparadoras.Contains(celda))
                        {
                            GD.Print("MATCH con disparador, retornando.");
                            return celdas;
                        }
                    }
                    FiguraEncontrada?.Invoke(celdas);
                    GD.Print("Figura encontrada en: " + string.Join(", ", celdas));
                }
            }
        }
        GD.Print("BuscarFigura terminó sin encontrar nada.");
        return null;
    }
}