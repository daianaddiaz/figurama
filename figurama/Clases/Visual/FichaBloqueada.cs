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
		abuelo.GetNode<FichaModelo>("Modelo").SetearColorFuego();
		abuelo.GetNode<Node>("Sonidos").GetNode<AudioStreamPlayer>("FuegoSFX").Play();
	}

	public override void OnExit()
	{
		abuelo.GetNode<Node>("Sonidos").GetNode<AudioStreamPlayer>("FuegoSFX").Stop();
	}
}