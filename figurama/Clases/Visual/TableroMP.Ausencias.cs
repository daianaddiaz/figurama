using Godot;

public partial class TableroMP
{
    // ---------- Ausencias, eliminación y fin de partida ----------

    private void PausarPorAusencia()
    {
        _pausadoPorAusencia = true;
        Controller.GetInstance().PausarTemporizador(true);
    }

    private void OnJugadorAusente(int indice)
    {
        var controller = Controller.GetInstance();
        if (controller.JuegoTerminado) return;

        // Si el que se fue está en su turno, el juego espera a que vuelva
        if (indice == controller.JugadorActual && !controller.TurnoEnTransicion)
        {
            PausarPorAusencia();
        }
    }

    private void OnJugadorVolvio(int indice)
    {
        var controller = Controller.GetInstance();
        if (controller.JuegoTerminado) return;

        if (_pausadoPorAusencia && indice == controller.JugadorActual)
        {
            _pausadoPorAusencia = false;
            controller.PausarTemporizador(false);
        }
    }

    private void OnJugadorExpulsado(int indice)
    {
        var controller = Controller.GetInstance();
        if (controller.JuegoTerminado) return;

        bool eraSuTurno = indice == controller.JugadorActual;

        // Libera su comodín y sus bloqueos, y lo marca como eliminado
        controller.EliminarJugador(indice);

        if (controller.JugadoresActivos() < 2)
        {
            string motivo = "Quedan menos de 2 jugadores.\nLa partida terminó sin ganador.";
            if (Multiplayer.GetPeers().Length > 0)
            {
                Rpc(MethodName.RecibirFinPartida, motivo);
            }
            FinalizarPartida(motivo);
            return;
        }

        if (eraSuTurno && !controller.TurnoEnTransicion)
        {
            _pausadoPorAusencia = false;
            controller.TerminarTurno();
        }

        EnviarResultado(-1, false);
    }

    [Rpc(MultiplayerApi.RpcMode.Authority)]
    public void RecibirFinPartida(string motivo)
    {
        if (Multiplayer.IsServer()) return;
        FinalizarPartida(motivo);
    }

    private void OnServidorDesconectado()
    {
        FinalizarPartida("Se perdió la conexión con el host.\nLa partida terminó.");
    }

    private void FinalizarPartida(string motivo)
    {
        if (_partidaCortada) return;
        if (Controller.GetInstance().JuegoTerminado) return;

        _partidaCortada = true;
        Controller.GetInstance().CortarPartida();

        MostrarAviso("");
        GetNode<Label>("UITemporal/PanelVictoria/LabelGanador").Text = motivo;
        Estadisticas.Mostrar(GetNodeOrNull<Label>("UITemporal/PanelVictoria/LabelEstadisticas"));
        GetNode<Control>("UITemporal/PanelVictoria").Show();

        // Refresca los botones del jugador (quedan deshabilitados)
        AccionAplicada?.Invoke(-1);
    }
}
