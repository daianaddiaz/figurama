using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class Controller
{
    public static Controller _instance;

    public List<string> NombresJugadores { get; set; } = new List<string>();
    public List<TipoHabilidad> PersonajesElegidos { get; set; } = new List<TipoHabilidad>();
    public int CantidadCartasMovimiento { get; set; } = 3;
    public int CantidadFigurasPorJugador { get; set; } = 4;
    public ColorFicha? UltimoColorUsado { get; private set; }
    

    private TableroReglas tablero;
    private Jugador[] jugadores;
    private Juego juego;
    private bool condicionDeVictoria = false;

    public int JugadorActual { get; private set; } = 0;

    public CartaMovimiento CartaSeleccionada { get; set; } = null;
    public event System.Action<int> TurnoCambiado;
    public event System.Action<CartaMovimiento> CartaUsadaEvent;
    public event System.Action<ColorFicha> UltimoColorCambiado;
    public event System.Action<string> Victoria;
    public event System.Action<float> TemporizadorActualizado;
    public event System.Action<FichaData> FichaComodinDesactivada;

    public event System.Action TiempoAgotado; // Aviso a UI

    public event System.Action<CartaFigura> FiguraCompletada;
    
    private bool enPausa = false; // Pausa el juego

    public int MovimientosUsadosEnTurno { get; private set; } = 0;
    public int MaxMovimientosPorTurnoBase { get; set; } = 3;

    // Temporizador

    private const float TIEMPO_MAXIMO_TURNO = 120.0f; // 2 minutos
    public float TiempoRestante { get; private set; } = TIEMPO_MAXIMO_TURNO;
    private bool temporizadorActivo = true;

    public bool JuegoTerminado => condicionDeVictoria;

    private Controller() { }

    public static Controller GetInstance()
    {
        if (_instance == null)
        {
            _instance = new Controller();
            _instance.InicializarController();
        }
        return _instance;
    }

    public void InicializarController()
    {
        tablero = new TableroReglas();
        juego = new Juego();
    }

    public bool RerollearManoActual()
    {
        if (condicionDeVictoria) return false;
        if (jugadores == null || jugadores.Length == 0) return false;

        Jugador jugador = jugadores[JugadorActual];
        if (!jugador.RerollDisponible) return false;

        jugador.manoCartas = MazoMovimiento.GetInstance().generarMano();
        jugador.RerollDisponible = false;
        CartaSeleccionada = null;

        TerminarTurno();
        return true;
    }

    public void InicializarJugadores()
    {   
        TurnoCambiado = null;
        Victoria = null;
        UltimoColorCambiado = null;
        CartaUsadaEvent = null;
        TemporizadorActualizado = null;
        TiempoAgotado = null;
        FiguraCompletada = null;
            
        int cantidad = NombresJugadores.Count > 0 ? NombresJugadores.Count : 4;
        jugadores = new Jugador[cantidad];

        for (int i = 0; i < cantidad; i++)
        {
            string nombre = i < NombresJugadores.Count ? NombresJugadores[i] : $"Jugador {i + 1}";

            var figuras = new List<FiguraAsignada>();
            foreach (CartaFigura figura in MazoFiguras.GetInstance().generarMano(CantidadFigurasPorJugador))
            {
                figuras.Add(new FiguraAsignada { Figura = figura, Completada = false });
            }

            TipoHabilidad tipo = i < PersonajesElegidos.Count ? PersonajesElegidos[i] : TipoHabilidad.Lobizon;

            jugadores[i] = new Jugador
            {
                nombre = nombre,
                manoCartas = MazoMovimiento.GetInstance().generarMano(), // Trae 3 cartas iniciales
                figurasAArmar = figuras,
                PersonajeAsignado = new Personaje { Nombre = nombre, Tipo = tipo }
            };
        }

        JugadorActual = 0;
        condicionDeVictoria = false;
        enPausa = false; 
        MovimientosUsadosEnTurno = 0;
        ReiniciarTemporizador();
    }

    // Metodo para descontar tiempo

    private float _tiempoEsperaPausa = 0.0f;

    public void ActualizarTiempo(float delta)
    {
        if (condicionDeVictoria || jugadores == null || jugadores.Length == 0) return;

        // Manejo del tiempo de espera post-agotado (3 segundos de pausa)
        if (enPausa)
        {
            _tiempoEsperaPausa -= delta;
            if (_tiempoEsperaPausa <= 0.0f)
            {
                enPausa = false;
                TerminarTurno();
            }
            return;
        }

        if (!temporizadorActivo) return;

        TiempoRestante -= delta;

        if (TiempoRestante <= 0.0f)
        {
            TiempoRestante = 0.0f;
            temporizadorActivo = false;
            enPausa = true;
            _tiempoEsperaPausa = 3.0f; // Pausa de 3 segundos dentro del bucle principal

            TemporizadorActualizado?.Invoke(TiempoRestante);
            TiempoAgotado?.Invoke();
        }
        else
        {
            TemporizadorActualizado?.Invoke(TiempoRestante);
        }
    }

    private void ReiniciarTemporizador()
    {
        TiempoRestante = TIEMPO_MAXIMO_TURNO;
        enPausa = false;
        temporizadorActivo = true;
        TemporizadorActualizado?.Invoke(TiempoRestante);
    }

    public void CambiarCartaSeleccionada(CartaMovimiento carta)
    {
        CartaSeleccionada = carta;
    }

    public Jugador[] Jugadores() => jugadores;

    public string NombreJugadorActual() => jugadores != null && jugadores.Length > JugadorActual ? jugadores[JugadorActual].nombre : "";

    public List<CartaMovimiento> ManoJugadorActual() => jugadores != null && jugadores.Length > JugadorActual ? jugadores[JugadorActual].manoCartas : new List<CartaMovimiento>();

    public List<FiguraAsignada> FigurasJugadorActual() => jugadores != null && jugadores.Length > JugadorActual ? jugadores[JugadorActual].figurasAArmar : new List<FiguraAsignada>();

    public CartaMovimiento MovimientoActual() => CartaSeleccionada;

    public List<CartaFigura> ChequearFigurasCompletadas(TableroReglas tableroVisual, HashSet<(int fila, int columna)> celdasMovidas)
    {
        var completadasAhora = new List<CartaFigura>();
        Jugador jugador = jugadores[JugadorActual];

        for (int i = 0; i < jugador.figurasAArmar.Count; i++)
        {
            FiguraAsignada asignada = jugador.figurasAArmar[i];
            if (asignada.Completada) continue;

            var celdas = tableroVisual.BuscarFigura(asignada.Figura, celdasMovidas);

            if (celdas != null)
            {
                ColorFicha colorFormado = tableroVisual.ObtenerFicha(celdas[0].fila, celdas[0].columna).Color;
                
                if (UltimoColorUsado.HasValue && colorFormado == UltimoColorUsado.Value) continue;

                asignada.Completada = true;
                jugador.Puntuacion += asignada.Figura.CantidadFichas;
                completadasAhora.Add(asignada.Figura);

                UltimoColorUsado = colorFormado;
                UltimoColorCambiado?.Invoke(colorFormado);

                // Reemplaza la figura completada por una carta nueva del mazo
                FiguraCompletada?.Invoke(asignada.Figura);
            }
        }

        if (jugador.figurasAArmar.TrueForAll(f => f.Completada))
        {
            condicionDeVictoria = true;
            temporizadorActivo = false;
            Victoria?.Invoke(jugador.nombre);
        }

        return completadasAhora;
    }

    public void RegistrarMovimientoRealizado(CartaMovimiento cartaUsada)
    {
        Jugador jugador = jugadores[JugadorActual];

        int index = jugador.manoCartas.IndexOf(cartaUsada);
        if (index != -1)
        {
            jugador.manoCartas[index] = null;
        }

        MovimientosUsadosEnTurno++;

        CartaUsadaEvent?.Invoke(cartaUsada);

        int limiteEsteTurno = MaxMovimientosPorTurnoBase + jugador.MovimientosExtraEsteTurno;
        if (MovimientosUsadosEnTurno >= limiteEsteTurno)
        {
            TerminarTurno();
        }
    }

    public void TerminarTurno()
    {
        CartaSeleccionada = null;
        if (condicionDeVictoria || jugadores == null || jugadores.Length == 0) return;

        MovimientosUsadosEnTurno = 0;
        jugadores[JugadorActual].MovimientosExtraEsteTurno = 0;

        Jugador jugadorSaliente = jugadores[JugadorActual];
        jugadorSaliente.VueltasJugadas++;

        if (jugadorSaliente.FichaComodinActiva != null)
        {
            jugadorSaliente.FichaComodinActiva.EsComodin = false;
            FichaComodinDesactivada?.Invoke(jugadorSaliente.FichaComodinActiva);
            jugadorSaliente.FichaComodinActiva = null;
        }

        JugadorActual = (JugadorActual + 1) % jugadores.Length;
        Jugador jugadorEntrante = jugadores[JugadorActual];

        jugadorEntrante.manoCartas = MazoMovimiento.GetInstance().generarMano();

        if (jugadorEntrante.CartasMovimientoAQuitar > 0)
        {
            int aQuitar = jugadorEntrante.CartasMovimientoAQuitar;
            for (int i = 0; i < aQuitar && jugadorEntrante.manoCartas.Count > 0; i++)
            {
                jugadorEntrante.manoCartas.RemoveAt(jugadorEntrante.manoCartas.Count - 1);
            }
            jugadorEntrante.CartasMovimientoAQuitar = 0;
        }

        if (jugadorEntrante.PersonajeAsignado.Tipo == TipoHabilidad.Mulanima)
        {
            var habilidadMulanima = new HabilidadMulanima();
            habilidadMulanima.LiberarTodas(jugadorEntrante);
        }

        jugadorEntrante.RerollDisponible = true;

        ReiniciarTemporizador();
        TurnoCambiado?.Invoke(JugadorActual);
    }
    
    private Habilidad ObtenerHabilidad(TipoHabilidad tipo)
    {
        return tipo switch
        {
            TipoHabilidad.Lobizon => new HabilidadLobizon(),
            TipoHabilidad.LuzMala => new HabilidadLuzMala(),
            TipoHabilidad.Pomberito => new HabilidadPomberito(),
            TipoHabilidad.Mulanima => new HabilidadMulanima(),
            _ => null
        };
    }

    public bool PuedeUsarHabilidad()
    {
        if (jugadores == null || jugadores.Length == 0) return false;
        Jugador jugador = jugadores[JugadorActual];
        Habilidad habilidad = ObtenerHabilidad(jugador.PersonajeAsignado.Tipo);
        return habilidad.PuedeActivarse(jugador.PersonajeAsignado, jugador);
    }

    public bool ActivarHabilidadLobizon(FichaData ficha)
    {
        if (!PuedeUsarHabilidad()) return false;
        Jugador jugador = jugadores[JugadorActual];
        if (jugador.PersonajeAsignado.Tipo != TipoHabilidad.Lobizon) return false;

        var habilidad = (HabilidadLobizon)ObtenerHabilidad(TipoHabilidad.Lobizon);
        habilidad.Activar(jugador, ficha);
        habilidad.MarcarUsada(jugador.PersonajeAsignado, jugador);
        return true;
    }

    public bool ActivarHabilidadPomberito()
    {
        if (!PuedeUsarHabilidad()) return false;
        Jugador jugador = jugadores[JugadorActual];
        if (jugador.PersonajeAsignado.Tipo != TipoHabilidad.Pomberito) return false;

        var habilidad = (HabilidadPomberito)ObtenerHabilidad(TipoHabilidad.Pomberito);
        habilidad.Activar(jugador);
        habilidad.MarcarUsada(jugador.PersonajeAsignado, jugador);
        return true;
    }

    public bool ActivarHabilidadLuzMala(Jugador jugadorObjetivo)
    {
        if (!PuedeUsarHabilidad()) return false;
        Jugador jugador = jugadores[JugadorActual];
        if (jugador.PersonajeAsignado.Tipo != TipoHabilidad.LuzMala) return false;

        var habilidad = (HabilidadLuzMala)ObtenerHabilidad(TipoHabilidad.LuzMala);
        habilidad.Activar(jugadorObjetivo);
        habilidad.MarcarUsada(jugador.PersonajeAsignado, jugador);
        return true;
    }

    public bool ActivarHabilidadMulanima(FichaData ficha)
    {
        if (!PuedeUsarHabilidad()) return false;
        Jugador jugador = jugadores[JugadorActual];
        if (jugador.PersonajeAsignado.Tipo != TipoHabilidad.Mulanima) return false;

        var habilidad = (HabilidadMulanima)ObtenerHabilidad(TipoHabilidad.Mulanima);
        habilidad.Activar(jugador, ficha);
        habilidad.MarcarUsada(jugador.PersonajeAsignado, jugador);
        return true;
    }
}