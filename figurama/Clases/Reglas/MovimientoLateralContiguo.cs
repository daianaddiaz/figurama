using Godot;

public class MovimientoLateralContiguo : CartaMovimiento
{
    public override bool EsValido(Tablero tablero, int filaA, int columnaA, int filaB, int columnaB)
    {
        int difFila = Mathf.Abs(filaA - filaB);
        int difColumna = Mathf.Abs(columnaA - columnaB);

        bool horizontalContiguo = difFila == 0 && difColumna == 1;
        bool verticalContiguo = difColumna == 0 && difFila == 1;

        return horizontalContiguo || verticalContiguo;
    }
}