using Godot;
using System.Collections.Generic;

public partial class Tutorial : Control
{
    [Export] public Godot.Collections.Array<Texture2D> PaginasTutorial = new Godot.Collections.Array<Texture2D>();
    [Export] public string RutaMenuPrincipal = "res://Objetos/menuPrincipal.tscn";
    [Export] public float DuracionTransicion = 0.4f;

    private TextureRect _visualizadorActual;
    private TextureRect _visualizadorSiguiente;
    private Button _botonVolver;
    private Button _botonSiguiente;

    private int _paginaActual = 0;
    private bool _animando = false;

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
            _botonSiguiente.Text = "¡Jugar!";
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
        else
        {
            // Si está en la primera página, vuelve al menú principal
            NavegadorEscenas.CambiarEscenaConPaneo(GetTree(), RutaMenuPrincipal, this, haciaIzquierda: true);
        }
    }
}