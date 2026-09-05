using System.Collections.Generic;
using Godot;

public partial class Tablero : Node3D
{
    [Export] public PackedScene FichaScene;
    [Export] public PackedScene ManoCartasScene;

    private const float SizeCelda = 1.0f;
    private const int cantidadColores = 9;

    private TableroReglas _reglas = new TableroReglas();

    [Signal] public delegate void SeleccionadaEventHandler(Ficha ficha);
    [Signal] public delegate void DesclickeadaEventHandler(Ficha ficha);

    private bool _hayFichaSeleccionada = false;
    private Ficha _fichaSeleccionada;

    private List<Control> Manos = new List<Control>();

    public override void _Ready()
    {
        Color[] colores = { Colors.Red, Colors.Blue, Colors.Yellow, Colors.Green };
        ColorFicha[] coloresLogicos = { ColorFicha.Rojo, ColorFicha.Azul, ColorFicha.Amarillo, ColorFicha.Verde };
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
                datos.Color = coloresLogicos[System.Array.IndexOf(colores, colorElegido)];
                nodoFicha.Datos = datos;
                _reglas.ColocarFicha(datos, fila, columna);
                ActualizarPosicionVisual(nodoFicha);

                nodoFicha.Clickeada += OnFichaClickeada;
                this.Connect(SignalName.Seleccionada, new Callable(nodoFicha, "_on_ficha_seleccionada"));
                this.Connect(SignalName.Desclickeada, new Callable(nodoFicha, "_on_ficha_desclickeada"));
            }
        }

        CrearManos();
        Manos[0].Show();

        Controller.GetInstance().TurnoCambiado += OnTurnoCambiado;
        Controller.GetInstance().Victoria += MostrarVictoria;

        GetNode<Button>("UITemporal/PanelVictoria/BotonVolverMenu").Pressed += OnVolverMenuPresionado;

        ActualizarLabelTurno(Controller.GetInstance().JugadorActual);
        ActualizarLabelFiguras();
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

    private void OnFichaClickeada(Ficha ficha)
    {
        if (Controller.GetInstance().JuegoTerminado) return;
        if (Controller.GetInstance().MovimientoActual() == null) return;

        if (!_hayFichaSeleccionada)
        {
            _fichaSeleccionada = ficha;
            _hayFichaSeleccionada = true;
            EmitSignal(SignalName.Seleccionada, ficha);
            return;
        }

        if (Controller.GetInstance().MovimientoActual().EsValido(_reglas, _fichaSeleccionada.Datos.Fila, _fichaSeleccionada.Datos.Columna, ficha.Datos.Fila, ficha.Datos.Columna))
        {
            Controller.GetInstance().MovimientoActual().Ejecutar(_reglas, _fichaSeleccionada.Datos.Fila, _fichaSeleccionada.Datos.Columna, ficha.Datos.Fila, ficha.Datos.Columna);

            ActualizarPosicionVisual(_fichaSeleccionada);
            ActualizarPosicionVisual(ficha);

            var celdasMovidas = new HashSet<(int fila, int columna)>
            {
                (_fichaSeleccionada.Datos.Fila, _fichaSeleccionada.Datos.Columna),
                (ficha.Datos.Fila, ficha.Datos.Columna)
            };

            var figurasCompletadas = Controller.GetInstance().ChequearFigurasCompletadas(_reglas, celdasMovidas);
            foreach (CartaFigura figura in figurasCompletadas)
            {
                GD.Print($"{Controller.GetInstance().NombreJugadorActual()} completó la figura: {figura.Nombre}");
            }
            ActualizarLabelFiguras();

            Controller.GetInstance().TerminarTurno();
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
        int jugadorAnterior = (jugadorActual - 1 + Manos.Count) % Manos.Count;
        Manos[jugadorAnterior].Hide();
        Manos[jugadorActual].Show();
        ActualizarLabelTurno(jugadorActual);
        ActualizarLabelFiguras();
    }

    private void ActualizarLabelTurno(int jugadorActual)
    {
        GetNode<Label>("UITemporal/LabelTurno").Text = $"Turno: Jugador {Controller.GetInstance().NombreJugadorActual()}";
    }

    private void ActualizarLabelFiguras()
    {
        var figuras = Controller.GetInstance().FigurasJugadorActual();
        string texto = "Figuras a armar:\n";

        foreach (var asignada in figuras)
        {
            texto += asignada.Completada ? $"[s]{asignada.Figura.Nombre}[/s]\n" : $"{asignada.Figura.Nombre}\n";
        }

        GetNode<RichTextLabel>("UITemporal/LabelFiguras").Text = texto;
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