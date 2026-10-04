using Godot;
using System;

public partial class FichaCompletada : State
{

	private Node3D abuelo;

	public override void _Ready()
	{
		abuelo = GetParent<StateMachine>().GetParent<Node3D>();
	}

	public override void OnEnter()
	{
		abuelo.GetNode<CsgCombiner3D>("Modelo").GetNode<Decal>("Completada").Visible = true;
		GetParent<StateMachine>().GetParent<Node3D>().GetNode<Timer>("TimerEfectoCompletada").Start();
	}
}
