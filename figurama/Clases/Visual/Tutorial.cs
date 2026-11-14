using Godot;
using System.Collections.Generic;

public partial class Tutorial : Control
{
    private const string RutaEscena = "res://Objetos/tutorial.tscn";

    [Export] public Godot.Collections.Array<Texture2D> PaginasTutorial = new Godot.Collections.Array<Texture2D>();
    [Export] public string RutaMenuPrincipal = "res://Objetos/menuPrincipal.tscn";
    [Export] public float DuracionTransicion = 0.4f;

    // true cuando el tutorial se abre sobre una partida: no cambia de escena, solo se cierra
    public bool Superpuesto = false;

    private TextureRect _visualizadorActual;
    private TextureRect _visualizadorSiguiente;
    private Button _botonVolver;
    private Button _botonSiguiente;

    private int _paginaActual = 0;
    private bool _animando = false;

    // Abre el tutorial encima de la escena actual, sin sacar al jugador de la partida
    public static void AbrirSuperpuesto(SceneTree tree)
    {
        Node escenaActual = tree.CurrentScene;
        if (escenaActual == null) return;
        if (escenaActual.GetNodeOrNull("TutorialSuperpuesto") != null) return; // ya está abierto

        PackedScene escena = GD.Load<PackedScene>(RutaEscena);
        if (escena == null)
        {
            GD.PrintErr($"[Tutorial] No se pudo cargar la escena: {RutaEscena}");
            return;
        }

        // Capa alta (tapa todo el HUD) y activa aunque el juego esté pausado
        var capa = new CanvasLayer
        {
            Name = "TutorialSuperpuesto",
            Layer = 128,
            ProcessMode = ProcessModeEnum.Always
        };

        var tutorial = escena.Instantiate<Tutorial>();
        tutorial.Superpuesto = true;
        capa.AddChild(tutorial);

        // Último hijo de la escena: recibe el teclado antes que el resto del juego
        escenaActual.AddChild(capa);
        tutorial.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
    }

    public override void _Ready()
    {
        _visualizadorActual = GetNode<TextureRect>("VisualizadorActual");
        _visualizadorSiguiente = GetNode<TextureRect>("VisualizadorSiguiente");
        _botonVolver = GetNode<Button>("PanelNavegacion/ContenedorBotones/BotonVolver");
        _botonSiguiente = GetNode<Button>("PanelNavegacion/ContenedorBotones/BotonSiguiente");

        _botonVolver.Pressed += OnVolverPresionado;
        _botonSiguiente.Pressed += OnSiguientePresionado;

        if (PaginasTutorial.Count > 0)
        {
            _visualizadorActual.Texture = PaginasTutorial[0];
        }

        ActualizarTextoBotones();
    }

    public override void _Input(InputEvent @event)
    {
        if (!Superpuesto) return;

        // Con el tutorial abierto, el teclado no llega al juego. ESC lo cierra.
        if (@event is InputEventKey)
        {
            if (@event.IsActionPressed("ui_cancel"))
            {
                Cerrar();
            }
            GetViewport().SetInputAsHandled();
        }
    }

    private void Cerrar()
    {
        Node padre = GetParent();
        if (padre != null) padre.QueueFree();
        else QueueFree();
    }

    private void CambiarPaginaConPaneo(int nuevaPagina, bool haciaIzquierda = false)
    {
        if (_animando || nuevaPagina < 0 || nuevaPagina >= PaginasTutorial.Count) return;

        _animando = true;
        _paginaActual = nuevaPagina;

        // Cargar la nueva textura en el contenedor secundario
        _visualizadorSiguiente.Texture = PaginasTutorial[_paginaActual];

        Vector2 tamanoPantalla = GetViewportRect().Size;
        float startX = haciaIzquierda ? -tamanoPantalla.X : tamanoPantalla.X;
        float endXActual = haciaIzquierda ? tamanoPantalla.X : -tamanoPantalla.X;

        // Posicionar el siguiente visualizador fuera de la pantalla
        _visualizadorSiguiente.Position = new Vector2(startX, 0);
        _visualizadorSiguiente.Visible = true;

        Tween tween = CreateTween().SetParallel(true);

        // Deslizar la imagen actual hacia afuera
        tween.TweenProperty(_visualizadorActual, "position:x", endXActual, DuracionTransicion)
             .SetTrans(Tween.TransitionType.Cubic)
             .SetEase(Tween.EaseType.Out);

        // Deslizar la imagen nueva hacia adentro
        tween.TweenProperty(_visualizadorSiguiente, "position:x", 0.0f, DuracionTransicion)
             .SetTrans(Tween.TransitionType.Cubic)
             .SetEase(Tween.EaseType.Out);

        tween.Chain().TweenCallback(Callable.From(() =>
        {
            // Intercambiar referencias para el próximo cambio
            _visualizadorActual.Texture = _visualizadorSiguiente.Texture;
            _visualizadorActual.Position = Vector2.Zero;
            _visualizadorSiguiente.Visible = false;

            ActualizarTextoBotones();
            _animando = false;
        }));
    }

    private void ActualizarTextoBotones()
    {
        if (_paginaActual == PaginasTutorial.Count - 1)
        {
            _botonSiguiente.Text = Superpuesto ? "Cerrar" : "¡Jugar!";
        }
        else
        {
            _botonSiguiente.Text = "Siguiente";
        }
    }

    private void OnSiguientePresionado()
    {
        if (_animando) return;

        if (_paginaActual < PaginasTutorial.Count - 1)
        {
            CambiarPaginaConPaneo(_paginaActual + 1, haciaIzquierda: false);
        }
        else if (Superpuesto)
        {
            // Dentro de una partida, el último botón solo cierra el tutorial
            Cerrar();
        }
        else
        {
            // Al tocar "¡Jugar!" en la última página, indicamos que abra el contador de jugadores
            MenuPrincipal.PanelInicial = "ContadorJugadores";
            NavegadorEscenas.CambiarEscenaConPaneo(GetTree(), RutaMenuPrincipal, this);
        }
    }

    private void OnVolverPresionado()
    {
        if (_animando) return;

        // Si está en una página avanzada, retrocede a la diapositiva anterior con paneo
        if (_paginaActual > 0)
        {
            CambiarPaginaConPaneo(_paginaActual - 1, haciaIzquierda: true);
        }
        else if (Superpuesto)
        {
            // En la primera página, dentro de una partida, "Volver" cierra el tutorial
            Cerrar();
        }
        else
        {
            // Si está en la primera página, vuelve al menú principal
            NavegadorEscenas.CambiarEscenaConPaneo(GetTree(), RutaMenuPrincipal, this, haciaIzquierda: true);
        }
    }
}