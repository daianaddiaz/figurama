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
		abuelo.GetNode<MeshInstance3D>("MeshInstance3D").GetNode<Decal>("Comodin").Visible = true;
		abuelo.GetNode<Node>("Sonidos").GetNode<AudioStreamPlayer>("ComodinSFX").Play();
	}

	public override void OnExit()
	{
		abuelo.GetNode<MeshInstance3D>("MeshInstance3D").GetNode<Decal>("Comodin").Visible = false;
	}
}