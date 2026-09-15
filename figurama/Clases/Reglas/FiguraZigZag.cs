using System.Collections.Generic;

public class FiguraZigZag : CartaFigura
{
    public override string Nombre => "ZigZag";
    public override int CantidadFichas => 5;

    protected override List<(int fila, int columna)[]> Patrones => new List<(int fila, int columna)[]>
    {
        new (int fila, int columna)[] { (0, 0), (0, 1), (1, 1), (2, 1), (2, 2) },
        new (int fila, int columna)[] { (0, 2), (1, 0), (1, 1), (1, 2), (2, 0) }
    };
}