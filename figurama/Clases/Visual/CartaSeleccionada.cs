using Godot;
using System;

public partial class CartaSeleccionada : State
{
	private Button abuelo;

	public override void _Ready()
	{
		abuelo = GetParent<StateMachine>().GetParent<Button>();
	}

	public override void OnEnter()
	{
		abuelo.GetNode<TextureRect>("Seleccionada").Visible = true;
	}

	public override void OnExit()
	{
		abuelo.GetNode<TextureRect>("Seleccionada").Visible = false;
	}
}
