using System.Collections.Generic;

public class FiguraB : CartaFigura
{
    public override string Nombre => "B";
    public override int CantidadFichas => 5;

    public override string RutaImagen => "res://Assets/Cartas Figuras/figura5_bloqueB.png";

    protected override List<(int fila, int columna)[]> Patrones => new List<(int fila, int columna)[]>
    {
        new (int fila, int columna)[] { (0, 0), (1, 0), (1, 1), (2, 0), (2, 1) },
        new (int fila, int columna)[] { (0, 0), (0, 1), (0, 2), (1, 0), (1, 1) },
        new (int fila, int columna)[] { (0, 0), (0, 1), (1, 0), (1, 1), (2, 1) },
        new (int fila, int columna)[] { (0, 1), (0, 2), (1, 0), (1, 1), (1, 2) }
    };
}