using Godot;
using System.Collections.Generic;

public partial class InsigniaView : Control
{

	private List<Color> _coloresDisponibles = new List<Color>{Colors.MediumVioletRed, Colors.Chartreuse, Colors.RoyalBlue, Colors.Yellow};

	public void SetInsignia(string nombreJugador, string texturaInsignia, int indiceJugador)
	{
		GD.Print($"SetInsignia: nombreJugador={nombreJugador}, texturaInsignia={texturaInsignia}, indiceJugador={indiceJugador}");
		GetNode<Label>("NombreJugador").Text = nombreJugador;
		GetNode<TextureRect>("Fondo").Modulate = _coloresDisponibles[indiceJugador % _coloresDisponibles.Count];
		GetNode<TextureRect>("Decoracion").Modulate = _coloresDisponibles[indiceJugador % _coloresDisponibles.Count];
		GetNode<TextureRect>("Icono").Texture = GD.Load<Texture2D>(texturaInsignia);
	}

	public void SetInsigniaSecundaria(ManoCartasView manoAsignada, string texturaInsignia, int indiceJugador)
	{
		GD.Print($"SetInsigniaSecundaria: manoAsignada={manoAsignada}, texturaInsignia={texturaInsignia}, indiceJugador={indiceJugador}");
		Scale = new Vector2(0.75f, 0.75f);
		GetNode<Label>("NombreJugador").Hide();
		GetNode<TextureRect>("Fondo").Modulate = _coloresDisponibles[indiceJugador % _coloresDisponibles.Count];
		GetNode<TextureRect>("Decoracion").Hide();
		GetNode<TextureRect>("Icono").Texture = GD.Load<Texture2D>(texturaInsignia);

		var contenedor = GetNode<HBoxContainer>("ContenedorSecundario");
		foreach (FiguraAsignada figura in manoAsignada.figurasAsignadas)
		{
			var cartaFigura = figura.Figura.RutaImagen;
			var cartaFiguraNode = new TextureRect{Texture = GD.Load<Texture2D>(cartaFigura), CustomMinimumSize = new Vector2(36, 50), CustomMaximumSize = new Vector2(36, 50)};
			contenedor.AddChild(cartaFiguraNode);
		}
	}
}
