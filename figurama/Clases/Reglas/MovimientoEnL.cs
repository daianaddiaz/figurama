using System;

public class MovimientoEnL : CartaMovimiento
{

    public override string Nombre => "Movimiento en L";

    public override bool EsValido(TableroReglas tablero, int filaA, int columnaA, int filaB, int columnaB)
    {
        int difFila = Math.Abs(filaA - filaB);
        int difColumna = Math.Abs(columnaA - columnaB);

        bool formaL = (difFila == 1 && difColumna == 2) || (difFila == 2 && difColumna == 1);

        return formaL;
    }
}