using Godot;
using System;

public partial class CamaraJugador : Camera3D
{
	Vector3 _posicionInicial;
	Vector3 _rotacionInicial;

	Vector3 _posicionObjetivo;
	Vector3 _rotacionObjetivo;

	private Control flechasUI;

	private float velocidadMovimiento = 5.0f;
	private float velocidadRotacion = 0.04f;

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

		flechasUI = GetParent<Node3D>().GetNode<Control>("UITemporal/CambioPerspectiva");
	}

	public override void _Process(double delta)
	{
		MoverArriba((float)delta);
		MoverAbajo((float)delta);
		RotarArriba((float)delta);
		RotarAbajo((float)delta);
	}

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed)
        {
            if (@event.IsActionPressed("mirarArriba"))
            {
                _on_carta_seleccionada();
            }
            else if (@event.IsActionPressed("mirarAbajo"))
            {
                _on_carta_no_seleccionada();
            }
        }
    }


	public void _on_carta_seleccionada()
	{
		moviendoArriba = true;
		rotandoArriba = true;
		moviendoAbajo = false;
		rotandoAbajo = false;
		flechasUI.GetNode<Control>("Bajada").Show();
		flechasUI.GetNode<Control>("Subida").Hide();
	}

	public void _on_carta_no_seleccionada()
	{
		moviendoAbajo = true;
		rotandoAbajo = true;
		moviendoArriba = false;
		rotandoArriba = false;
		flechasUI.GetNode<Control>("Subida").Show();
		flechasUI.GetNode<Control>("Bajada").Hide();
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
				GlobalTransform = GlobalTransform with { Origin = _posicionObjetivo };
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
				GlobalTransform = GlobalTransform with { Origin = _posicionInicial };
			}
		}
	}

	private void RotarArriba(float delta)
	{
		if (rotandoArriba)
		{
			Vector3 newRotation = Rotation.Slerp(_rotacionObjetivo, delta * velocidadRotacion);
			Rotation = newRotation;
			if (GlobalRotationDegrees.X < _rotacionObjetivo.X)
			{
				rotandoArriba = false;
				GlobalRotationDegrees = _rotacionObjetivo;
			}
		}
	}

	private void RotarAbajo(float delta)
	{
		if (rotandoAbajo)
		{
			Vector3 newRotation = Rotation.Slerp(_rotacionInicial, delta * -velocidadRotacion);
			Rotation = newRotation;
			if (GlobalRotationDegrees.X > _rotacionInicial.X)
			{
				rotandoAbajo = false;
				GlobalRotationDegrees = _rotacionInicial;
			}
		}
	}
}
