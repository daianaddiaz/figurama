public class HabilidadLuzMala : Habilidad
{
    public override string Nombre => "Luz Mala";

    public override void Activar(Jugador jugador, FichaData ficha = null, Jugador objetivo = null)
    {
        objetivo.CartasMovimientoAQuitar += 1;
    }
}