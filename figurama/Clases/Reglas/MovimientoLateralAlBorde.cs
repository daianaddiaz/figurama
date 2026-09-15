using System;

public class MovimientoLateralAlBorde : CartaMovimiento
{
    public override string Nombre => "Lateral al Borde";

    public override bool EsValido(TableroReglas tablero, int filaA, int columnaA, int filaB, int columnaB)
    {
        if (filaA == filaB && columnaA == columnaB) return false;

        bool mismaFilaBordeColumna = filaA == filaB && (columnaB == 0 || columnaB == TableroReglas.Columnas - 1);
        bool mismaColumnaBordeFila = columnaA == columnaB && (filaB == 0 || filaB == TableroReglas.Filas - 1);

        return mismaFilaBordeColumna || mismaColumnaBordeFila;
    }
}