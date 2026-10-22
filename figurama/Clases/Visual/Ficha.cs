using Godot;

public partial class Ficha : Node3D
{
    [Signal]
    public delegate void ClickeadaEventHandler(Ficha ficha);

    public FichaData Datos;
    private Color _colorVisualActual;
    private FichaModelo _modelo;
    private Vector3 _posicionNueva;
    private bool _moverse = false;
    private float velocidadMovimiento = 5.0f;

    public override void _Ready()
    {
        _modelo = GetNode<FichaModelo>("Modelo");

        Area3D areaDeClick = GetNode<Area3D>("Area3D");
        areaDeClick.InputEvent += OnInputEvent;
    }

    public override void _Process(double delta)
    {
        MoverAPosicion((float)delta);
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

    public void AnimarCambio(Vector3 nuevaPosicion)
    {
        _posicionNueva = nuevaPosicion;
        _moverse = true;
    }

    private void MoverAPosicion(float delta)
    {
        if (_moverse)
        {
            Vector3 newPosition = GlobalTransform.Origin.Lerp(_posicionNueva, delta * velocidadMovimiento);
			GlobalTransform = GlobalTransform with { Origin = newPosition };
			if (GlobalTransform.Origin.DistanceTo(_posicionNueva) < 0.01f)
			{
				_moverse = false;
				GlobalTransform = GlobalTransform with { Origin = _posicionNueva };
			}
        }
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
        GetNode<CsgCombiner3D>("Modelo").GetNode<Decal>("Completada").Visible = false;
    }
}