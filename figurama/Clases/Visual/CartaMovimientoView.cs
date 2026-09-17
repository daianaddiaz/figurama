using Godot;
using System;


public partial class CartaMovimientoView : Button
{
    [Export] public Texture2D TexturaDorso;

    private TextureRect _frente;
    private TextureRect _dorso;
    private CartaMovimiento _cartaRepresentada; 

    private bool _estaVolteada = false;

    public CartaMovimiento CartaRepresentada => _cartaRepresentada;

    public override void _Ready()
    {
        _frente = GetNode<TextureRect>("Frente");
        _dorso = GetNode<TextureRect>("Dorso");

        if (TexturaDorso != null && _dorso != null)
        {
            _dorso.Texture = TexturaDorso;
        }

		CustomMinimumSize = new Vector2(80, 120);
		Size = CustomMinimumSize;
        // Asegura el centro para la rotación/escalado Tween
        PivotOffset = Size / 2;

		var emptyStyle = new StyleBoxEmpty();
		AddThemeStyleboxOverride("normal", emptyStyle);
		AddThemeStyleboxOverride("hover", emptyStyle);
		AddThemeStyleboxOverride("pressed", emptyStyle);
		AddThemeStyleboxOverride("disabled", emptyStyle);
		AddThemeStyleboxOverride("focus", emptyStyle);

        Controller.GetInstance().CartaUsadaEvent += OnCartaUsada;
    }

    public void SetCarta(CartaMovimiento carta)
    {
        _cartaRepresentada = carta;
        string nombreArchivo = ObtenerNombreAsset(carta);
        string rutaImagen = $"res://Assets/Cartas Movimiento/{nombreArchivo}";

        // Corregido: ResourceLoader en lugar de ResourseLoader
        if (ResourceLoader.Exists(rutaImagen) && _frente != null)
        {
            _frente.Texture = GD.Load<Texture2D>(rutaImagen);
        }
    }

    public override void _ExitTree()
    {
        if (Controller._instance != null)
        {
            Controller.GetInstance().CartaUsadaEvent -= OnCartaUsada;
        }
    }

    private string ObtenerNombreAsset(CartaMovimiento carta)
{
    switch (carta)
    {
        case MovimientoDiagonalContiguo:
            return "Mov_ContDiag.png";

        case MovimientoLateralContiguo:
            return "Mov_ContLinea.png";

        case MovimientoLateralAlBorde:
            return "Mov_Lateral.png";

        case MovimientoEnL mL when mL.DireccionPermitida == MovimientoEnL.TipoDireccion.Derecha:
            return "Mov_LDerecha.png";

        case MovimientoEnL mL when mL.DireccionPermitida == MovimientoEnL.TipoDireccion.Izquierda:
            return "Mov_LIzquierda.png";

        case MovimientoEnL:
            return "Mov_LDerecha.png";

        case MovimientoDiagonalConEspacio:
            return "Mov_UnEspacioDiagonal.png";

        case MovimientoLateralConEspacio:
            return "Mov_UnEspacioLinea.png";

        default:
            return "Mov_ContLinea.png";
    }
}

    public void SetEstadoUsadaDirecto()
    {
        _estaVolteada = true;
        Disabled = true;

        if (_frente != null) _frente.Hide();
        if (_dorso != null) _dorso.Show();
    }

    public override void _Pressed()
    {
        if (_estaVolteada) return;

        Controller.GetInstance().CambiarCartaSeleccionada(_cartaRepresentada);
    }

    private void OnCartaUsada(CartaMovimiento cartaUsada)
    {
        if (cartaUsada != null && _cartaRepresentada != null && cartaUsada == _cartaRepresentada && !_estaVolteada)
        {
            GD.Print($"Animando carta usada: {_cartaRepresentada}");
            AnimarVolteoUsada();
        }
    }

    public void AnimarVolteoUsada()
    {
        if (_estaVolteada) return;

        _estaVolteada = true;
        Disabled = true;

        PivotOffset = Size / 2;

        Tween tween = CreateTween();

        tween.TweenProperty(this, "scale", new Vector2(0.0f, Scale.Y), 0.25f)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.In);

        tween.TweenCallback(Callable.From(() => {
            if (_frente != null) _frente.Hide();
            if (_dorso != null) _dorso.Show();
        }));

        tween.TweenProperty(this, "scale", new Vector2(1.0f, Scale.Y), 0.25f)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.Out);
    }
}