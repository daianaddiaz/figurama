public abstract class Habilidad
{
    public abstract string Nombre { get; }
    public const int CooldownVueltas = 2;

    public abstract void Activar(Jugador jugador, FichaData ficha = null, Jugador objetivo = null);
    
    public bool PuedeActivarse(Personaje personaje, Jugador jugador)
    {
        if (!personaje.TurnoUltimoUso.HasValue) return true;
        return (jugador.VueltasJugadas - personaje.TurnoUltimoUso.Value) >= CooldownVueltas;
    }

    public void MarcarUsada(Personaje personaje, Jugador jugador)
    {
        personaje.TurnoUltimoUso = jugador.VueltasJugadas;
    }
}