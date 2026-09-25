using Godot;
using System;

public partial class InsigniaView : Control
{
	public void SetInsignia(string nombreJugador, string texturaInsignia)
	{
		GD.Print($"SetInsignia: nombreJugador={nombreJugador}, texturaInsignia={texturaInsignia}");
		GetNode<TextureRect>("Insignia").GetNode<Label>("NombreJugador").Text = nombreJugador;
		GetNode<TextureRect>("Insignia").Texture = GD.Load<Texture2D>(texturaInsignia);
	}
}
