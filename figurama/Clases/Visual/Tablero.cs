using System.Collections.Generic;
using Godot;

public partial class Tablero : Node3D
{
    [Export] public PackedScene FichaScene;
    [Export] public PackedScene ManoCartasScene;
    [Export] public PackedScene CinematicaScene;
    [Export] public VideoStreamTheora CinematicaLobizon;
    [Export] public VideoStreamTheora CinematicaLuzMala;
    [Export] public VideoStreamTheora CinematicaMulanima;
    [Export] public VideoStreamTheora CinematicaPomberito;

    private const float SizeCelda = 1.0f;
    private const int cantidadColores = 9;

    private TextureRect rectRojo;
    private TextureRect rectAzul;
    private TextureRect rectAmarillo;
    private TextureRect rectVerde;

    private TableroReglas _reglas = new TableroReglas();

    private Node _sonidos = null;

    [Signal] public delegate void SeleccionadaEventHandler(Ficha ficha);
    [Signal] public delegate void DesclickeadaEventHandler(Ficha ficha);
    [Signal] public delegate void DisponibleEventHandler(Ficha ficha);
    [Signal] public delegate void FiguraCompletadaEventHandler(Ficha ficha);
    [Signal] public delegate void BloqueadaEventHandler(Ficha ficha);
    [Signal] public delegate void DesbloqueadaEventHandler(Ficha ficha);
    [Signal] public delegate void ComodinActivadoEventHandler(Ficha ficha);
    [Signal] public delegate void ComodinDesactivadoEventHandler(Ficha ficha);

    private bool _hayFichaSeleccionada = false;
    private Ficha _fichaSeleccionada;

    private List<ManoCartasView> Manos = new List<ManoCartasView>();

    private static readonly Dictionary<TipoHabilidad, string> NombreBotonHabilidad = new Dictionary<TipoHabilidad, string>
    {
        { TipoHabilidad.Lobizon, "Furia del Lobizón" },
        { TipoHabilidad.LuzMala, "Luz Mala Activa" },
        { TipoHabilidad.Pomberito, "Pomberito Recargado" },
        { TipoHabilidad.Mulanima, "Mulánima Enfurecida" }
    };

    public override void _Ready()
    {
        if (Controller.GetInstance().Jugadores() == null || Controller.GetInstance().Jugadores().Length == 0)
        {
            Controller.GetInstance().InicializarJugadores();
        }

        Controller.GetInstance().FichaBloqueadaEvent += OnFichaBloqueada;
        Controller.GetInstance().FichaDesbloqueadaEvent += OnFichaDesbloqueada;

        var botonHabilidad = GetNodeOrNull<Button>("UITemporal/BotonHabilidad");
        if (botonHabilidad != null)
        {
            botonHabilidad.Pressed += OnHabilidadPresionada;
        }
        _sonidos = GetNode<Node>("Sonidos");
        
        rectRojo = GetNode<TextureRect>("UITemporal/indicadorUltimoColor/ultimoRojo");
        rectAzul = GetNode<TextureRect>("UITemporal/indicadorUltimoColor/ultimoAzul");
        rectAmarillo = GetNode<TextureRect>("UITemporal/indicadorUltimoColor/ultimoAmarillo");
        rectVerde = GetNode<TextureRect>("UITemporal/indicadorUltimoColor/ultimoVerde");

        ApagarTodosLosRects();

        Controller.GetInstance().UltimoColorCambiado += ActualizarColor;
        Controller.GetInstance().FichaComodinDesactivada += OnFichaComodinDesactivada;

        Color[] colores = { Colors.MediumVioletRed, Colors.RoyalBlue, Colors.Yellow, Colors.Chartreuse };
        ColorFicha[] coloresLogicos = { ColorFicha.Rojo, ColorFicha.Azul, ColorFicha.Amarillo, ColorFicha.Verde };
        int[] contadorColores = { cantidadColores, cantidadColores, cantidadColores, cantidadColores };

        var botonFinTurno = GetNodeOrNull<Button>("UITemporal/BotonFinTurno");
        if (botonFinTurno != null)
        {
            botonFinTurno.Pressed += OnFinTurnoPresionado;
        }

        for (int fila = 0; fila < TableroReglas.Filas; fila++)
        {
            for (int columna = 0; columna < TableroReglas.Columnas; columna++)
            {
                Color colorElegido = colores[GD.Randi() % colores.Length];
                colorElegido = VerificarCantidadDeFichas(colorElegido, colores, contadorColores);

                Ficha nodoFicha = FichaScene.Instantiate<Ficha>();
                AddChild(nodoFicha);
                nodoFicha.SetearColor(colorElegido);

                var datos = new FichaData();
                datos.Color = coloresLogicos[System.Array.IndexOf(colores, colorElegido)];
                nodoFicha.Datos = datos;
                _reglas.ColocarFicha(datos, fila, columna);
                ActualizarPosicionVisual(nodoFicha);

                nodoFicha.Clickeada += OnFichaClickeada;
                Controller.GetInstance().FiguraEncontrada += OnFiguraCompletada;
                this.Connect(SignalName.Seleccionada, new Callable(nodoFicha, "_on_ficha_seleccionada"));
                this.Connect(SignalName.Desclickeada, new Callable(nodoFicha, "_on_ficha_desclickeada"));
                this.Connect(SignalName.Disponible, new Callable(nodoFicha, "_on_ficha_disponible"));
                this.Connect(SignalName.FiguraCompletada, new Callable(nodoFicha, "_on_figura_completada"));
                this.Connect(SignalName.Bloqueada, new Callable(nodoFicha, "_on_ficha_bloqueada"));
                this.Connect(SignalName.Desbloqueada, new Callable(nodoFicha, "_on_ficha_desbloqueada"));
                this.Connect(SignalName.ComodinActivado, new Callable(nodoFicha, "_on_ficha_comodin_activado"));
                this.Connect(SignalName.ComodinDesactivado, new Callable(nodoFicha, "_on_ficha_comodin_desactivado"));
            }
        }

        CrearManos();
        Manos[0].Show();

        Controller.GetInstance().TurnoCambiado += OnTurnoCambiado;
        Controller.GetInstance().Victoria += MostrarVictoria;

        GetNode<Button>("UITemporal/PanelVictoria/BotonVolverMenu").Pressed += OnVolverMenuPresionado;

        ActualizarLabelTurno(Controller.GetInstance().JugadorActual);
        
        ActualizarBotonHabilidad();
        ActualizarBotonesAccion();    
    }

    private void OnFinTurnoPresionado()
    {
        Controller.GetInstance().TerminarTurno();
    }

    private void OnCartaSeleccionadaCambiada(CartaMovimiento nuevaCarta)
    {
        DesalumbrarFichas();

        if (nuevaCarta != null)
        {
            AlumbrarFichasDisponibles();
        }
    }

    private void ActualizarColor(ColorFicha nuevoColor)
    {
        ApagarTodosLosRects();

        // Prendemos solo el TextureRect correspondiente
        switch (nuevoColor)
        {
            case ColorFicha.Rojo:
                rectRojo.Visible = true;
                break;
            case ColorFicha.Azul:
                rectAzul.Visible = true;
                break;
            case ColorFicha.Amarillo:
                rectAmarillo.Visible = true;
                break;
            case ColorFicha.Verde:
                rectVerde.Visible = true;
                break;
        }
    }

    private void ApagarTodosLosRects()
    {
        if (rectRojo != null) rectRojo.Visible = false;
        if (rectAzul != null) rectAzul.Visible = false;
        if (rectAmarillo != null) rectAmarillo.Visible = false;
        if (rectVerde != null) rectVerde.Visible = false;
    }

    private Color VerificarCantidadDeFichas(Color color, Color[] colores, int[] contadorColores)
    {
        int colorIndice = System.Array.IndexOf(colores, color);
        if (contadorColores[colorIndice] > 0)
        {
            contadorColores[colorIndice]--;
            return color;
        }
        else
        {
            Color nuevoColor;
            do
            {
                nuevoColor = colores[GD.Randi() % colores.Length];
                colorIndice = System.Array.IndexOf(colores, nuevoColor);
            } while (contadorColores[colorIndice] <= 0);

            contadorColores[colorIndice]--;
            color = nuevoColor;
            return color;
        }
    }

    private void CrearManos()
    {
        for (int i = 0; i < Controller.GetInstance().Jugadores().Length; i++)
        {
            var mano = ManoCartasScene.Instantiate<ManoCartasView>();
            GetNode<Node>("UITemporal/Manos").AddChild(mano);
            mano.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
            mano.MouseFilter = Control.MouseFilterEnum.Ignore;
            mano.Hide();
            mano.crearMano(Controller.GetInstance().Jugadores()[i]);
            Manos.Add(mano);
        }
    }

    public void OnFiguraCompletada(List<(int fila, int columna)> celdas)
    {
        _sonidos.GetNode<AudioStreamPlayer>("FiguraSFX").Play();
        foreach (var celda in celdas)
        {
            Ficha ficha = GetFichaEnPosicion(celda.fila, celda.columna);
            EmitSignal(SignalName.FiguraCompletada, ficha);
        }
    }

    private void OnFichaClickeada(Ficha ficha) 
    {
        GD.Print($"Ficha clickeada. ModoHabilidad activo: {_modoSeleccionHabilidadActivo}");

        if (_modoSeleccionHabilidadActivo)
        {
            var jugador = Controller.GetInstance().Jugadores()[Controller.GetInstance().JugadorActual];
            bool activada = false;

            GD.Print($"Tipo de personaje: {jugador.PersonajeAsignado.Tipo}");

            if (jugador.PersonajeAsignado.Tipo == TipoHabilidad.Lobizon)
            {
                activada = Controller.GetInstance().ActivarHabilidadLobizon(ficha.Datos);
                if (activada)
                {
                    EmitSignal(SignalName.ComodinActivado, ficha);

                    var celdaComodin = new HashSet<(int fila, int columna)>
                    {
                        (ficha.Datos.Fila, ficha.Datos.Columna)
                    };
                    Controller.GetInstance().ChequearFigurasCompletadas(_reglas, celdaComodin);
                    Manos[Controller.GetInstance().JugadorActual].ActualizarMano();
                }
            }

            else if (jugador.PersonajeAsignado.Tipo == TipoHabilidad.Mulanima)
            {
                activada = Controller.GetInstance().ActivarHabilidadMulanima(ficha.Datos);
            }

            _modoSeleccionHabilidadActivo = false;
            if (activada)
            {
                ActualizarBotonHabilidad();
                ActualizarBotonesAccion(); 
            }
            return;
        }

        if (ficha.Datos.Bloqueada && _fichaSeleccionada == null)
        {
            return;
        }

        // Si no hay ficha seleccionada todavía, la marcamos como primera ficha
        if (_fichaSeleccionada == null)
        {
            _fichaSeleccionada = ficha;
            _hayFichaSeleccionada = true;
            EmitSignal(SignalName.Seleccionada, _fichaSeleccionada);
            AlumbrarFichasDisponibles();
            return;
        }

        // Si vuelve a clickear la misma ficha, la deseleccionamos
        if (_fichaSeleccionada == ficha)
        {
            EmitSignal(SignalName.Desclickeada, _fichaSeleccionada);
            _fichaSeleccionada = null;
            _hayFichaSeleccionada = false;

            Controller.GetInstance().CambiarCartaSeleccionada(null);

            DesalumbrarFichas();
            AlumbrarFichasDisponibles();
            return;
        }

        // Si hay una segunda ficha seleccionada, intentamos ejecutar el movimiento
        CartaMovimiento movimientoActual = Controller.GetInstance().CartaSeleccionada;

        if (movimientoActual != null && !ficha.Datos.Bloqueada && movimientoActual.EsValido(_reglas, _fichaSeleccionada.Datos.Fila, _fichaSeleccionada.Datos.Columna, ficha.Datos.Fila, ficha.Datos.Columna))
        {
            movimientoActual.Ejecutar(_reglas, _fichaSeleccionada.Datos.Fila, _fichaSeleccionada.Datos.Columna, ficha.Datos.Fila, ficha.Datos.Columna);
            _sonidos.GetNode<AudioStreamPlayer>("CambioSFX").Play();

            ActualizarPosicionVisual(_fichaSeleccionada);
            ActualizarPosicionVisual(ficha);

            var celdasMovidas = new HashSet<(int fila, int columna)>
            {
                (_fichaSeleccionada.Datos.Fila, _fichaSeleccionada.Datos.Columna),
                (ficha.Datos.Fila, ficha.Datos.Columna)
            };

            Controller.GetInstance().ChequearFigurasCompletadas(_reglas, celdasMovidas);

            Manos[Controller.GetInstance().JugadorActual].ActualizarMano();

            Controller.GetInstance().RegistrarMovimientoRealizado(movimientoActual);

            Controller.GetInstance().CambiarCartaSeleccionada(null);

            ActualizarBotonesAccion();
            
        }

        DesalumbrarFichas();
        _fichaSeleccionada = null;
        _hayFichaSeleccionada = false;
    }

    private void DesalumbrarFichas()
    {
        for (int fila = 0; fila < TableroReglas.Filas; fila++)
        {
            for (int columna = 0; columna < TableroReglas.Columnas; columna++)
            {
                Ficha ficha = GetFichaEnPosicion(fila, columna);
                if (ficha != null && ficha.Datos.Bloqueada == false)
                {
                    EmitSignal(SignalName.Desclickeada, ficha);
                }
            }
        }
    }

    private void DesclickearFichas()
    {
        DesalumbrarFichas();
        _fichaSeleccionada = null;
        _hayFichaSeleccionada = false;
    }

    private void AlumbrarFichasDisponibles()
    {
        CartaMovimiento cartaActiva = Controller.GetInstance().CartaSeleccionada;
        if (cartaActiva == null) return;

        //Ya se eligió una primera ficha -> ilumina los destinos válidos desde esa ficha
        if (_fichaSeleccionada != null)
        {
            for (int fila = 0; fila < TableroReglas.Filas; fila++)
            {
                for (int columna = 0; columna < TableroReglas.Columnas; columna++)
                {
                    Ficha destino = GetFichaEnPosicion(fila, columna);
                    if (destino != null && destino != _fichaSeleccionada)
                    {
                        AlumbrarSiEsValida(destino);
                    }
                }
            }
        }
        //No hay ficha elegida aún -> ilumina todas las fichas del tablero que puedan hacer al menos un movimiento
        else
        {
            for (int f1 = 0; f1 < TableroReglas.Filas; f1++)
            {
                for (int c1 = 0; c1 < TableroReglas.Columnas; c1++)
                {
                    Ficha origen = GetFichaEnPosicion(f1, c1);
                    if (origen == null) continue;

                    bool tieneOpcionValida = false;

                    for (int f2 = 0; f2 < TableroReglas.Filas; f2++)
                    {
                        for (int c2 = 0; c2 < TableroReglas.Columnas; c2++)
                        {
                            if (f1 == f2 && c1 == c2) continue;

                            if (cartaActiva.EsValido(_reglas, f1, c1, f2, c2))
                            {
                                tieneOpcionValida = true;
                                break;
                            }
                        }
                        if (tieneOpcionValida) break;
                    }

                    if (tieneOpcionValida)
                    {
                        EmitSignal(SignalName.Disponible, origen);
                    }
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
            if (cartaActiva.EsValido(_reglas, _fichaSeleccionada.Datos.Fila, _fichaSeleccionada.Datos.Columna, ficha.Datos.Fila, ficha.Datos.Columna))
            {
                EmitSignal(SignalName.Disponible, ficha);
            }
        }
    }
    private Ficha GetFichaEnPosicion(int fila, int columna)
    {
        foreach (Node child in GetChildren())
        {
            if (child is Ficha ficha && ficha.Datos.Fila == fila && ficha.Datos.Columna == columna)
            {
                return ficha;
            }
        }
        return null;
    }


    private void ActualizarPosicionVisual(Ficha nodoFicha)
    {
        nodoFicha.Position = new Vector3(nodoFicha.Datos.Columna * SizeCelda, 0, nodoFicha.Datos.Fila * SizeCelda);
    }

    private void OnTurnoCambiado(int jugadorActual)
    {
        int jugadorAnterior = (jugadorActual - 1 + Manos.Count) % Manos.Count;
        Manos[jugadorAnterior].Hide();
        Manos[jugadorActual].Show();
        Manos.ForEach(man => man.ActualizarMano());
        ActualizarLabelTurno(jugadorActual);
        ActualizarBotonHabilidad();
        ActualizarBotonesAccion();
    }

    private void ActualizarLabelTurno(int jugadorActual)
    {
        GetNode<Label>("UITemporal/LabelTurno").Text = $"Turno: {Controller.GetInstance().NombreJugadorActual()}";
    }

    private void MostrarVictoria(string nombreGanador)
    {
        GetNode<Label>("UITemporal/PanelVictoria/LabelGanador").Text = $"¡{nombreGanador} ganó la partida!";
        GetNode<Control>("UITemporal/PanelVictoria").Show();
    }

    private void OnVolverMenuPresionado()
    {
        GetTree().ChangeSceneToFile("res://Objetos/menuPrincipal.tscn");
    }

    private bool _modoSeleccionHabilidadActivo = false;

    private void OnHabilidadPresionada()
    {
        var jugador = Controller.GetInstance().Jugadores()[Controller.GetInstance().JugadorActual];
        TipoHabilidad tipo = jugador.PersonajeAsignado.Tipo;

        GD.Print($"Habilidad presionada. Tipo: {tipo}, PuedeUsar: {Controller.GetInstance().PuedeUsarHabilidad()}");

        if (tipo == TipoHabilidad.LuzMala)
        {
            MostrarPopupLuzMala();
            return;
        }

        PasarCinematica(tipo);

        if (tipo == TipoHabilidad.Pomberito)
        {
            bool activada = Controller.GetInstance().ActivarHabilidadPomberito();
            if (activada)
            {
                Manos[Controller.GetInstance().JugadorActual].ActualizarMano();
                ActualizarBotonesAccion();
            }
            ActualizarBotonHabilidad();
            return;
        }

        // Lobizón y Mulánima necesitan clickear una ficha después
        _modoSeleccionHabilidadActivo = true;
        GD.Print($"Modo selección activado: {_modoSeleccionHabilidadActivo}");
    }

    private void PasarCinematica(TipoHabilidad tipo)
    {
        VideoStreamTheora[] cinematicas = { CinematicaLobizon, CinematicaLuzMala, CinematicaPomberito, CinematicaMulanima };
        VideoStreamTheora cinematica = cinematicas[(int)tipo];
        var player = CinematicaScene.Instantiate<Cinematica>();
        player.Stream = cinematica;
        GetNode<CanvasLayer>("UITemporal").AddChild(player);
    }

    private void ActualizarBotonHabilidad()
    {
        var botonHabilidad = GetNodeOrNull<Button>("UITemporal/BotonHabilidad");
        if (botonHabilidad == null) return;

        var jugador = Controller.GetInstance().Jugadores()[Controller.GetInstance().JugadorActual];
        botonHabilidad.Text = NombreBotonHabilidad[jugador.PersonajeAsignado.Tipo];
        botonHabilidad.Disabled = !Controller.GetInstance().PuedeUsarHabilidad();
    }

    private void MostrarPopupLuzMala()
    {
        var jugadores = Controller.GetInstance().Jugadores();
        int actual = Controller.GetInstance().JugadorActual;

        var popup = GetNode<PopupMenu>("UITemporal/PopupLuzMala");
        popup.Clear();

        for (int i = 0; i < jugadores.Length; i++)
        {
            if (i == actual) continue;
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
        var jugadores = Controller.GetInstance().Jugadores();
        bool activada = Controller.GetInstance().ActivarHabilidadLuzMala(jugadores[id]);
        if (activada)
        {
            ActualizarBotonHabilidad();
            ActualizarBotonesAccion();
        }
    }

    private void OnFichaComodinDesactivada(FichaData datosFicha)
    {
        Ficha ficha = GetFichaEnPosicion(datosFicha.Fila, datosFicha.Columna);
        if (ficha != null)
        {
            EmitSignal(SignalName.ComodinDesactivado, ficha);
        }
    }

    private void OnFichaBloqueada(FichaData datos)
    {
        Ficha ficha = GetFichaEnPosicion(datos.Fila, datos.Columna);
        if (ficha != null) EmitSignal(SignalName.Bloqueada, ficha);
    }

    private void OnFichaDesbloqueada(FichaData datos)
    {
        Ficha ficha = GetFichaEnPosicion(datos.Fila, datos.Columna);
        if (ficha != null) EmitSignal(SignalName.Desbloqueada, ficha);
    }

    private void ActualizarBotonesAccion()
    {
        var jugador = Controller.GetInstance().Jugadores()[Controller.GetInstance().JugadorActual];

        var botonFinTurno = GetNodeOrNull<Button>("UITemporal/BotonFinTurno");
        if (botonFinTurno != null)
        {
            botonFinTurno.Disabled = !jugador.RealizoAccionEsteTurno;
        }

        var botonReroll = Manos[Controller.GetInstance().JugadorActual].GetNodeOrNull<Button>("BotonReroll");
        if (botonReroll != null)
        {
            botonReroll.Disabled = jugador.RealizoAccionEsteTurno || !jugador.RerollDisponible;
        }
    }
}