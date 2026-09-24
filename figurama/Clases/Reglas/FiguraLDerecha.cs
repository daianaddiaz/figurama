using System.Collections.Generic;

public class Figura4LDerecha : CartaFigura
{
    public override string Nombre => "LDerecha4";
    public override int CantidadFichas => 4;
    public override string RutaImagen => "res://Assets/Cartas Figuras/figura4_LDerecha.png";

    protected override List<(int fila, int columna)[]> Patrones => new List<(int fila, int columna)[]>
    {
        // Base 
        new (int fila, int columna)[] { (0, 0), (1, 0), (2, 0), (2, 1) },

        // 90° 
        new (int fila, int columna)[] { (0, 0), (0, 1), (0, 2), (1, 0) },

        //  180° 
        new (int fila, int columna)[] { (0, 0), (0, 1), (1, 1), (2, 1) },

        // 270° 
        new (int fila, int columna)[] { (0, 2), (1, 0), (1, 1), (1, 2) }
    };
}