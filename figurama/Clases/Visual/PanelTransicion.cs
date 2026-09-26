using Godot;
using System.Threading.Tasks;

public partial class PanelTransicion : CanvasLayer
{
    private Control _panelContenedor;
    private Label _labelTurno;

    public override void _Ready()
    {
        // Usamos GetNodeOrNull con fallback por si cambia la estructura de nodos
        _panelContenedor = GetNodeOrNull<Control>("PanelContenedor") 
                            ?? GetNodeOrNull<Control>("%PanelContenedor");

        _labelTurno = GetNodeOrNull<Label>("PanelContenedor/ColorRect/LabelTurno") 
                      ?? GetNodeOrNull<Label>("PanelContenedor/LabelTurno")
                      ?? GetNodeOrNull<Label>("%LabelTurno");

        if (_panelContenedor != null)
        {
            _panelContenedor.MouseFilter = Control.MouseFilterEnum.Ignore;
        }
        else
        {
            GD.PrintErr("[PanelTransicion] No se encontró el nodo 'PanelContenedor'.");
        }
    }

    public async Task ReproducirTransicionAsync(string nombreJugador, float duracionPausa = 1.5f)
    {
        if (_panelContenedor == null)
        {
            QueueFree();
            return;
        }

        if (_labelTurno != null)
        {
            _labelTurno.Text = $"Próximo Turno: {nombreJugador}";
        }

        Vector2 tamanoPantalla = GetViewport().GetVisibleRect().Size;

        // Posición inicial: fuera de pantalla a la izquierda
        _panelContenedor.Position = new Vector2(-tamanoPantalla.X, 0);

        // 1. Paneo de entrada al centro
        Tween tweenEntrada = CreateTween();
        tweenEntrada.TweenProperty(_panelContenedor, "position", Vector2.Zero, 0.4f)
                    .SetTrans(Tween.TransitionType.Cubic)
                    .SetEase(Tween.EaseType.Out);

        await ToSignal(tweenEntrada, Tween.SignalName.Finished);

        // 2. Espera para lectura
        await ToSignal(GetTree().CreateTimer(duracionPausa), SceneTreeTimer.SignalName.Timeout);

        // 3. Paneo de salida a la derecha
        Tween tweenSalida = CreateTween();
        tweenSalida.TweenProperty(_panelContenedor, "position", new Vector2(tamanoPantalla.X, 0), 0.4f)
                   .SetTrans(Tween.TransitionType.Cubic)
                   .SetEase(Tween.EaseType.In);

        await ToSignal(tweenSalida, Tween.SignalName.Finished);

        QueueFree();
    }
}