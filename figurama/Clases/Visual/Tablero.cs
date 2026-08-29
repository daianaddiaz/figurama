using Godot;

public partial class Tablero : Node3D
{
    [Export] public PackedScene FichaScene;
    private const float SizeCelda = 1.0f;
    private const int cantidadColores = 9;

    private TableroReglas _reglas = new TableroReglas();

    [Signal] public delegate void SeleccionadaEventHandler(Ficha ficha);
    [Signal] public delegate void DesclickeadaEventHandler(Ficha ficha);

    private CartaMovimiento _movimiento;
    private bool _hayFichaSeleccionada = false;
    private Ficha _fichaSeleccionada;

    public override void _Ready()
    {
        Color[] colores = { Colors.Red, Colors.Blue, Colors.Yellow, Colors.Green };
        int[] contadorColores = { cantidadColores, cantidadColores, cantidadColores, cantidadColores };

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
                nodoFicha.Datos = datos;
                _reglas.ColocarFicha(datos, fila, columna);
                ActualizarPosicionVisual(nodoFicha);

                nodoFicha.Clickeada += OnFichaClickeada;
                this.Connect(SignalName.Seleccionada, new Callable(nodoFicha, "_on_ficha_seleccionada"));
                this.Connect(SignalName.Desclickeada, new Callable(nodoFicha, "_on_ficha_desclickeada"));
            }
        }

        GetNode<Button>("UITemporal/BotonesMovimientos/BotonLateralContiguo").Pressed += SeleccionarLateralContiguo;
        GetNode<Button>("UITemporal/BotonesMovimientos/BotonLateralConEspacio").Pressed += SeleccionarLateralConEspacio;
        GetNode<Button>("UITemporal/BotonesMovimientos/BotonEnL").Pressed += SeleccionarEnL;

        Controller.Instance.TurnoCambiado += OnTurnoCambiado;
        ActualizarLabelTurno(Controller.Instance.JugadorActual);
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

    private void SeleccionarLateralContiguo() => _movimiento = new MovimientoLateralContiguo();
    private void SeleccionarLateralConEspacio() => _movimiento = new MovimientoLateralConEspacio();
    private void SeleccionarEnL() => _movimiento = new MovimientoEnL();

    private void OnFichaClickeada(Ficha ficha)
    {
        if (_movimiento == null) return;

        if (!_hayFichaSeleccionada)
        {
            _fichaSeleccionada = ficha;
            _hayFichaSeleccionada = true;
            EmitSignal(SignalName.Seleccionada, ficha);
            return;
        }

        if (_movimiento.EsValido(_reglas, _fichaSeleccionada.Datos.Fila, _fichaSeleccionada.Datos.Columna, ficha.Datos.Fila, ficha.Datos.Columna))
        {
            _movimiento.Ejecutar(_reglas, _fichaSeleccionada.Datos.Fila, _fichaSeleccionada.Datos.Columna, ficha.Datos.Fila, ficha.Datos.Columna);

            ActualizarPosicionVisual(_fichaSeleccionada);
            ActualizarPosicionVisual(ficha);

            Controller.Instance.TerminarTurno();
        }

        EmitSignal(SignalName.Desclickeada, ficha);
        EmitSignal(SignalName.Desclickeada, _fichaSeleccionada);
        _hayFichaSeleccionada = false;
    }

    private void ActualizarPosicionVisual(Ficha nodoFicha)
    {
        nodoFicha.Position = new Vector3(nodoFicha.Datos.Columna * SizeCelda, 0, nodoFicha.Datos.Fila * SizeCelda);
    }

    private void OnTurnoCambiado(int jugadorActual)
    {
        ActualizarLabelTurno(jugadorActual);
    }

    private void ActualizarLabelTurno(int jugadorActual)
    {
        GetNode<Label>("UITemporal/LabelTurno").Text = $"Turno: Jugador {Controller.Instance.NombreJugadorActual()}";
    }
}