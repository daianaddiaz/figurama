using Godot;

public partial class FichaComodin : State
{
	private Node3D abuelo;

	public override void _Ready()
	{
		abuelo = GetParent<StateMachine>().GetParent<Node3D>();
	}

	public override void OnEnter()
	{
		abuelo.GetNode<CsgCombiner3D>("Modelo").GetNode<Decal>("Comodin").Visible = true;
	}

	public override void OnExit()
	{
		abuelo.GetNode<CsgCombiner3D>("Modelo").GetNode<Decal>("Comodin").Visible = false;
	}
}