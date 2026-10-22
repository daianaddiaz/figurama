using Godot;

public partial class JugadorMP : Node3D
{
    [Export] public PackedScene InsigniaViewScene;
    [Export] public PackedScene CinematicaScene;
    [Export] public VideoStreamTheora CinematicaLobizon;
    [Export] public VideoStreamTheora CinematicaLuzMala;
    [Export] public VideoStreamTheora CinematicaMulanima;
    [Export] public VideoStreamTheora CinematicaPomberito;
    [Export] public PackedScene TransicionScene;
    [Export] public Texture2D[] FondosJugadores;

    [Signal] public delegate void SeleccionadaEventHandler(Ficha ficha);
    [Signal] public delegate void DesclickeadaEventHandler(Ficha ficha);
    [Signal] public delegate void DisponibleEventHandler(Ficha ficha);

    private TableroMP _tablero;
    private Jugador _jugador;
    private int _indice;
    private bool _modoSeleccionHabilidadActivo = false;
    private CamaraJugador _camara;
    private ManoCartasView _mano;
    private Button _botonFinTurno;
    private Ficha _fichaSeleccionada;
    private Sprite3D _fondo;
    private InsigniaView _insigniaPrincipal;

    public override void _Ready()
    {
        _tablero = GetParent<TableroMP>();
        _indice = DatosPartida.IndiceJugadorLocal;
        _jugador = Controller.GetInstance().Jugadores()[_indice];

        _camara = GetNode<CamaraJugador>("CamaraJugador");
        _fondo = GetNodeOrNull<Sprite3D>("CamaraJugador/Fondo");
        if (_fondo == null)
        {
            GD.PrintErr("[JugadorMP] No se encontró el nodo 'CamaraJugador/Fondo'.");
        }
        ActualizarFondo(Controller.GetInstance().JugadorActual);

        _mano = GetNode<ManoCartasView>("UITemporal/Mano");
        _mano.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        _mano.MouseFilter = Control.MouseFilterEnum.Ignore;
        _mano.crearMano(_jugador, _camara);

        _botonFinTurno = GetNode<Button>("UITemporal/BotonFinTurno");
        _botonFinTurno.Pressed += OnFinTurnoPresionado;

        var botonHabilidad = GetNodeOrNull<TextureButton>("UITemporal/BotonHabilidad");
        if (botonHabilidad != null)
        {
            botonHabilidad.Pressed += OnHabilidadPresionada;
        }

        foreach (Ficha ficha in _tablero.Fichas)
        {
            ficha.Clickeada += OnFichaClickeada;
            Connect(SignalName.Seleccionada, new Callable(ficha, "_on_ficha_seleccionada"));
            Connect(SignalName.Desclickeada, new Callable(ficha, "_on_ficha_desclickeada"));
            Connect(SignalName.Disponible, new Callable(ficha, "_on_ficha_disponible"));
        }

        _tablero.AccionAplicada += OnAccionAplicada;
        _tablero.RerollAplicado += OnRerollAplicado;
        _tablero.FinTurnoAplicado += ActualizarBotones;
        _mano.RerollSolicitado += OnRerollSolicitado;
        Controller.GetInstance().TurnoCambiado += OnTurnoCambiado;

        CrearInsignia();
        CrearHabilidad();
        ActualizarInsigniasSecundarias();
        var botonPausa = GetNodeOrNull<BaseButton>("UITemporal/BotonPausa");
        if (botonPausa != null)
        {
            botonPausa.Pressed += OnPausaPresionada;
        }
        ActualizarBotones();
    }

    public override void _ExitTree()
    {
        if (_tablero != null)
        {
            _tablero.AccionAplicada -= OnAccionAplicada;
            _tablero.RerollAplicado -= OnRerollAplicado;
            _tablero.FinTurnoAplicado -= ActualizarBotones;
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
        _insigniaPrincipal = InsigniaViewScene.Instantiate<InsigniaView>();
        GetNode<Control>("UITemporal/InsigniaViewContainer").AddChild(_insigniaPrincipal);
        _insigniaPrincipal.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        SetInsigniaPrincipal();
    }

    private void SetInsigniaPrincipal()
    {
        Jugador jugadorDeTurno = Controller.GetInstance().Jugadores()[_indice];
        _insigniaPrincipal.SetInsignia(
            jugadorDeTurno.nombre,
            CargadorAssetsTablero.TexturaInsignia[jugadorDeTurno.PersonajeAsignado.Tipo],
            _indice
        );
    }

    private void CrearHabilidad()
    {
        var botonHabilidad = GetNodeOrNull<TextureButton>("UITemporal/BotonHabilidad");
        if (botonHabilidad == null) return;

        TipoHabilidad tipo = _jugador.PersonajeAsignado.Tipo;

        botonHabilidad.TextureNormal = GD.Load<Texture2D>(CargadorAssetsTablero.TexturaDisponibleHabilidad[tipo]);
        botonHabilidad.TextureDisabled = GD.Load<Texture2D>(CargadorAssetsTablero.TexturaNoDisponibleHabilidad[tipo]);
        botonHabilidad.TooltipText = CargadorAssetsTablero.NombreBotonHabilidad[tipo];
    }

    private void ActualizarFondo(int indiceJugador)
    {
        if (_fondo == null || FondosJugadores == null) return;
        if (indiceJugador < 0 || indiceJugador >= FondosJugadores.Length) return;

        if (FondosJugadores[indiceJugador] != null)
        {
            _fondo.Texture = FondosJugadores[indiceJugador];
        }
    }

    private void ActualizarInsigniasSecundarias()
    {
        var contenedor = GetNodeOrNull<Control>("UITemporal/InsigniaViewContainer/InsigniasSecundarias");
        if (contenedor == null)
        {
            GD.PrintErr("[JugadorMP] No se encontró 'UITemporal/InsigniaViewContainer/InsigniasSecundarias'.");
            return;
        }

        foreach (Node hijo in contenedor.GetChildren())
        {
            contenedor.RemoveChild(hijo);
            hijo.QueueFree();
        }

        var jugadores = Controller.GetInstance().Jugadores();
        for (int i = 0; i < jugadores.Length; i++)
        {
            if (i == _indice) continue;

            var insignia = InsigniaViewScene.Instantiate<InsigniaView>();
            contenedor.AddChild(insignia);
            insignia.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
            insignia.SetInsigniaSecundariaJugador(
                jugadores[i],
                CargadorAssetsTablero.TexturaInsignia[jugadores[i].PersonajeAsignado.Tipo],
                i
            );
        }
        CambiarTurnoInsignias();
    }

    private void CambiarTurnoInsignias()
    {
        var contenedor = GetNodeOrNull<Control>("UITemporal/InsigniaViewContainer/InsigniasSecundarias");
        var insigniasSecundarias = contenedor.GetChildren();
        foreach(InsigniaView insignia in insigniasSecundarias)
        {
            insignia.SetDecoracion(Controller.GetInstance().IndiceJugadorActual());
        }
        _insigniaPrincipal.SetDecoracion(Controller.GetInstance().IndiceJugadorActual());
    }

    private void OnPausaPresionada()
    {
        GetNodeOrNull<MenuPausa>("UITemporal/MenuPausa")?.TogglePausa();
    }

    private void OnFinTurnoPresionado()
    {
        _tablero.SolicitarFinTurno();
    }

    private void OnRerollSolicitado()
    {
        _tablero.SolicitarReroll();
    }

    private void OnRerollAplicado(int indiceJugador)
    {
        if (indiceJugador == _indice)
        {
            _mano.GetNodeOrNull<AudioStreamPlayer>("Sonidos/RerollSFX")?.Play();
        }
    }

    private void ActualizarBotones()
    {
        var controller = Controller.GetInstance();
        bool miTurno = controller.JugadorActual == _indice && !controller.JuegoTerminado && !controller.TurnoEnTransicion;

        _botonFinTurno.Disabled = !(miTurno && _jugador.RealizoAccionEsteTurno);

        var botonReroll = _mano.GetNodeOrNull<Button>("BotonReroll");
        if (botonReroll != null)
        {
            botonReroll.Disabled = !(miTurno && !_jugador.RealizoAccionEsteTurno && _jugador.RerollDisponible);
        }

        var botonHabilidad = GetNodeOrNull<TextureButton>("UITemporal/BotonHabilidad");
        if (botonHabilidad != null)
        {
            botonHabilidad.Disabled = !(miTurno && controller.PuedeUsarHabilidad());
        }
    }

    private async void OnTurnoCambiado(int jugadorActual)
    {
        foreach (Jugador j in Controller.GetInstance().Jugadores())
        {
            string mano = string.Join(",", j.manoCartas.ConvertAll(c => c == null ? "-" : c.Nombre));
            GD.Print($"[Diagnóstico] {j.nombre}: {mano}");
        }

        _modoSeleccionHabilidadActivo = false;

        if (_fichaSeleccionada != null)
        {
            _fichaSeleccionada = null;
            DesalumbrarFichas();
        }

        _mano.ActualizarMano();
        ActualizarFondo(jugadorActual);
        CambiarTurnoInsignias();
        ActualizarBotones();

        if (TransicionScene != null)
        {
            var transicion = TransicionScene.Instantiate<PanelTransicion>();
            AddChild(transicion);

            string nombreProximoJugador = Controller.GetInstance().NombreJugadorActual();
            await transicion.ReproducirTransicionAsync(nombreProximoJugador, 1.5f);
        }
    }

    private void OnAccionAplicada(int indiceJugador)
    {
        _mano.ActualizarMano();
        ActualizarInsigniasSecundarias();

        if (indiceJugador == _indice)
        {
            _fichaSeleccionada = null;
            Controller.GetInstance().CambiarCartaSeleccionada(null);
            DesalumbrarFichas();
        }

        ActualizarBotones();
    }

    private void OnHabilidadPresionada()
    {
        var controller = Controller.GetInstance();
        if (controller.JuegoTerminado || controller.TurnoEnTransicion || controller.JugadorActual != _indice || !controller.PuedeUsarHabilidad()) return;

        TipoHabilidad tipo = _jugador.PersonajeAsignado.Tipo;

        if (tipo == TipoHabilidad.LuzMala)
        {
            MostrarPopupLuzMala();
            return;
        }

        PasarCinematica(tipo);

        if (tipo == TipoHabilidad.Pomberito)
        {
            _tablero.SolicitarHabilidad(tipo, 0, 0, -1);
            return;
        }

        _modoSeleccionHabilidadActivo = true;
    }

    private void OnFichaClickeada(Ficha ficha)
    {
        var controller = Controller.GetInstance();

        if (!_camara.levantada) return;
        if (controller.JuegoTerminado || controller.TurnoEnTransicion || controller.JugadorActual != _indice) return;

        if (ficha.Datos.Bloqueada && _fichaSeleccionada == null) return;

        if (_modoSeleccionHabilidadActivo)
        {
            _modoSeleccionHabilidadActivo = false;
            _tablero.SolicitarHabilidad(_jugador.PersonajeAsignado.Tipo, ficha.Datos.Fila, ficha.Datos.Columna, -1);
            return;
        }

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

    private void MostrarPopupLuzMala()
    {
        var jugadores = Controller.GetInstance().Jugadores();

        var popup = GetNode<PopupMenu>("UITemporal/PopupLuzMala");
        popup.Clear();

        for (int i = 0; i < jugadores.Length; i++)
        {
            if (i == _indice) continue;
            popup.AddItem(jugadores[i].nombre, i);
        }

        if (popup.IsConnected(PopupMenu.SignalName.IdPressed, Callable.From<long>(OnJugadorElegidoLuzMala)))
        {
            popup.IdPressed -= OnJugadorElegidoLuzMala;
        }
        popup.IdPressed += OnJugadorElegidoLuzMala;
        popup.Popup();
    }

    private void OnJugadorElegidoLuzMala(long id)
    {
        PasarCinematica(TipoHabilidad.LuzMala);
        _tablero.SolicitarHabilidad(TipoHabilidad.LuzMala, 0, 0, (int)id);
    }

    private void PasarCinematica(TipoHabilidad tipo)
    {
        VideoStreamTheora[] cinematicas = { CinematicaLobizon, CinematicaLuzMala, CinematicaPomberito, CinematicaMulanima };
        VideoStreamTheora cinematica = cinematicas[(int)tipo];
        var player = CinematicaScene.Instantiate<Cinematica>();
        player.Stream = cinematica;
        GetNode<CanvasLayer>("UITemporal").AddChild(player);
    }
}