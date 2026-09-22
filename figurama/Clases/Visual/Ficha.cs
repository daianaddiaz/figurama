using Godot;

public partial class Ficha : Node3D
{
    [Signal]
    public delegate void ClickeadaEventHandler(Ficha ficha);

    public FichaData Datos;

    private MeshInstance3D _visual;
    private SpotLight3D _luz;
    private Color _colorVisualActual;

    public override void _Ready()
    {
        _visual = GetNode<MeshInstance3D>("MeshInstance3D");
        _luz = GetNode<SpotLight3D>("SpotLight3D");

        Area3D areaDeClick = GetNode<Area3D>("Area3D");
        areaDeClick.InputEvent += OnInputEvent;
    }

    private void OnInputEvent(Node camara, InputEvent evento, Vector3 posicionClick, Vector3 normal, long shapeIdx)
    {
        if (evento is InputEventMouseButton mouseEvento &&
            mouseEvento.Pressed &&
            mouseEvento.ButtonIndex == MouseButton.Left)
        {   
            EmitSignal(SignalName.Clickeada, this);
        }
    }

    public void SetearColor(Color color)
    {
        _colorVisualActual = color;
        var material = new StandardMaterial3D();
        material.AlbedoColor = color;
        _visual.MaterialOverride = material;
        _luz.LightColor = color;
    }

    public void _on_ficha_desclickeada(Ficha ficha)
    {   
        if(ficha == this)
        {
            GetNode<StateMachine>("FSM").ChangeState("Neutral");
        }
    }

    public void _on_ficha_seleccionada(Ficha ficha)
    {   
        if(ficha == this)
        {   
            GetNode<StateMachine>("FSM").ChangeState("Seleccionada");
        }
    }

    public void _on_ficha_disponible(Ficha ficha)
    {   
        if(ficha == this)
        {   
            GetNode<StateMachine>("FSM").ChangeState("Disponible");
        }
    }

    public void _on_figura_completada(Ficha ficha)
    {   
        
    }

    public void _on_timer_efecto_completada_timeout()
    {
        GetNode<StateMachine>("FSM").ChangeState("Neutral");
    }

    private static readonly Color ColorComodin = Colors.MidnightBlue;
    private Color _colorAntesDeComodin;

    public void ActivarComodinVisual()
    {
        _colorAntesDeComodin = _colorVisualActual;
        SetearColor(ColorComodin);
        GetNode<Node>("Sonidos").GetNode<AudioStreamPlayer>("ComodinSFX").Play();
    }

    public void DesactivarComodinVisual()
    {
        SetearColor(_colorAntesDeComodin);
    }

}