using System.Collections.Generic;

public class FiguraZ : CartaFigura
{
    public override string Nombre => "Z";
    public override int CantidadFichas => 4;

    public override string RutaImagen => "res://Assets/Cartas Figuras/figura4_zeta.png";

    protected override List<(int fila, int columna)[]> Patrones => new List<(int fila, int columna)[]>
    {
        new (int fila, int columna)[] { (0, 1), (0, 2), (1, 0), (1, 1) },
        new (int fila, int columna)[] { (0, 0), (1, 0), (1, 1), (2, 1) }
    };
}