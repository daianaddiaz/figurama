using System.Collections.Generic;

public enum TipoEventoRed
{
    FiguraEncontrada,
    FichaBloqueada,
    FichaDesbloqueada,
    ComodinActivado,
    ComodinDesactivado,
    UltimoColor,
    Victoria,
    TurnoCambiado,
    TiempoAgotado
}

public class EventoRed
{
    public int Tipo { get; set; }
    public List<int> Datos { get; set; } = new List<int>();
    public string Texto { get; set; } = "";
}

public class FichaSnapshot
{
    public int Posicion { get; set; } // fila * 6 + columna
    public bool EsComodin { get; set; }
    public bool Bloqueada { get; set; }
}

public class FiguraSnapshot
{
    public string Nombre { get; set; } = "";
    public bool Completada { get; set; }
}

public class JugadorSnapshot
{
    public string Nombre { get; set; } = "";
    public int Personaje { get; set; }
    public int TurnoUltimoUso { get; set; } = -1;
    public bool RerollDisponible { get; set; }
    public bool RealizoAccionEsteTurno { get; set; }
    public int Puntuacion { get; set; }
    public int VueltasJugadas { get; set; }
    public int CartasMovimientoAQuitar { get; set; }
    public int MovimientosExtraEsteTurno { get; set; }
    public int FichaComodinActiva { get; set; } = -1;
    public List<int> Mano { get; set; } = new List<int>();
    public List<FiguraSnapshot> Figuras { get; set; } = new List<FiguraSnapshot>();
    public List<FiguraSnapshot> Reserva { get; set; } = new List<FiguraSnapshot>();
    public List<int> FichasBloqueadas { get; set; } = new List<int>();
}

public class PartidaSnapshot
{
    public int JugadorActual { get; set; }
    public int UltimoColor { get; set; } = -1;
    public int MovimientosUsadosEnTurno { get; set; }
    public bool JuegoTerminado { get; set; }
    public bool TurnoEnTransicion { get; set; }
    public float TiempoRestante { get; set; }
    public List<JugadorSnapshot> Jugadores { get; set; } = new List<JugadorSnapshot>();
    public List<FichaSnapshot> Fichas { get; set; } = new List<FichaSnapshot>();
}