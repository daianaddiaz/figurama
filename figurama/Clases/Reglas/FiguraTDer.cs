using System.Collections.Generic;

public class FiguraTDer : CartaFigura
{
    public override string Nombre => "T Derecha";
    public override int CantidadFichas => 5;

    public override string RutaImagen => "res://Assets/Cartas Figuras/figura5_TDerecha.png";

    protected override List<(int fila, int columna)[]> Patrones => new List<(int fila, int columna)[]>
    {
        new (int fila, int columna)[] { (0, 0), (1, 0), (2, 0), (2, 1), (3, 0) },
        new (int fila, int columna)[] { (0, 0), (0, 1), (0, 2), (0, 3), (1, 1) },
        new (int fila, int columna)[] { (0, 1), (1, 0), (1, 1), (2, 1), (3, 1) },
        new (int fila, int columna)[] { (0, 2), (1, 0), (1, 1), (1, 2), (1, 3) }
    };
}