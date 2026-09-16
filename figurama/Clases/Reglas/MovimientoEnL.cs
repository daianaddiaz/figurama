using System;

public class MovimientoEnL : CartaMovimiento
{

   public enum TipoDireccion { Cualquiera, Derecha, Izquierda }
    
    public TipoDireccion DireccionPermitida { get; set; } = TipoDireccion.Cualquiera;

    public override string Nombre => $"Movimiento en L ({DireccionPermitida})";

    public override bool EsValido(TableroReglas tablero, int filaA, int columnaA, int filaB, int columnaB)
    {
        int dFila = filaB - filaA;
        int dColumna = columnaB - columnaA;

        int absFila = Math.Abs(dFila);
        int absColumna = Math.Abs(dColumna);

        // Validar la forma en L básica (2x1 o 1x2)
        bool esL = (absFila == 1 && absColumna == 2) || (absFila == 2 && absColumna == 1);
        if (!esL) return false;

        if (DireccionPermitida == TipoDireccion.Cualquiera) return true;

        // Si se requiere restringir dirección a Izquierda/Derecha desde la perspectiva del movimiento:
        if (DireccionPermitida == TipoDireccion.Derecha)
        {
            return dColumna > 0; // Hacia columnas mayores (derecha)
        }
        else // Izquierda
        {
            return dColumna < 0; // Hacia columnas menores (izquierda)
        }
    }
}