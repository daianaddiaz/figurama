using System;

public class MovimientoDiagonalConEspacio : CartaMovimiento
{
    public override string Nombre => "Diagonal con Espacio";

    public override bool EsValido(TableroReglas tablero, int filaA, int columnaA, int filaB, int columnaB)
    {
        int difFila = Math.Abs(filaA - filaB);
        int difColumna = Math.Abs(columnaA - columnaB);

        return difFila == 2 && difColumna == 2;
    }
}