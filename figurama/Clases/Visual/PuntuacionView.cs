using Godot;
using System;

public partial class PuntuacionView : Label
{
	private string puntuacion = "0";

	public override void _Ready()
	{
		Text = $"Figuras formadas: {puntuacion}";
	}

	public void ActualizarPuntuacion(int nuevaPuntuacion)
	{
		puntuacion = nuevaPuntuacion.ToString();
		Text = $"Figuras formadas: {puntuacion}";
	}
}
