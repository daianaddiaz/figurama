using Godot;
using System;

public partial class FichaCompletada : State
{
	public override void OnEnter()
	{
		GetParent<StateMachine>().GetParent<Node3D>().GetNode<SpotLight3D>("SpotLight3D").Visible = true;
		GetParent<StateMachine>().GetParent<Node3D>().GetNode<Timer>("TimerEfectoCompletada").Start();
	}
}
