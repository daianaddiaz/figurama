using Godot;

public partial class Ficha : Node3D
{
    [Signal]
    public delegate void ClickeadaEventHandler(Ficha ficha);

    public int Fila;
    public int Columna;

    private MeshInstance3D _visual;

    public override void _Ready()
    {
        _visual = GetNode<MeshInstance3D>("MeshInstance3D");

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
        var material = new StandardMaterial3D();
        material.AlbedoColor = color;
        _visual.MaterialOverride = material;
    }
}