using System.Collections.Generic;

public class FiguraCuadrado : CartaFigura
{
    public override string Nombre => "Cuadrado";
    public override int CantidadFichas => 4;

    public override string RutaImagen => "res://Assets/Cartas Figuras/figura4_cubo.png";

    protected override List<(int fila, int columna)[]> Patrones => new List<(int fila, int columna)[]>
    {
        new (int fila, int columna)[] { (0, 0), (0, 1), (1, 0), (1, 1) }
    };
}