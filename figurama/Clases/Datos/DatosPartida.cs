using System.Collections.Generic;

public static class DatosPartida
{
    public static List<string> NombresJugadores = new List<string>();
    public static List<TipoHabilidad> PersonajesElegidos = new List<TipoHabilidad>();
    public static int IndiceJugadorLocal = 0;
    public static bool EsMultijugador = false;
    public static List<long> IdsJugadores = new List<long>();
    public static bool EsEspectador = false;
    public static string NombreSala = "";
    public static string TokenLocal = System.Guid.NewGuid().ToString();
    public static List<string> TokensJugadores = new List<string>();
    public static string SalaReconexionId = "";
}