using Godot;

public partial class SplashScreen : Control
{
    [Export] public string RutaMenuPrincipal = "res://Objetos/menuPrincipal.tscn";
    [Export] public float TiempoVisibilidadSegundos = 4.0f; // Tiempo total en segundos (máximo 7s)
    [Export] public float TiempoFade = 1.0f; // Duración de los efectos de entrada y salida

    private TextureRect _logo;
    private bool _cambiandoDeEscena = false;

    public override void _Ready()
    {
        _logo = GetNode<TextureRect>("CenterContainer/Logo");

        // Arrancamos con el logo transparente
        _logo.Modulate = new Color(1, 1, 1, 0);

        // Animación con Tween: Fade In -> Espera -> Fade Out -> Cambiar Escena
        Tween tween = CreateTween();

        // 1. Fade In (Aparece el logo)
        tween.TweenProperty(_logo, "modulate:a", 1.0f, TiempoFade)
             .SetTrans(Tween.TransitionType.Cubic)
             .SetEase(Tween.EaseType.Out);

        // 2. Tiempo en pantalla
        float tiempoEspera = TiempoVisibilidadSegundos - (TiempoFade * 2.0f);
        if (tiempoEspera > 0)
        {
            tween.TweenInterval(tiempoEspera);
        }

        // 3. Fade Out (Desaparece el logo)
        tween.TweenProperty(_logo, "modulate:a", 0.0f, TiempoFade)
             .SetTrans(Tween.TransitionType.Cubic)
             .SetEase(Tween.EaseType.In);

        // 4. Cambiar al menú principal
        tween.TweenCallback(Callable.From(IrAlMenuPrincipal));
    }

    public override void _Input(InputEvent @event)
    {
        // Permitir que el usuario saltee la pantalla tocando una tecla o clic
        if (!_cambiandoDeEscena && (@event is InputEventMouseButton mouseEvt && mouseEvt.Pressed || @event is InputEventKey keyEvt && keyEvt.Pressed))
        {
            IrAlMenuPrincipal();
        }
    }

    private void IrAlMenuPrincipal()
    {
        if (_cambiandoDeEscena) return;
        _cambiandoDeEscena = true;

        GetTree().ChangeSceneToFile(RutaMenuPrincipal);
    }
}