public abstract class CartaMovimiento
{
    public abstract bool EsValido(TableroReglas tablero, int filaA, int columnaA, int filaB, int columnaB);

    public void Ejecutar(TableroReglas tablero, int filaA, int columnaA, int filaB, int columnaB)
    {
        tablero.IntercambiarFichas(filaA, columnaA, filaB, columnaB);
    }
}