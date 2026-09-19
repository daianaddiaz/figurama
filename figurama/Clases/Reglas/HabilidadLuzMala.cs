public class HabilidadLuzMala : Habilidad
{
    public override string Nombre => "Luz Mala";

    public void Activar(Jugador jugadorObjetivo)
    {
        jugadorObjetivo.CartasMovimientoAQuitar += 1;
    }
}