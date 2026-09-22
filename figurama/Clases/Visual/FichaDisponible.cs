using Godot;
using System;

public partial class FichaDisponible : State
{

	private Node3D abuelo;

	public override void _Ready()
	{
		abuelo = GetParent<StateMachine>().GetParent<Node3D>();
	}

	public override void OnEnter()
	{
		abuelo.GetNode<MeshInstance3D>("MeshInstance3D").GetNode<Decal>("Disponible").Visible = true;
	}

	public override void OnExit()
	{
		abuelo.GetNode<MeshInstance3D>("MeshInstance3D").GetNode<Decal>("Disponible").Visible = false;
	}
	
}
