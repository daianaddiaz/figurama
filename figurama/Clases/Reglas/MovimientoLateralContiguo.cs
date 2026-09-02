using System;

public class MovimientoLateralContiguo : CartaMovimiento
{

    public override string Nombre => "Movimiento lateral contiguo";

    public override bool EsValido(TableroReglas tablero, int filaA, int columnaA, int filaB, int columnaB)
    {
        int difFila = Math.Abs(filaA - filaB);
        int difColumna = Math.Abs(columnaA - columnaB);

        bool horizontalContiguo = difFila == 0 && difColumna == 1;
        bool verticalContiguo = difColumna == 0 && difFila == 1;

        return horizontalContiguo || verticalContiguo;
    }
}