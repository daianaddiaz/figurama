public class HabilidadLobizon : Habilidad
{
    public override string Nombre => "Lobizón";

    public void Activar(Jugador jugador, FichaData ficha)
    {
        ficha.EsComodin = true;
        jugador.FichaComodinActiva = ficha;
    }
}