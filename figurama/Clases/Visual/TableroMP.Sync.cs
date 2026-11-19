using Godot;
using System.Collections.Generic;
using System.Text.Json;

public partial class TableroMP
{
    // ---------- Snapshot y eventos (servidor -> clientes) ----------

    private PartidaSnapshot ConstruirSnapshot()
    {
        var snap = Controller.GetInstance().ExportarEstado(IdDeFicha);

        if (Fichas.Count == 0)
        {
            // Tablero todavía no creado: posiciones iniciales
            for (int i = 0; i < TableroReglas.Filas * TableroReglas.Columnas; i++)
            {
                snap.Fichas.Add(new FichaSnapshot { Posicion = i });
            }
        }
        else
        {
            foreach (Ficha ficha in Fichas)
            {
                snap.Fichas.Add(new FichaSnapshot
                {
                    Posicion = ficha.Datos.Fila * TableroReglas.Columnas + ficha.Datos.Columna,
                    EsComodin = ficha.Datos.EsComodin,
                    Bloqueada = ficha.Datos.Bloqueada
                });
            }
        }

        return snap;
    }

    private void RegistrarEvento(TipoEventoRed tipo, int[] datos = null, string texto = "")
    {
        if (!Multiplayer.IsServer()) return;

        _eventosPendientes.Add(new EventoRed
        {
            Tipo = (int)tipo,
            Datos = datos != null ? new List<int>(datos) : new List<int>(),
            Texto = texto
        });
    }

    // Manda a los clientes el estado actual y los eventos acumulados desde el último envío
    private void EnviarResultado(int indiceActor, bool fueReroll)
    {
        if (!Multiplayer.IsServer()) return;

        string snapshotJson = JsonSerializer.Serialize(ConstruirSnapshot());
        string eventosJson = JsonSerializer.Serialize(_eventosPendientes);
        _eventosPendientes.Clear();

        if (Multiplayer.GetPeers().Length == 0) return;

        Rpc(MethodName.RecibirResultado, snapshotJson, eventosJson, indiceActor, fueReroll);
    }

    // Solo la reciben los clientes. No calculan nada: aplican lo que decidió el servidor.
    [Rpc(MultiplayerApi.RpcMode.Authority)]
    public void RecibirResultado(string snapshotJson, string eventosJson, int indiceActor, bool fueReroll)
    {
        if (Multiplayer.IsServer() || !_tableroCreado) return;

        var snap = JsonSerializer.Deserialize<PartidaSnapshot>(snapshotJson);
        var eventos = JsonSerializer.Deserialize<List<EventoRed>>(eventosJson);

        Controller.GetInstance().ImportarEstado(snap, FichaPorId);

        bool huboMovimiento = AplicarTablero(snap.Fichas);
        if (huboMovimiento)
        {
            _sonidos.GetNode<AudioStreamPlayer>("CambioSFX").Play();
        }

        foreach (EventoRed evento in eventos)
        {
            AplicarEvento(evento);
        }

        if (fueReroll) RerollAplicado?.Invoke(indiceActor);
        AccionAplicada?.Invoke(indiceActor);
    }

    private bool AplicarTablero(List<FichaSnapshot> snaps)
    {
        var movidas = new List<Ficha>();

        for (int i = 0; i < Fichas.Count && i < snaps.Count; i++)
        {
            FichaData datos = Fichas[i].Datos;
            int fila = snaps[i].Posicion / TableroReglas.Columnas;
            int columna = snaps[i].Posicion % TableroReglas.Columnas;

            if (datos.Fila != fila || datos.Columna != columna)
            {
                movidas.Add(Fichas[i]);
            }

            datos.EsComodin = snaps[i].EsComodin;
            datos.Bloqueada = snaps[i].Bloqueada;
            _reglas.ColocarFicha(datos, fila, columna);
        }

        foreach (Ficha ficha in movidas)
        {
            ActualizarPosicionVisual(ficha);
        }

        return movidas.Count > 0;
    }

    // Vuelve a disparar, en el cliente, los eventos que emitió la lógica del servidor
    private void AplicarEvento(EventoRed evento)
    {
        var controller = Controller.GetInstance();

        switch ((TipoEventoRed)evento.Tipo)
        {
            case TipoEventoRed.FiguraEncontrada:
                var celdas = new List<(int fila, int columna)>();
                for (int i = 0; i + 1 < evento.Datos.Count; i += 2)
                {
                    celdas.Add((evento.Datos[i], evento.Datos[i + 1]));
                }
                controller.DispararFiguraEncontrada(celdas);
                break;

            case TipoEventoRed.FichaBloqueada:
                controller.DispararFichaBloqueada(FichaPorId(evento.Datos[0]));
                break;

            case TipoEventoRed.FichaDesbloqueada:
                controller.DispararFichaDesbloqueada(FichaPorId(evento.Datos[0]));
                break;

            case TipoEventoRed.ComodinActivado:
                EmitSignal(SignalName.ComodinActivado, Fichas[evento.Datos[0]]);
                break;

            case TipoEventoRed.ComodinDesactivado:
                controller.DispararFichaComodinDesactivada(FichaPorId(evento.Datos[0]));
                break;

            case TipoEventoRed.UltimoColor:
                controller.DispararUltimoColor((ColorFicha)evento.Datos[0]);
                break;

            case TipoEventoRed.Victoria:
                controller.DispararVictoria(evento.Texto);
                break;

            case TipoEventoRed.TurnoCambiado:
                controller.DispararTurnoCambiado(evento.Datos[0]);
                break;

            case TipoEventoRed.TiempoAgotado:
                controller.DispararTiempoAgotado();
                break;
        }
    }

    // Respuesta del servidor al jugador que mandó una acción (válida o inválida)
    private void Responder(long emisor, bool ok, string mensaje)
    {
        RpcId(emisor, MethodName.RecibirRespuesta, ok, mensaje);
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
    public void RecibirRespuesta(bool ok, string mensaje)
    {
        RespuestaServidor?.Invoke(ok, mensaje);
    }

    // Solo la reciben quienes entran a mitad de partida: alinean su música con la del host
    [Rpc(MultiplayerApi.RpcMode.Authority)]
    public void SincronizarMusica(float posicion)
    {
        if (Multiplayer.IsServer()) return;

        GetNodeOrNull<AudioStreamPlayer>("Musica/MusicaPrincipal")?.Seek(posicion);
    }
}
