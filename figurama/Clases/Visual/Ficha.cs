using Godot;

public partial class Ficha : Node3D
{
    [Signal]
    public delegate void ClickeadaEventHandler(Ficha ficha);

    public FichaData Datos;

    private MeshInstance3D _visual;
    private Color _colorVisualActual;
    private FichaModelo _modelo;

    public override void _Ready()
    {
        _visual = GetNode<MeshInstance3D>("MeshInstance3D");
        _modelo = GetNode<FichaModelo>("Modelo");

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

    public void SetearColor(Color colorElegido, Color colorSecundario, int indice)
    {
        _colorVisualActual = colorElegido;
        _modelo.SetearColor(colorElegido, colorSecundario, indice);
    }

    public void _on_ficha_bloqueada(Ficha ficha)
    {
        if (ficha == this)
        {
            GetNode<StateMachine>("FSM").ChangeState(EstadoFSM.Bloqueada);
        }
    }

    public void _on_ficha_desbloqueada(Ficha ficha)
    {
        if (ficha == this)
        {
            GetNode<StateMachine>("FSM").ChangeState(EstadoFSM.Neutral);
        }
    }

    public void _on_ficha_comodin_activado(Ficha ficha)
    {
        if (ficha == this)
        {
            GetNode<Node>("Sonidos").GetNode<AudioStreamPlayer>("ComodinSFX").Play();
            GetNode<StateMachine>("FSM").ChangeState(EstadoFSM.Comodin);
        }
    }

    public void _on_ficha_comodin_desactivado(Ficha ficha)
    {
        if (ficha == this)
        {
            GetNode<StateMachine>("FSM").ChangeState(EstadoFSM.Neutral);
        }
    }

    public void _on_ficha_desclickeada(Ficha ficha)
    {   
        if(ficha == this)
        {
            GetNode<StateMachine>("FSM").ChangeState(Datos.EsComodin ? EstadoFSM.Comodin : EstadoFSM.Neutral);
        }
    }

    public void _on_ficha_seleccionada(Ficha ficha)
    {   
        if(ficha == this)
        {   
            GetNode<StateMachine>("FSM").ChangeState(Datos.EsComodin ? EstadoFSM.ComodinSeleccionado : EstadoFSM.Seleccionada);
        }
    }

    public void _on_ficha_disponible(Ficha ficha)
    {   
        if(ficha == this)
        {   
            GetNode<StateMachine>("FSM").ChangeState(Datos.EsComodin ? EstadoFSM.ComodinDisponible : EstadoFSM.Disponible);
        }
    }

    public void _on_figura_completada(Ficha ficha)
    {   
        if (ficha == this)
        {
            GetNode<StateMachine>("FSM").ChangeState(Datos.EsComodin ? EstadoFSM.ComodinCompletada : EstadoFSM.Completada);
        }
    }

    public void _on_timer_efecto_completada_timeout()
    {
        GetNode<MeshInstance3D>("MeshInstance3D").GetNode<Decal>("Completada").Visible = false;
    }
}