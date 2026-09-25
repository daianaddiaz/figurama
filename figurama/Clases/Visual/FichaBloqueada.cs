using Godot;

public partial class FichaBloqueada : State
{
	private Node3D abuelo;

	public override void _Ready()
	{
		abuelo = GetParent<StateMachine>().GetParent<Node3D>();
	}

	public override void OnEnter()
	{
		abuelo.GetNode<MeshInstance3D>("MeshInstance3D").GetNode<AnimatedSprite3D>("Bloqueada").Visible = true;
		abuelo.GetNode<Node>("Sonidos").GetNode<AudioStreamPlayer>("FuegoSFX").Play();
	}

	public override void OnExit()
	{
		abuelo.GetNode<MeshInstance3D>("MeshInstance3D").GetNode<AnimatedSprite3D>("Bloqueada").Visible = false;
		abuelo.GetNode<Node>("Sonidos").GetNode<AudioStreamPlayer>("FuegoSFX").Stop();
	}
}