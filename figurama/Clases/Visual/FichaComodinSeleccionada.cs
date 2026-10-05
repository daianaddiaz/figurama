using Godot;

public partial class FichaComodinSeleccionada : State
{
	private Node3D abuelo;

	public override void _Ready()
	{
		abuelo = GetParent<StateMachine>().GetParent<Node3D>();
	}

	public override void OnEnter()
	{
		abuelo.GetNode<CsgCombiner3D>("Modelo").GetNode<Decal>("Seleccionada").Visible = true;
		abuelo.GetNode<Node>("Sonidos").GetNode<AudioStreamPlayer>("ClickSFX").Play();
	}

	public override void OnExit()
	{
		abuelo.GetNode<CsgCombiner3D>("Modelo").GetNode<Decal>("Seleccionada").Visible = false;
	}
}