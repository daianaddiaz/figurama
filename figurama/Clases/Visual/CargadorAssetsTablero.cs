using System.Collections.Generic;

public static class CargadorAssetsTablero
{
    public static readonly Dictionary<TipoHabilidad, string> NombreBotonHabilidad = new Dictionary<TipoHabilidad, string>
    {
        { TipoHabilidad.Lobizon, "Furia del Lobizón" },
        { TipoHabilidad.LuzMala, "Luz Mala Activa" },
        { TipoHabilidad.Pomberito, "Pomberito Recargado" },
        { TipoHabilidad.Mulanima, "Mulánima Enfurecida" }
    };

    public static readonly Dictionary<TipoHabilidad, string> TexturaDisponibleHabilidad = new Dictionary<TipoHabilidad, string>
    {
        { TipoHabilidad.Lobizon, "res://Assets/ActivarLobizon.png" },
        { TipoHabilidad.LuzMala, "res://Assets/ActivarLuzMala.png" },
        { TipoHabilidad.Pomberito, "res://Assets/ActivarPomberito.png" },
        { TipoHabilidad.Mulanima, "res://Assets/ActivarMulanima.png" }
    };

    public static readonly Dictionary<TipoHabilidad, string> TexturaNoDisponibleHabilidad = new Dictionary<TipoHabilidad, string>
    {
        { TipoHabilidad.Lobizon, "res://Assets/DesactivarLobizon.png" },
        { TipoHabilidad.LuzMala, "res://Assets/DesactivarLuzMala.png" },
        { TipoHabilidad.Pomberito, "res://Assets/DesactivarPomberito.png" },
        { TipoHabilidad.Mulanima, "res://Assets/DesactivarMulanima.png" }
    };

    public static readonly Dictionary<TipoHabilidad, string> TexturaInsignia = new Dictionary<TipoHabilidad, string>
    {
        { TipoHabilidad.Lobizon, "res://Assets/IconoLobizon.png" },
        { TipoHabilidad.LuzMala, "res://Assets/IconoLuzMala.png" },
        { TipoHabilidad.Pomberito, "res://Assets/IconoPomberito.png" },
        { TipoHabilidad.Mulanima, "res://Assets/IconoMulanima.png" }
    };
}