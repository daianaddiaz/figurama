public class HabilidadPomberito : Habilidad
{
    public override string Nombre => "El Pomberito";

    public void Activar(Jugador jugador)
    {
        var cartaExtra = MazoMovimiento.GetInstance().ObtenerCartaAlAzar();
        if (cartaExtra != null)
        {
            jugador.manoCartas.Add(cartaExtra);
            jugador.MovimientosExtraEsteTurno += 1;
        }
    }
}