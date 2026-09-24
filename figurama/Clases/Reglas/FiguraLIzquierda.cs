using System.Collections.Generic;

public class Figura4LIzquierda : CartaFigura
{
    public override string Nombre => "LIzquierda4";
    public override int CantidadFichas => 4;
    public override string RutaImagen => "res://Assets/Cartas Figuras/figura4_LIzquierda.png";

    protected override List<(int fila, int columna)[]> Patrones => new List<(int fila, int columna)[]>
    {
        // 1. Base (0°: columna vertical en x=1, patita abajo a la izquierda en (2,0))
        new (int fila, int columna)[] { (0, 1), (1, 1), (2, 1), (2, 0) },

        // 2. 90° (barra horizontal abajo en fila 1, patita arriba a la izquierda en (0,0))
        new (int fila, int columna)[] { (0, 0), (1, 0), (1, 1), (1, 2) },

        // 3. 180° (columna vertical en x=0, patita arriba a la derecha en (0,1))
        new (int fila, int columna)[] { (0, 0), (0, 1), (1, 0), (2, 0) },

        // 4. 270° (barra horizontal arriba en fila 0, patita abajo a la derecha en (1,2))
        new (int fila, int columna)[] { (0, 0), (0, 1), (0, 2), (1, 2) }
    };
}