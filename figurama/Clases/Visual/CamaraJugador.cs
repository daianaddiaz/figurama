using Godot;
using System;

public partial class CamaraJugador : Camera3D
{
	Vector3 _posicionInicial;
	Vector3 _rotacionInicial;

	Vector3 _posicionObjetivo;
	Vector3 _rotacionObjetivo;

	private float velocidadMovimiento = 5.0f;
	private float velocidadRotacion = 0.05f;

	private bool moviendoArriba = false;
	private bool moviendoAbajo = false;
	private bool rotandoArriba = false;
	private bool rotandoAbajo = false;

	public override void _Ready()
	{
		_posicionInicial = GlobalPosition;
		_rotacionInicial = GlobalRotationDegrees;

		_posicionObjetivo = new Vector3(2.0f, 5.5f, 3.0f);
		_rotacionObjetivo = new Vector3(-89.5f, 0.0f, 0.0f);
	}

	public override void _Process(double delta)
	{
		MoverArriba((float)delta);
		MoverAbajo((float)delta);
		RotarArriba((float)delta);
		RotarAbajo((float)delta);
	}

	public void _on_carta_seleccionada()
	{
		moviendoArriba = true;
		rotandoArriba = true;
	}

	public void _on_carta_no_seleccionada()
	{
		moviendoAbajo = true;
		rotandoAbajo = true;
	}

	private void MoverArriba(float delta)
	{
		if (moviendoArriba)
		{
			Vector3 newPosition = GlobalTransform.Origin.Lerp(_posicionObjetivo, delta * velocidadMovimiento);
			GlobalTransform = GlobalTransform with { Origin = newPosition };
			if (GlobalTransform.Origin.DistanceTo(_posicionObjetivo) < 0.01f)
			{
				moviendoArriba = false;
			}
		}
	}

	private void MoverAbajo(float delta)
	{
		if (moviendoAbajo)
		{
			Vector3 newPosition = GlobalTransform.Origin.Lerp(_posicionInicial, delta * velocidadMovimiento);
			GlobalTransform = GlobalTransform with { Origin = newPosition };
			if (GlobalTransform.Origin.DistanceTo(_posicionInicial) < 0.01f)
			{
				moviendoAbajo = false;
			}
		}
	}

	private void RotarArriba(float delta)
	{
		if (rotandoArriba)
		{
			Vector3 newRotation = Rotation.Slerp(_rotacionObjetivo, delta * velocidadRotacion);
			Rotation = newRotation;
			GD.Print($"Rotación actual: {GlobalRotationDegrees}, Rotación objetivo: {_rotacionObjetivo}");
			if (GlobalRotationDegrees.X < _rotacionObjetivo.X)
			{
				rotandoArriba = false;
			}
		}
	}

	private void RotarAbajo(float delta)
	{
		if (rotandoAbajo)
		{
			Vector3 newRotation = Rotation.Slerp(_rotacionInicial, delta * -velocidadRotacion);
			Rotation = newRotation;
			GD.Print($"Rotación actual: {GlobalRotationDegrees}, Rotación objetivo: {_rotacionInicial}");
			if (GlobalRotationDegrees.X > _rotacionInicial.X)
			{
				rotandoAbajo = false;
			}
		}
	}
}
