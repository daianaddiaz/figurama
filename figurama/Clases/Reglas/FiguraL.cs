using System.Collections.Generic;

public class FiguraL : CartaFigura
{
    public override string Nombre => "L";
    public override int CantidadFichas => 4;

    protected override List<(int fila, int columna)[]> Patrones => new List<(int fila, int columna)[]>
    {
        new (int fila, int columna)[] { (0, 0), (1, 0), (2, 0), (2, 1) }, // vertical, pie abajo a la derecha
        new (int fila, int columna)[] { (0, 0), (1, 0), (2, 0), (0, 1) }, // vertical, pie arriba a la derecha
        new (int fila, int columna)[] { (0, 0), (0, 1), (0, 2), (1, 0) }, // horizontal, pie a la izquierda
        new (int fila, int columna)[] { (0, 0), (0, 1), (0, 2), (1, 2) }  // horizontal, pie a la derecha
    };
}