using Godot;

public class MovimientoLateralConEspacio : CartaMovimiento
{
    public override bool EsValido(Tablero tablero, int filaA, int columnaA, int filaB, int columnaB)
    {
        int difFila = Mathf.Abs(filaA - filaB);
        int difColumna = Mathf.Abs(columnaA - columnaB);

        bool horizontalConEspacio = difFila == 0 && difColumna == 2;
        bool verticalConEspacio = difColumna == 0 && difFila == 2;

        return horizontalConEspacio || verticalConEspacio;
    }
}