using System.Collections.Generic;

public class FiguraCruz : CartaFigura
{
    public override string Nombre => "Cruz";
    public override int CantidadFichas => 5;

    public override string RutaImagen => "res://Assets/Cartas Figuras/figura5_cruz.png";

    protected override List<(int fila, int columna)[]> Patrones => new List<(int fila, int columna)[]>
    {
        new (int fila, int columna)[] { (0, 0), (-1, 0), (1, 0), (0, -1), (0, 1) }
    };
}