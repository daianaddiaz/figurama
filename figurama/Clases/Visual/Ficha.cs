using Godot;

public partial class Ficha : Node3D
{
    [Signal]
    public delegate void ClickeadaEventHandler(Ficha ficha);

    // La hitbox es una lámina fina sobre la cara superior de la ficha, un poco más chica que la celda (0.8)
    private const float LadoMaximoHitbox = 0.76f;
    private const float GrosorHitbox = 0.06f;
    private static bool _hitboxInformada = false;

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

        AjustarHitbox();
    }

    public override void _Process(double delta)
    {
        MoverAPosicion((float)delta);
    }

    // Alinea la hitbox con el modelo. Una hitbox alta se "pisa" con las fichas vecinas
    // cuando se la ve desde la cámara inclinada.
    private async void AjustarHitbox()
    {
        // El modelo CSG se genera recién en los primeros cuadros
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        if (!GodotObject.IsInstanceValid(this) || !IsInsideTree()) return;

        var forma = GetNodeOrNull<CollisionShape3D>("Area3D/CollisionShape3D");
        if (forma == null || forma.Shape is not BoxShape3D caja) return;

        Aabb enModelo = _modelo.GetAabb();
        if (enModelo.Size == Vector3.Zero)
        {
            GD.PrintErr("[Ficha] El modelo todavía no tiene volumen: no se ajustó la hitbox.");
            return;
        }

        // Del espacio del modelo al espacio de la ficha
        Aabb enFicha = _modelo.Transform * enModelo;
        Vector3 centro = enFicha.Position + enFicha.Size * 0.5f;
        float cima = enFicha.Position.Y + enFicha.Size.Y;

        caja.Size = new Vector3(
            Mathf.Min(enFicha.Size.X, LadoMaximoHitbox),
            GrosorHitbox,
            Mathf.Min(enFicha.Size.Z, LadoMaximoHitbox));
        forma.Position = new Vector3(centro.X, cima - GrosorHitbox * 0.5f, centro.Z);

        if (!_hitboxInformada)
        {
            _hitboxInformada = true;
            GD.Print($"[Hitbox] modelo (en la ficha): pos={enFicha.Position} tam={enFicha.Size} -> hitbox centro={forma.Position} tam={caja.Size}");
        }
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
        if (ficha == this)
        {
            GetNode<StateMachine>("FSM").ChangeState(Datos.EsComodin ? EstadoFSM.Comodin : EstadoFSM.Neutral);
        }
    }

    public void _on_ficha_seleccionada(Ficha ficha)
    {
        if (ficha == this)
        {
            GetNode<StateMachine>("FSM").ChangeState(Datos.EsComodin ? EstadoFSM.ComodinSeleccionado : EstadoFSM.Seleccionada);
        }
    }

    public void _on_ficha_disponible(Ficha ficha)
    {
        if (ficha == this)
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