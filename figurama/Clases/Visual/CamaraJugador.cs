using Godot;
using System;

public partial class CamaraJugador : Camera3D
{
	Vector3 _posicionInicial;
	Vector3 _rotacionInicial;

	Vector3 _posicionObjetivo;
	Vector3 _rotacionObjetivo;

	public override void _Ready()
	{
		_posicionInicial = GlobalPosition;
		_rotacionInicial = Rotation;

		_posicionObjetivo = new Vector3(2.0f, 5.5f, 3.0f);
		_rotacionObjetivo = new Vector3(-89.5f, 0.0f, 0.0f);
	}

	public void _on_carta_seleccionada()
	{
		GD.Print("Rotacion");
		GlobalTransform = GlobalTransform with { Origin = _posicionObjetivo };
		GlobalRotation = _rotacionObjetivo;
	}
}
