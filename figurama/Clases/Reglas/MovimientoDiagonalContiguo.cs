using System;

public class MovimientoDiagonalContiguo : CartaMovimiento
{
    public override string Nombre => "Diagonal Contiguo";

    public override bool EsValido(TableroReglas tablero, int filaA, int columnaA, int filaB, int columnaB)
    {
        int difFila = Math.Abs(filaA - filaB);
        int difColumna = Math.Abs(columnaA - columnaB);

        return difFila == 1 && difColumna == 1;
    }
}