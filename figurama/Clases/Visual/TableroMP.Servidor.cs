using Godot;
using System.Collections.Generic;

public partial class TableroMP
{
    // ---------- Validación (servidor) ----------

    private string ValidarTurno(long emisor, out int indiceEmisor)
    {
        var controller = Controller.GetInstance();
        indiceEmisor = DatosPartida.IdsJugadores.IndexOf(emisor);

        if (indiceEmisor < 0) return "No sos parte de esta partida.";
        if (controller.Jugadores()[indiceEmisor].Eliminado) return "Fuiste eliminado de la partida.";
        if (controller.JuegoTerminado) return "La partida ya terminó.";
        if (controller.TurnoEnTransicion) return "El turno está cambiando, esperá un momento.";
        if (indiceEmisor != controller.JugadorActual) return "No es tu turno.";
        return null;
    }

    private string ValidarMovimiento(int indiceJugador, int indiceCarta, int filaA, int colA, int filaB, int colB)
    {
        var jugador = Controller.GetInstance().Jugadores()[indiceJugador];

        if (indiceCarta < 0 || indiceCarta >= jugador.manoCartas.Count) return "Carta inválida.";

        CartaMovimiento carta = jugador.manoCartas[indiceCarta];
        if (carta == null) return "Esa carta ya fue usada.";

        if (!EnTablero(filaA, colA) || !EnTablero(filaB, colB)) return "Posición fuera del tablero.";

        FichaData fichaA = _reglas.ObtenerFicha(filaA, colA);
        FichaData fichaB = _reglas.ObtenerFicha(filaB, colB);
        if (fichaA == null || fichaB == null) return "Ficha inexistente.";
        if (fichaA.Bloqueada || fichaB.Bloqueada) return "No podés mover una ficha bloqueada.";

        if (!carta.EsValido(_reglas, filaA, colA, filaB, colB)) return "Ese movimiento no es válido para la carta elegida.";
        return null;
    }

    private string ValidarHabilidad(int indiceJugador, int tipo, int fila, int columna, int indiceObjetivo)
    {
        var controller = Controller.GetInstance();

        if (!controller.PuedeUsarHabilidad()) return "La habilidad todavía se está recargando.";

        var jugador = controller.Jugadores()[indiceJugador];
        if ((int)jugador.PersonajeAsignado.Tipo != tipo) return "Esa no es tu habilidad.";

        var tipoHabilidad = (TipoHabilidad)tipo;

        if (tipoHabilidad == TipoHabilidad.Lobizon || tipoHabilidad == TipoHabilidad.Mulanima)
        {
            if (!EnTablero(fila, columna)) return "Posición fuera del tablero.";

            FichaData ficha = _reglas.ObtenerFicha(fila, columna);
            if (ficha == null) return "Ficha inexistente.";
            if (ficha.Bloqueada) return "No podés usar la habilidad sobre una ficha bloqueada.";
        }

        if (tipoHabilidad == TipoHabilidad.LuzMala)
        {
            if (indiceObjetivo < 0 || indiceObjetivo >= controller.Jugadores().Length || indiceObjetivo == indiceJugador)
            {
                return "Jugador objetivo inválido.";
            }
            if (controller.Jugadores()[indiceObjetivo].Eliminado) return "Ese jugador ya fue eliminado.";
        }

        return null;
    }

    // ---------- Mover una ficha ----------

    public void SolicitarMovimiento(int indiceCarta, int filaA, int colA, int filaB, int colB)
    {
        RpcId(1, MethodName.PedirMovimiento, indiceCarta, filaA, colA, filaB, colB);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public void PedirMovimiento(int indiceCarta, int filaA, int colA, int filaB, int colB)
    {
        if (!Multiplayer.IsServer()) return;

        long emisor = EmisorActual();
        string error = ValidarTurno(emisor, out int indice);
        if (error == null) error = ValidarMovimiento(indice, indiceCarta, filaA, colA, filaB, colB);

        if (error != null)
        {
            Responder(emisor, false, error);
            return;
        }

        EjecutarMovimiento(indice, indiceCarta, filaA, colA, filaB, colB);
        Responder(emisor, true, "Movimiento realizado.");
        EnviarResultado(indice, false);
    }

    private void EjecutarMovimiento(int indiceJugador, int indiceCarta, int filaA, int colA, int filaB, int colB)
    {
        var controller = Controller.GetInstance();
        CartaMovimiento carta = controller.Jugadores()[indiceJugador].manoCartas[indiceCarta];

        Ficha nodoA = GetFichaEnPosicion(filaA, colA);
        Ficha nodoB = GetFichaEnPosicion(filaB, colB);

        carta.Ejecutar(_reglas, filaA, colA, filaB, colB);
        _sonidos.GetNode<AudioStreamPlayer>("CambioSFX").Play();

        ActualizarPosicionVisual(nodoA);
        ActualizarPosicionVisual(nodoB);

        var celdasMovidas = new HashSet<(int fila, int columna)>
        {
            (nodoA.Datos.Fila, nodoA.Datos.Columna),
            (nodoB.Datos.Fila, nodoB.Datos.Columna)
        };

        controller.ChequearFigurasCompletadas(_reglas, celdasMovidas);
        controller.RegistrarMovimientoRealizado(carta);

        GD.Print($"Movimiento aplicado: jugador {indiceJugador}, carta {carta.Nombre}, ({filaA},{colA}) -> ({filaB},{colB})");
        AccionAplicada?.Invoke(indiceJugador);
    }

    // ---------- Finalizar turno ----------

    public void SolicitarFinTurno()
    {
        RpcId(1, MethodName.PedirFinTurno);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public void PedirFinTurno()
    {
        if (!Multiplayer.IsServer()) return;

        long emisor = EmisorActual();
        string error = ValidarTurno(emisor, out int indice);
        if (error == null && !Controller.GetInstance().Jugadores()[indice].RealizoAccionEsteTurno)
        {
            error = "Tenés que realizar al menos una acción antes de terminar el turno.";
        }

        if (error != null)
        {
            Responder(emisor, false, error);
            return;
        }

        Controller.GetInstance().TerminarTurno();
        FinTurnoAplicado?.Invoke();
        Responder(emisor, true, "Turno finalizado.");
        EnviarResultado(-1, false);
    }

    // ---------- Habilidades ----------

    public void SolicitarHabilidad(TipoHabilidad tipo, int fila, int columna, int indiceObjetivo)
    {
        RpcId(1, MethodName.PedirHabilidad, (int)tipo, fila, columna, indiceObjetivo);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public void PedirHabilidad(int tipo, int fila, int columna, int indiceObjetivo)
    {
        if (!Multiplayer.IsServer()) return;

        long emisor = EmisorActual();
        string error = ValidarTurno(emisor, out int indice);
        if (error == null) error = ValidarHabilidad(indice, tipo, fila, columna, indiceObjetivo);
        if (error == null) error = EjecutarHabilidad(indice, tipo, fila, columna, indiceObjetivo);

        if (error != null)
        {
            Responder(emisor, false, error);
            return;
        }

        Responder(emisor, true, "Habilidad activada.");
        EnviarResultado(indice, false);
    }

    private string EjecutarHabilidad(int indiceJugador, int tipo, int fila, int columna, int indiceObjetivo)
    {
        var controller = Controller.GetInstance();
        var tipoHabilidad = (TipoHabilidad)tipo;

        Ficha nodoFicha = null;
        FichaData ficha = null;
        if (tipoHabilidad == TipoHabilidad.Lobizon || tipoHabilidad == TipoHabilidad.Mulanima)
        {
            nodoFicha = GetFichaEnPosicion(fila, columna);
            ficha = nodoFicha.Datos;
        }

        Jugador objetivo = null;
        if (tipoHabilidad == TipoHabilidad.LuzMala)
        {
            objetivo = controller.Jugadores()[indiceObjetivo];
        }

        if (!controller.ActivarHabilidad(tipoHabilidad, ficha, objetivo))
        {
            return "No se pudo activar la habilidad.";
        }

        if (tipoHabilidad == TipoHabilidad.Lobizon)
        {
            EmitSignal(SignalName.ComodinActivado, nodoFicha);
            RegistrarEvento(TipoEventoRed.ComodinActivado, new[] { IdDeFicha(ficha) });

            var celdaComodin = new HashSet<(int fila, int columna)>
            {
                (ficha.Fila, ficha.Columna)
            };
            controller.ChequearFigurasCompletadas(_reglas, celdaComodin);
        }

        GD.Print($"Habilidad aplicada: jugador {indiceJugador}, {tipoHabilidad}");
        AccionAplicada?.Invoke(indiceJugador);
        return null;
    }

    // ---------- Reroll ----------

    public void SolicitarReroll()
    {
        RpcId(1, MethodName.PedirReroll);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public void PedirReroll()
    {
        if (!Multiplayer.IsServer()) return;

        long emisor = EmisorActual();
        string error = ValidarTurno(emisor, out int indice);
        if (error == null)
        {
            var jugador = Controller.GetInstance().Jugadores()[indice];
            if (jugador.RealizoAccionEsteTurno) error = "Solo podés rerollear antes de realizar una acción.";
            else if (!jugador.RerollDisponible) error = "Ya usaste el reroll este turno.";
        }

        if (error != null)
        {
            Responder(emisor, false, error);
            return;
        }

        Controller.GetInstance().RerollearManoActual();
        RerollAplicado?.Invoke(indice);
        AccionAplicada?.Invoke(indice);

        Responder(emisor, true, "Mano rerolleada.");
        EnviarResultado(indice, true);
    }
}
