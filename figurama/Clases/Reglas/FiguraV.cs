using System.Collections.Generic;

public class FiguraV : CartaFigura
{
    public override string Nombre => "V";
    public override int CantidadFichas => 5;

    public override string RutaImagen => "res://Assets/Cartas Figuras/figura5_LCubo.png";

    protected override List<(int fila, int columna)[]> Patrones => new List<(int fila, int columna)[]>
    {
        new (int fila, int columna)[] { (0, 0), (1, 0), (2, 0), (2, 1), (2, 2) },
        new (int fila, int columna)[] { (0, 0), (0, 1), (0, 2), (1, 0), (2, 0) },
        new (int fila, int columna)[] { (0, 0), (0, 1), (0, 2), (1, 2), (2, 2) },
        new (int fila, int columna)[] { (0, 2), (1, 2), (2, 0), (2, 1), (2, 2) }
    };
}