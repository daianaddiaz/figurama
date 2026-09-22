using System.Collections.Generic;

public abstract class CartaFigura
{
    public abstract string Nombre { get; }
    public abstract int CantidadFichas { get; }

    protected abstract List<(int fila, int columna)[]> Patrones { get; }

    public bool EsValida(TableroReglas tablero, int filaAncla, int columnaAncla, out List<(int fila, int columna)> celdas)
    {
        celdas = null;

        foreach (var patron in Patrones)
        {
            var primerOffset = patron[0];
            int filaPrimera = filaAncla + primerOffset.fila;
            int columnaPrimera = columnaAncla + primerOffset.columna;

            if (filaPrimera < 0 || filaPrimera >= TableroReglas.Filas || columnaPrimera < 0 || columnaPrimera >= TableroReglas.Columnas)
                continue;

            FichaData primeraFicha = tablero.ObtenerFicha(filaPrimera, columnaPrimera);
            if (primeraFicha == null) continue;

            if (CoincidePatron(tablero, filaAncla, columnaAncla, primeraFicha.Color, patron, out celdas))
                return true;
        }

        return false;
    }

    private bool CoincidePatron(TableroReglas tablero, int filaAncla, int columnaAncla, ColorFicha colorEsperado, (int fila, int columna)[] patron, out List<(int fila, int columna)> celdas)
    {
        celdas = new List<(int fila, int columna)>();
        var celdasSet = new HashSet<(int fila, int columna)>();

        foreach (var offset in patron)
        {
            int fila = filaAncla + offset.fila;
            int columna = columnaAncla + offset.columna;

            if (fila < 0 || fila >= TableroReglas.Filas || columna < 0 || columna >= TableroReglas.Columnas)
            {
                celdas = null;
                return false;
            }

            FichaData ficha = tablero.ObtenerFicha(fila, columna);
            if (ficha == null || (!ficha.EsComodin && ficha.Color != colorEsperado))
            {
                celdas = null;
                return false;
            }

            celdas.Add((fila, columna));
            celdasSet.Add((fila, columna));
        }

        int[] deltaFila = { -1, 1, 0, 0 };
        int[] deltaColumna = { 0, 0, -1, 1 };

        foreach (var celda in celdas)
        {
            for (int i = 0; i < 4; i++)
            {
                int filaVecina = celda.fila + deltaFila[i];
                int columnaVecina = celda.columna + deltaColumna[i];

                if (filaVecina < 0 || filaVecina >= TableroReglas.Filas || columnaVecina < 0 || columnaVecina >= TableroReglas.Columnas)
                    continue;

                if (celdasSet.Contains((filaVecina, columnaVecina)))
                    continue;

                FichaData vecino = tablero.ObtenerFicha(filaVecina, columnaVecina);
                if (vecino != null && (vecino.EsComodin || vecino.Color == colorEsperado))
                {
                    Godot.GD.Print($"[{Nombre}] Rechazado: vecino en ({filaVecina},{columnaVecina}) tiene color {vecino.Color}, EsComodin={vecino.EsComodin}. Color esperado de la figura: {colorEsperado}.");
                    return false;
                }
            }
        }

        return true;
    }
}