using System.Collections.Generic;
using Godot;

public partial class Tablero : Node3D
{
    [Export] public PackedScene FichaScene;
    [Export] public PackedScene ManoCartasScene;

    private const float SizeCelda = 1.0f;
    private const int cantidadColores = 9;

    private TextureRect rectRojo;
    private TextureRect rectAzul;
    private TextureRect rectAmarillo;
    private TextureRect rectVerde;

    private TableroReglas _reglas = new TableroReglas();

    [Signal] public delegate void SeleccionadaEventHandler(Ficha ficha);
    [Signal] public delegate void DesclickeadaEventHandler(Ficha ficha);
    [Signal] public delegate void DisponibleEventHandler(Ficha ficha);
    [Signal] public delegate void FiguraCompletadaEventHandler(Ficha ficha);

    private bool _hayFichaSeleccionada = false;
    private Ficha _fichaSeleccionada;

    private List<ManoCartasView> Manos = new List<ManoCartasView>();

    public override void _Ready()
    {
        if (Controller.GetInstance().Jugadores() == null || Controller.GetInstance().Jugadores().Length == 0)
        {
            Controller.GetInstance().InicializarJugadores();
        }

        rectRojo = GetNode<TextureRect>("UITemporal/indicadorUltimoColor/ultimoRojo");
        rectAzul = GetNode<TextureRect>("UITemporal/indicadorUltimoColor/ultimoAzul");
        rectAmarillo = GetNode<TextureRect>("UITemporal/indicadorUltimoColor/ultimoAmarillo");
        rectVerde = GetNode<TextureRect>("UITemporal/indicadorUltimoColor/ultimoVerde");

        ApagarTodosLosRects();

        Controller.GetInstance().UltimoColorCambiado += ActualizarColor;

        Color[] colores = { Colors.Red, Colors.Blue, Colors.Yellow, Colors.Green };
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
                _reglas.FiguraEncontrada += OnFiguraCompletada;
                this.Connect(SignalName.Seleccionada, new Callable(nodoFicha, "_on_ficha_seleccionada"));
                this.Connect(SignalName.Desclickeada, new Callable(nodoFicha, "_on_ficha_desclickeada"));
                this.Connect(SignalName.Disponible, new Callable(nodoFicha, "_on_ficha_disponible"));
                this.Connect(SignalName.FiguraCompletada, new Callable(nodoFicha, "_on_figura_completada"));
            }
        }

        CrearManos();
        Manos[0].Show();

        Controller.GetInstance().TurnoCambiado += OnTurnoCambiado;
        Controller.GetInstance().Victoria += MostrarVictoria;

        GetNode<Button>("UITemporal/PanelVictoria/BotonVolverMenu").Pressed += OnVolverMenuPresionado;

        ActualizarLabelTurno(Controller.GetInstance().JugadorActual);
        //Controller.GetInstance().CartaSeleccionada += OnCartaSeleccionadaCambiada;    
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
        foreach (var celda in celdas)
        {
            Ficha ficha = GetFichaEnPosicion(celda.fila, celda.columna);
            EmitSignal(SignalName.FiguraCompletada, ficha);
        }
    }

    private void OnFichaClickeada(Ficha ficha) 
    {
        //Si no hay ficha seleccionada todavía, la marcamos como primera ficha
        if (_fichaSeleccionada == null)
        {
            _fichaSeleccionada = ficha;
            _hayFichaSeleccionada = true;
            EmitSignal(SignalName.Seleccionada, _fichaSeleccionada);

            // Re-evaluamos para mostrar solo los destinos válidos para esta ficha concreta
            DesalumbrarFichas();
            AlumbrarFichasDisponibles();
            return;
        }

        // Si vuelve a clickear la misma ficha, la deseleccionamos
        if (_fichaSeleccionada == ficha)
        {
            EmitSignal(SignalName.Desclickeada, _fichaSeleccionada);
            _fichaSeleccionada = null;
            _hayFichaSeleccionada = false;

            // Volvemos a iluminar las fichas que se pueden mover en general
            DesalumbrarFichas();
            AlumbrarFichasDisponibles();
            return;
        }

        //Si hay una segunda ficha seleccionada, intentamos ejecutar el movimiento
        CartaMovimiento movimientoActual = Controller.GetInstance().CartaSeleccionada;

        if (movimientoActual != null && movimientoActual.EsValido(_reglas, _fichaSeleccionada.Datos.Fila, _fichaSeleccionada.Datos.Columna, ficha.Datos.Fila, ficha.Datos.Columna))
        {
            movimientoActual.Ejecutar(_reglas, _fichaSeleccionada.Datos.Fila, _fichaSeleccionada.Datos.Columna, ficha.Datos.Fila, ficha.Datos.Columna);

            ActualizarPosicionVisual(_fichaSeleccionada);
            ActualizarPosicionVisual(ficha);

            var celdasMovidas = new HashSet<(int fila, int columna)>
            {
                (_fichaSeleccionada.Datos.Fila, _fichaSeleccionada.Datos.Columna),
                (ficha.Datos.Fila, ficha.Datos.Columna)
            };

            Controller.GetInstance().ChequearFigurasCompletadas(_reglas, celdasMovidas);

            var manoActualView = Manos[Controller.GetInstance().JugadorActual];
            manoActualView.AnimarCartaUsada(movimientoActual);

            Controller.GetInstance().RegistrarMovimientoRealizado(movimientoActual);
            Controller.GetInstance().CambiarCartaSeleccionada(null);
            manoActualView.ActualizarMano();
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
                if (ficha != null)
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
}