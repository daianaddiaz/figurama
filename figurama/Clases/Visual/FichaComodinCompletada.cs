using Godot;

public partial class FichaComodinCompletada : State
{
	private Node3D abuelo;

	public override void _Ready()
	{
		abuelo = GetParent<StateMachine>().GetParent<Node3D>();
	}

	public override void OnEnter()
	{
		abuelo.GetNode<CsgCombiner3D>("Modelo").GetNode<Decal>("Completada").Visible = true;
		abuelo.GetNode<Timer>("TimerEfectoCompletada").Start();
	}
	public override void OnExit()
	{
		abuelo.GetNode<CsgCombiner3D>("Modelo").GetNode<Decal>("Completada").Visible = false;
	}
}