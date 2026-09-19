public enum TipoHabilidad
{
    Lobizon,
    LuzMala,
    Pomberito,
    Mulanima
}

public class Personaje
{
    public string Nombre;
    public TipoHabilidad Tipo;
    public int? TurnoUltimoUso;
}