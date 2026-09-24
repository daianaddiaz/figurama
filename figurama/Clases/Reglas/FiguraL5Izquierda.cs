using System.Collections.Generic;

public class Figura5LIzquierda : CartaFigura
{
    public override string Nombre => "LIzquierda5";
    public override int CantidadFichas => 5;
    public override string RutaImagen => "res://Assets/Cartas Figuras/figura5_LIzquierda.png";

    protected override List<(int fila, int columna)[]> Patrones => new List<(int fila, int columna)[]>
    {
        // Base
        new (int fila, int columna)[] { (0, 1), (1, 1), (2, 1), (3, 1), (3, 0) },

        // 90°
        new (int fila, int columna)[] { (1, 0), (1, 1), (1, 2), (1, 3), (0, 0) },

        // 180°
        new (int fila, int columna)[] { (0, 0), (1, 0), (2, 0), (3, 0), (0, 1) },

        // 270°
        new (int fila, int columna)[] { (0, 0), (0, 1), (0, 2), (0, 3), (1, 3) }
    };
}