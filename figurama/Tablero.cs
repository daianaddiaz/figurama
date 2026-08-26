using Godot;

public partial class Tablero : Node3D
{
    [Export] public PackedScene FichaScene;
    private const int Filas = 6;
    private const int Columnas = 6;
    private const float SizeCelda = 1.0f;

    private Ficha[,] _grilla = new Ficha[Filas, Columnas];

    [Signal] public delegate void SeleccionadaEventHandler(Ficha ficha);
    [Signal] public delegate void DesclickeadaEventHandler(Ficha ficha);

    private CartaMovimiento _movimiento;
    private bool _hayFichaSeleccionada = false;
    private Ficha _fichaSeleccionada;
    private int _filaSeleccionada;
    private int _columnaSeleccionada;
    private int cantidadColores = 9;

    public override void _Ready()
    {
        Color[] colores = { Colors.Red, Colors.Blue, Colors.Yellow, Colors.Green };
        int[] contadorColores = { cantidadColores, cantidadColores, cantidadColores, cantidadColores };

        for (int fila = 0; fila < Filas; fila++)
        {
            for (int columna = 0; columna < Columnas; columna++)
            {
                Color colorElegido = colores[GD.Randi() % colores.Length];
                colorElegido = VerificarCantidadDeFichas(colorElegido, colores, contadorColores);
                Ficha ficha = FichaScene.Instantiate<Ficha>();
                AddChild(ficha);
                ficha.SetearColor(colorElegido);
                ficha.Clickeada += OnFichaClickeada;
                this.Connect(SignalName.Seleccionada, new Callable(ficha, "_on_ficha_seleccionada"));
                this.Connect(SignalName.Desclickeada, new Callable(ficha, "_on_ficha_desclickeada"));
                ColocarFicha(ficha, fila, columna);
            }
        }

        GetNode<Button>("UITemporal/BotonesMovimientos/BotonLateralContiguo").Pressed += SeleccionarLateralContiguo;
        GetNode<Button>("UITemporal/BotonesMovimientos/BotonLateralConEspacio").Pressed += SeleccionarLateralConEspacio;
        GetNode<Button>("UITemporal/BotonesMovimientos/BotonEnL").Pressed += SeleccionarEnL;
    }

    private Color VerificarCantidadDeFichas(Color color, Color[] colores, int[] contadorColores)
    {
        int colorIndice = System.Array.IndexOf(colores, color);
        if(contadorColores[colorIndice] > 0)
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

    private void SeleccionarLateralContiguo()
    {
        _movimiento = new MovimientoLateralContiguo();
    }

    private void SeleccionarLateralConEspacio()
    {
        _movimiento = new MovimientoLateralConEspacio();
    }

    private void SeleccionarEnL()
    {
        _movimiento = new MovimientoEnL();
    }

    private void OnFichaClickeada(Ficha ficha)
    {
        if (_movimiento == null)
        {
            return;
        }

        if (!_hayFichaSeleccionada)
        {
            _fichaSeleccionada = ficha;
            _filaSeleccionada = ficha.Fila;
            _columnaSeleccionada = ficha.Columna;
            _hayFichaSeleccionada = true;
            EmitSignal(SignalName.Seleccionada, ficha);
            return;
        }

        if (_movimiento.EsValido(this, _filaSeleccionada, _columnaSeleccionada, ficha.Fila, ficha.Columna))
        {
            _movimiento.Ejecutar(this, _filaSeleccionada, _columnaSeleccionada, ficha.Fila, ficha.Columna);
        }

        EmitSignal(SignalName.Desclickeada, ficha);
        EmitSignal(SignalName.Desclickeada, _fichaSeleccionada);
        _hayFichaSeleccionada = false;
    }

    public void ColocarFicha(Ficha ficha, int fila, int columna)
    {
        _grilla[fila, columna] = ficha;
        ficha.Fila = fila;
        ficha.Columna = columna;
        ficha.Position = new Vector3(columna * SizeCelda, 0, fila * SizeCelda);
    }

    public Ficha ObtenerFicha(int fila, int columna)
    {
        return _grilla[fila, columna];
    }

    public void IntercambiarFichas(int filaA, int columnaA, int filaB, int columnaB)
    {
        Ficha fichaA = ObtenerFicha(filaA, columnaA);
        Ficha fichaB = ObtenerFicha(filaB, columnaB);

        ColocarFicha(fichaA, filaB, columnaB);
        ColocarFicha(fichaB, filaA, columnaA);
    }
}