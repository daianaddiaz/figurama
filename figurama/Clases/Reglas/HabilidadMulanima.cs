public class HabilidadMulanima : Habilidad
{
    public override string Nombre => "Mulánima";

    public void Activar(Jugador jugador, FichaData ficha)
    {
        ficha.Bloqueada = true;
        jugador.FichasBloqueadas.Add(ficha);
    }

    public void LiberarTodas(Jugador jugador)
    {
        foreach (FichaData ficha in jugador.FichasBloqueadas)
        {
            ficha.Bloqueada = false;
        }
        jugador.FichasBloqueadas.Clear();
    }
}