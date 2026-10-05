using Godot;
using System;

public partial class FichaNeutral : State
{
    private Node3D abuelo;

	public override void _Ready()
	{
		abuelo = GetParent<StateMachine>().GetParent<Node3D>();
	}

	public override void OnEnter()
	{
		abuelo.GetNode<FichaModelo>("Modelo").VolverABase();
	}
}
