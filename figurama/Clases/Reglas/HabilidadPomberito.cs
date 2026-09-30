public class HabilidadPomberito : Habilidad
{
    public override string Nombre => "El Pomberito";

    public override void Activar(Jugador jugador, FichaData ficha = null, Jugador objetivo = null)
    {
        var cartaExtra = MazoMovimiento.GetInstance().ObtenerCartaAlAzar();
        if (cartaExtra != null)
        {
            jugador.manoCartas.Add(cartaExtra);
            jugador.MovimientosExtraEsteTurno += 1;
        }
    }
}