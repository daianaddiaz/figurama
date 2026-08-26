using Godot;

public abstract class CartaMovimiento
{
    public abstract bool EsValido(Tablero tablero, int filaA, int columnaA, int filaB, int columnaB);

    public void Ejecutar(Tablero tablero, int filaA, int columnaA, int filaB, int columnaB)
    {
        tablero.IntercambiarFichas(filaA, columnaA, filaB, columnaB);
    }
}