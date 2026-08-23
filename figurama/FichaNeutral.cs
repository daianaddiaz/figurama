using Godot;
using System;

public partial class FichaNeutral : State
{
	public override void OnEnter()
	{
		GetParent<StateMachine>().GetParent<Node3D>().GetNode<SpotLight3D>("SpotLight3D").Visible = false;
	}
}
