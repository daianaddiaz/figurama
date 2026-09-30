public class HabilidadLobizon : Habilidad
{
    public override string Nombre => "Lobizón";

    public override void Activar(Jugador jugador, FichaData ficha = null, Jugador objetivo = null)
    {
        ficha.EsComodin = true;
        jugador.FichaComodinActiva = ficha;
    }
}