using System.Collections.Generic;

public class Figura5LDerecha : CartaFigura
{
    public override string Nombre => "LDerecha5";
    public override int CantidadFichas => 5;
    public override string RutaImagen => "res://Assets/Cartas Figuras/figura5_LDerecha.png";

    protected override List<(int fila, int columna)[]> Patrones => new List<(int fila, int columna)[]>
    {
        // Orientación vertical base (L normal: columna vertical en x=0, patita en (3,1))
        new (int fila, int columna)[] { (0, 0), (1, 0), (2, 0), (3, 0), (3, 1) },

        // Rotacion de 90° 
        new (int fila, int columna)[] { (0, 0), (0, 1), (0, 2), (0, 3), (1, 0) },

        // Rotacion de 180° 
        new (int fila, int columna)[] { (0, 0), (0, 1), (1, 1), (2, 1), (3, 1) },

        //Rotacion de 270° 
        new (int fila, int columna)[] { (1, 0), (1, 1), (1, 2), (1, 3), (0, 3) }
    };
}