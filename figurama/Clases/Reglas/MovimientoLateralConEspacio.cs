using System;

public class MovimientoLateralConEspacio : CartaMovimiento
{

    public override string Nombre => "Movimiento lateral con espacio";

    public override bool EsValido(TableroReglas tablero, int filaA, int columnaA, int filaB, int columnaB)
    {
        int difFila = Math.Abs(filaA - filaB);
        int difColumna = Math.Abs(columnaA - columnaB);

        bool horizontalConEspacio = difFila == 0 && difColumna == 2;
        bool verticalConEspacio = difColumna == 0 && difFila == 2;

        return horizontalConEspacio || verticalConEspacio;
    }
}