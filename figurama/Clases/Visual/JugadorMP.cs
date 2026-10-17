using Godot;

public partial class JugadorMP : Node3D
{
    [Export] public PackedScene InsigniaViewScene;

    [Signal] public delegate void SeleccionadaEventHandler(Ficha ficha);
    [Signal] public delegate void DesclickeadaEventHandler(Ficha ficha);
    [Signal] public delegate void DisponibleEventHandler(Ficha ficha);

    private TableroMP _tablero;
    private Jugador _jugador;
    private int _indice;
    private CamaraJugador _camara;
    private ManoCartasView _mano;
    private Button _botonFinTurno;
    private Ficha _fichaSeleccionada;

    public override void _Ready()
    {
        _tablero = GetParent<TableroMP>();
        _indice = DatosPartida.IndiceJugadorLocal;
        _jugador = Controller.GetInstance().Jugadores()[_indice];

        _camara = GetNode<CamaraJugador>("CamaraJugador");

        _mano = GetNode<ManoCartasView>("UITemporal/Mano");
        _mano.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        _mano.MouseFilter = Control.MouseFilterEnum.Ignore;
        _mano.crearMano(_jugador, _camara);

        _botonFinTurno = GetNode<Button>("UITemporal/BotonFinTurno");
        _botonFinTurno.Pressed += OnFinTurnoPresionado;

        foreach (Ficha ficha in _tablero.Fichas)
        {
            ficha.Clickeada += OnFichaClickeada;
            Connect(SignalName.Seleccionada, new Callable(ficha, "_on_ficha_seleccionada"));
            Connect(SignalName.Desclickeada, new Callable(ficha, "_on_ficha_desclickeada"));
            Connect(SignalName.Disponible, new Callable(ficha, "_on_ficha_disponible"));
        }

        _tablero.AccionAplicada += OnAccionAplicada;
        Controller.GetInstance().TurnoCambiado += OnTurnoCambiado;

        CrearInsignia();
        ActualizarBotones();
    }

    public override void _ExitTree()
    {
        if (_tablero != null)
        {
            _tablero.AccionAplicada -= OnAccionAplicada;
        }
        if (Controller._instance != null)
        {
            Controller.GetInstance().TurnoCambiado -= OnTurnoCambiado;
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed && @event.IsActionPressed("finalizarTurno"))
        {
            OnFinTurnoPresionado();
        }
    }

    private void CrearInsignia()
    {
        var insignia = InsigniaViewScene.Instantiate<InsigniaView>();
        GetNode<Control>("UITemporal/InsigniaViewContainer").AddChild(insignia);
        insignia.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        insignia.SetInsignia(
            _jugador.nombre,
            CargadorAssetsTablero.TexturaInsignia[_jugador.PersonajeAsignado.Tipo],
            _indice
        );
    }

    private void OnFinTurnoPresionado()
    {
        _tablero.SolicitarFinTurno();
    }

    private void ActualizarBotones()
    {
        var controller = Controller.GetInstance();
        bool miTurno = controller.JugadorActual == _indice && !controller.JuegoTerminado;

        _botonFinTurno.Disabled = !(miTurno && _jugador.RealizoAccionEsteTurno);

        var botonReroll = _mano.GetNodeOrNull<Button>("BotonReroll");
        if (botonReroll != null)
        {
            botonReroll.Disabled = true; // el reroll en red llega en el paso 5
        }
    }

    private void OnTurnoCambiado(int jugadorActual)
    {
        if (_fichaSeleccionada != null)
        {
            _fichaSeleccionada = null;
            DesalumbrarFichas();
        }

        _mano.ActualizarMano();
        ActualizarBotones();
    }

    private void OnAccionAplicada(int indiceJugador)
    {
        _mano.ActualizarMano();

        if (indiceJugador == _indice)
        {
            _fichaSeleccionada = null;
            Controller.GetInstance().CambiarCartaSeleccionada(null);
            DesalumbrarFichas();
        }

        ActualizarBotones();
    }

    private void OnFichaClickeada(Ficha ficha)
    {
        var controller = Controller.GetInstance();

        if (!_camara.levantada) return;
        if (controller.JuegoTerminado || controller.JugadorActual != _indice) return;

        if (ficha.Datos.Bloqueada && _fichaSeleccionada == null) return;

        if (_fichaSeleccionada == null)
        {
            _fichaSeleccionada = ficha;
            EmitSignal(SignalName.Seleccionada, ficha);
            AlumbrarFichasDisponibles();
            return;
        }

        if (_fichaSeleccionada == ficha)
        {
            EmitSignal(SignalName.Desclickeada, ficha);
            _fichaSeleccionada = null;

            controller.CambiarCartaSeleccionada(null);

            DesalumbrarFichas();
            AlumbrarFichasDisponibles();
            return;
        }

        CartaMovimiento carta = controller.CartaSeleccionada;

        if (carta != null && !ficha.Datos.Bloqueada &&
            carta.EsValido(_tablero.Reglas, _fichaSeleccionada.Datos.Fila, _fichaSeleccionada.Datos.Columna, ficha.Datos.Fila, ficha.Datos.Columna))
        {
            int indiceCarta = _jugador.manoCartas.IndexOf(carta);
            if (indiceCarta >= 0)
            {
                _tablero.SolicitarMovimiento(
                    indiceCarta,
                    _fichaSeleccionada.Datos.Fila, _fichaSeleccionada.Datos.Columna,
                    ficha.Datos.Fila, ficha.Datos.Columna
                );
            }
        }

        DesalumbrarFichas();
        _fichaSeleccionada = null;
    }

    private void DesalumbrarFichas()
    {
        foreach (Ficha ficha in _tablero.Fichas)
        {
            if (ficha.Datos.Bloqueada == false)
            {
                EmitSignal(SignalName.Desclickeada, ficha);
            }
        }
    }

    private void AlumbrarFichasDisponibles()
    {
        CartaMovimiento cartaActiva = Controller.GetInstance().CartaSeleccionada;
        if (cartaActiva == null) return;

        if (_fichaSeleccionada != null)
        {
            foreach (Ficha destino in _tablero.Fichas)
            {
                if (destino != _fichaSeleccionada)
                {
                    AlumbrarSiEsValida(destino);
                }
            }
        }
        else
        {
            foreach (Ficha origen in _tablero.Fichas)
            {
                bool tieneOpcionValida = false;

                for (int f2 = 0; f2 < TableroReglas.Filas && !tieneOpcionValida; f2++)
                {
                    for (int c2 = 0; c2 < TableroReglas.Columnas; c2++)
                    {
                        if (origen.Datos.Fila == f2 && origen.Datos.Columna == c2) continue;

                        if (cartaActiva.EsValido(_tablero.Reglas, origen.Datos.Fila, origen.Datos.Columna, f2, c2))
                        {
                            tieneOpcionValida = true;
                            break;
                        }
                    }
                }

                if (tieneOpcionValida)
                {
                    EmitSignal(SignalName.Disponible, origen);
                }
            }
        }
    }

    private void AlumbrarSiEsValida(Ficha ficha)
    {
        if (ficha.Datos.Bloqueada) return;

        CartaMovimiento cartaActiva = Controller.GetInstance().CartaSeleccionada;
        if (cartaActiva != null && _fichaSeleccionada != null)
        {
            if (cartaActiva.EsValido(_tablero.Reglas, _fichaSeleccionada.Datos.Fila, _fichaSeleccionada.Datos.Columna, ficha.Datos.Fila, ficha.Datos.Columna))
            {
                EmitSignal(SignalName.Disponible, ficha);
            }
        }
    }
}