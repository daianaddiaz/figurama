using Godot;

public partial class FichaComodinDisponible : State
{
	private Node3D abuelo;

	public override void _Ready()
	{
		abuelo = GetParent<StateMachine>().GetParent<Node3D>();
	}

	public override void OnEnter()
	{
		abuelo.GetNode<MeshInstance3D>("MeshInstance3D").GetNode<Decal>("Disponible").Visible = true;
		abuelo.GetNode<MeshInstance3D>("MeshInstance3D").GetNode<Decal>("Comodin").Visible = true;
	}

	public override void OnExit()
	{
		abuelo.GetNode<MeshInstance3D>("MeshInstance3D").GetNode<Decal>("Disponible").Visible = false;
		abuelo.GetNode<MeshInstance3D>("MeshInstance3D").GetNode<Decal>("Comodin").Visible = false;
	}
}