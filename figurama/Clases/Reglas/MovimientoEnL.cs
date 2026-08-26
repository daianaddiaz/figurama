using Godot;

public class MovimientoEnL : CartaMovimiento
{
    public override bool EsValido(Tablero tablero, int filaA, int columnaA, int filaB, int columnaB)
    {
        int difFila = Mathf.Abs(filaA - filaB);
        int difColumna = Mathf.Abs(columnaA - columnaB);

        bool formaL = (difFila == 1 && difColumna == 2) || (difFila == 2 && difColumna == 1);

        return formaL;
    }
}