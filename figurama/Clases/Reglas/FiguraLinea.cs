using System.Collections.Generic;

public class FiguraLinea : CartaFigura
{
    public override string Nombre => "Linea";
    public override int CantidadFichas => 4;

    protected override List<(int fila, int columna)[]> Patrones => new List<(int fila, int columna)[]>
    {
        new (int fila, int columna)[] { (0, 0), (0, 1), (0, 2), (0, 3) }, // horizontal
        new (int fila, int columna)[] { (0, 0), (1, 0), (2, 0), (3, 0) }  // vertical
    };
}