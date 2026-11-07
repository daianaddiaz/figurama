using System;
using System.Collections.Generic;

public partial class Controller
{
    // ---------- Servidor: exportar el estado ----------

    public PartidaSnapshot ExportarEstado(Func<FichaData, int> idDeFicha)
    {
        var snap = new PartidaSnapshot
        {
            JugadorActual = JugadorActual,
            UltimoColor = UltimoColorUsado.HasValue ? (int)UltimoColorUsado.Value : -1,
            MovimientosUsadosEnTurno = MovimientosUsadosEnTurno,
            JuegoTerminado = condicionDeVictoria,
            TurnoEnTransicion = TurnoEnTransicion,
            TiempoRestante = TiempoRestante
        };

        if (jugadores == null) return snap;

        foreach (Jugador jugador in jugadores)
        {
            snap.Jugadores.Add(ExportarJugador(jugador, idDeFicha));
        }

        return snap;
    }

    private JugadorSnapshot ExportarJugador(Jugador j, Func<FichaData, int> idDeFicha)
    {
        var s = new JugadorSnapshot
        {
            Nombre = j.nombre,
            Personaje = (int)j.PersonajeAsignado.Tipo,
            TurnoUltimoUso = j.PersonajeAsignado.TurnoUltimoUso ?? -1,
            RerollDisponible = j.RerollDisponible,
            RealizoAccionEsteTurno = j.RealizoAccionEsteTurno,
            Puntuacion = j.Puntuacion,
            VueltasJugadas = j.VueltasJugadas,
            CartasMovimientoAQuitar = j.CartasMovimientoAQuitar,
            MovimientosExtraEsteTurno = j.MovimientosExtraEsteTurno,
            FichaComodinActiva = j.FichaComodinActiva != null ? idDeFicha(j.FichaComodinActiva) : -1
        };

        foreach (CartaMovimiento carta in j.manoCartas)
        {
            s.Mano.Add(CodigoCarta(carta));
        }

        foreach (FiguraAsignada f in j.figurasAArmar)
        {
            s.Figuras.Add(new FiguraSnapshot { Nombre = f.Figura.Nombre, Completada = f.Completada });
        }

        foreach (FiguraAsignada f in j.figurasEnReserva)
        {
            s.Reserva.Add(new FiguraSnapshot { Nombre = f.Figura.Nombre, Completada = f.Completada });
        }

        foreach (FichaData ficha in j.FichasBloqueadas)
        {
            s.FichasBloqueadas.Add(idDeFicha(ficha));
        }

        return s;
    }

    // ---------- Cliente: importar el estado ----------

    // Prepara este Controller como réplica pasiva (el cliente no ejecuta reglas).
    public void PrepararReplica()
    {
        LimpiarEventos();
        jugadores = null;
        JugadorActual = 0;
        condicionDeVictoria = false;
        TurnoEnTransicion = false;
        enPausa = false;
        temporizadorActivo = false;
        MovimientosUsadosEnTurno = 0;
        UltimoColorUsado = null;
        CartaSeleccionada = null;
    }

    public void ImportarEstado(PartidaSnapshot snap, Func<int, FichaData> fichaPorId)
    {
        // Los objetos Jugador se actualizan "en el lugar": las vistas guardan referencias a ellos
        if (jugadores == null || jugadores.Length != snap.Jugadores.Count)
        {
            jugadores = new Jugador[snap.Jugadores.Count];
            for (int i = 0; i < jugadores.Length; i++)
            {
                jugadores[i] = new Jugador { PersonajeAsignado = new Personaje() };
            }
        }

        for (int i = 0; i < jugadores.Length; i++)
        {
            ImportarJugador(jugadores[i], snap.Jugadores[i], fichaPorId);
        }

        JugadorActual = snap.JugadorActual;
        if (snap.UltimoColor >= 0)
        {
            UltimoColorUsado = (ColorFicha)snap.UltimoColor;
        }
        else
        {
            UltimoColorUsado = null;
        }
        MovimientosUsadosEnTurno = snap.MovimientosUsadosEnTurno;
        condicionDeVictoria = snap.JuegoTerminado;
        TurnoEnTransicion = snap.TurnoEnTransicion;
        TiempoRestante = snap.TiempoRestante;
    }

    private void ImportarJugador(Jugador j, JugadorSnapshot s, Func<int, FichaData> fichaPorId)
    {
        j.nombre = s.Nombre;
        j.PersonajeAsignado.Nombre = s.Nombre;
        j.PersonajeAsignado.Tipo = (TipoHabilidad)s.Personaje;
        j.PersonajeAsignado.TurnoUltimoUso = s.TurnoUltimoUso >= 0 ? s.TurnoUltimoUso : (int?)null;

        j.RerollDisponible = s.RerollDisponible;
        j.RealizoAccionEsteTurno = s.RealizoAccionEsteTurno;
        j.Puntuacion = s.Puntuacion;
        j.VueltasJugadas = s.VueltasJugadas;
        j.CartasMovimientoAQuitar = s.CartasMovimientoAQuitar;
        j.MovimientosExtraEsteTurno = s.MovimientosExtraEsteTurno;

        j.manoCartas = new List<CartaMovimiento>();
        foreach (int codigo in s.Mano)
        {
            j.manoCartas.Add(CartaDesdeCodigo(codigo));
        }

        j.figurasAArmar = new List<FiguraAsignada>();
        foreach (FiguraSnapshot f in s.Figuras)
        {
            j.figurasAArmar.Add(new FiguraAsignada { Figura = FiguraPorNombre(f.Nombre), Completada = f.Completada });
        }

        j.figurasEnReserva = new List<FiguraAsignada>();
        foreach (FiguraSnapshot f in s.Reserva)
        {
            j.figurasEnReserva.Add(new FiguraAsignada { Figura = FiguraPorNombre(f.Nombre), Completada = f.Completada });
        }

        j.FichasBloqueadas = new List<FichaData>();
        foreach (int id in s.FichasBloqueadas)
        {
            FichaData ficha = fichaPorId(id);
            if (ficha != null) j.FichasBloqueadas.Add(ficha);
        }

        j.FichaComodinActiva = s.FichaComodinActiva >= 0 ? fichaPorId(s.FichaComodinActiva) : null;
    }

    // ---------- Cliente: volver a disparar los eventos de la lógica ----------

    public void DispararTurnoCambiado(int jugador) { TurnoCambiado?.Invoke(jugador); }
    public void DispararUltimoColor(ColorFicha color) { UltimoColorCambiado?.Invoke(color); }
    public void DispararVictoria(string nombre) { Victoria?.Invoke(nombre); }
    public void DispararTiempoAgotado() { TiempoAgotado?.Invoke(); }
    public void DispararFiguraEncontrada(List<(int fila, int columna)> celdas) { FiguraEncontrada?.Invoke(celdas); }
    public void DispararFichaBloqueada(FichaData ficha) { FichaBloqueadaEvent?.Invoke(ficha); }
    public void DispararFichaDesbloqueada(FichaData ficha) { FichaDesbloqueadaEvent?.Invoke(ficha); }
    public void DispararFichaComodinDesactivada(FichaData ficha) { FichaComodinDesactivada?.Invoke(ficha); }

    // ---------- Cliente: reloj (solo visual) ----------

    public void AplicarTiempoRemoto(float tiempo)
    {
        TiempoRestante = tiempo;
        TemporizadorActualizado?.Invoke(TiempoRestante);
    }

    public void DescontarTiempoVisual(float delta)
    {
        if (condicionDeVictoria || TurnoEnTransicion || jugadores == null) return;
        if (TiempoRestante <= 0.0f) return;

        TiempoRestante -= delta;
        if (TiempoRestante < 0.0f) TiempoRestante = 0.0f;
        TemporizadorActualizado?.Invoke(TiempoRestante);
    }

    // ---------- Códigos de cartas ----------

    private static int CodigoCarta(CartaMovimiento carta)
    {
        switch (carta)
        {
            case MovimientoEnL: return 0;
            case MovimientoLateralConEspacio: return 1;
            case MovimientoLateralContiguo: return 2;
            case MovimientoDiagonalContiguo: return 3;
            case MovimientoDiagonalConEspacio: return 4;
            case MovimientoLateralAlBorde: return 5;
            default: return -1;
        }
    }

    private static CartaMovimiento CartaDesdeCodigo(int codigo)
    {
        switch (codigo)
        {
            case 0: return new MovimientoEnL { DireccionPermitida = MovimientoEnL.TipoDireccion.Cualquiera };
            case 1: return new MovimientoLateralConEspacio();
            case 2: return new MovimientoLateralContiguo();
            case 3: return new MovimientoDiagonalContiguo();
            case 4: return new MovimientoDiagonalConEspacio();
            case 5: return new MovimientoLateralAlBorde();
            default: return null;
        }
    }

    // Las figuras se identifican por su Nombre (único en el mazo)
    private static CartaFigura FiguraPorNombre(string nombre)
    {
        return MazoFiguras.GetInstance().cartas.Find(c => c.Nombre == nombre);
    }
}