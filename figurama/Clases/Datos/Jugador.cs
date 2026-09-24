using System;
using System.Collections.Generic;

public partial class Jugador
{
    public string nombre { get; set; }
    public List<CartaMovimiento> manoCartas { get; set; } = new List<CartaMovimiento>();
    public List<FiguraAsignada> figurasAArmar { get; set; } = new List<FiguraAsignada>();
    public List<FiguraAsignada> figurasEnReserva { get; set; } = new List<FiguraAsignada>();
    public bool RerollDisponible { get; set; } = true;
    public bool RealizoAccionEsteTurno { get; set; } = false;
    public int Puntuacion { get; set; } = 0;
    public int VueltasJugadas { get; set; } = 0;
    public Personaje PersonajeAsignado { get; set; }
    public List<FichaData> FichasBloqueadas { get; set; } = new List<FichaData>();
    public int CartasMovimientoAQuitar { get; set; } = 0;
    public FichaData FichaComodinActiva { get; set; }
    public int MovimientosExtraEsteTurno { get; set; } = 0;

    public Jugador() { }
}