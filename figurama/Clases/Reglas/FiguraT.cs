using System.Collections.Generic;

public class FiguraT : CartaFigura
{
    public override string Nombre => "T";
    public override int CantidadFichas => 4;

    protected override List<(int fila, int columna)[]> Patrones => new List<(int fila, int columna)[]>
    {
        new (int fila, int columna)[] { (0, 0), (1, 0), (1, 1), (2, 0) }, // tallo a la derecha
        new (int fila, int columna)[] { (0, 0), (0, 1), (0, 2), (1, 1) }, // tallo abajo
        new (int fila, int columna)[] { (0, 1), (1, 0), (1, 1), (2, 1) }, // tallo a la izquierda
        new (int fila, int columna)[] { (0, 1), (1, 0), (1, 1), (1, 2) }  // tallo arriba
    };
}