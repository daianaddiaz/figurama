using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class Controller
{
    public static Controller _instance;

    public List<string> NombresJugadores { get; set; } = new List<string>();
    public int CantidadCartasMovimiento { get; set; } = 3;
    public int CantidadFigurasPorJugador { get; set; } = 4;

    private TableroReglas tablero;
    private Jugador[] jugadores;
    private Juego juego;
    private bool condicionDeVictoria = false;

    public int JugadorActual { get; private set; } = 0;

    public CartaMovimiento CartaSeleccionada { get; set; } = null;
    public event System.Action<int> TurnoCambiado;
    public event System.Action<string> Victoria;
    public event System.Action<float> TemporizadorActualizado;

    public event System.Action TiempoAgotado; // Aviso a UI

    public event System.Action<CartaFigura> FiguraCompletada;
    
    private bool enPausa = false; // Pausa el juego

    public int MovimientosUsadosEnTurno { get; private set; } = 0;
    public const int MAX_MOVIMIENTOS_POR_TURNO = 3;

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

                jugadores[i] = new Jugador
                {
                    nombre = nombre,
                    manoCartas = MazoMovimiento.GetInstance().generarMano(), // Trae 3 cartas iniciales
                    figurasAArmar = figuras
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

        if (tableroVisual.BuscarFigura(asignada.Figura, celdasMovidas) != null)
        {
            asignada.Completada = true;
            jugador.Puntuacion += asignada.Figura.CantidadFichas;
            completadasAhora.Add(asignada.Figura);

            // Reemplaza la figura completada por una carta nueva del mazo
            FiguraCompletada?.Invoke(asignada.Figura);
            break;
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
        
        // Remarcamos la carta como ejecutada en la mano actual
        int index = jugador.manoCartas.IndexOf(cartaUsada);
        if (index != -1)
        {
        // No puede ser seleccionada esa carta en este turno
            jugador.manoCartas[index] = null; 
        }

        MovimientosUsadosEnTurno++;

        // Si consumió las 3 cartas de su turno, se finaliza automáticamente
        if (MovimientosUsadosEnTurno >= MAX_MOVIMIENTOS_POR_TURNO)
        {
            TerminarTurno();
        }
}

    public void TerminarTurno()
{
    CartaSeleccionada = null;
    if (condicionDeVictoria || jugadores == null || jugadores.Length == 0) return;

    MovimientosUsadosEnTurno = 0;
    
    // El jugador que termina el turno o el nuevo roba 3 cartas completas
    JugadorActual = (JugadorActual + 1) % jugadores.Length;
    jugadores[JugadorActual].manoCartas = MazoMovimiento.GetInstance().generarMano();
    jugadores[JugadorActual].RerollDisponible = true;

    ReiniciarTemporizador();
    TurnoCambiado?.Invoke(JugadorActual);
}
}