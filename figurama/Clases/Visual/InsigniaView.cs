using Godot;
using System.Collections.Generic;

public partial class InsigniaView : Control
{

	private List<Color> _coloresDisponibles = new List<Color>{Colors.MediumVioletRed, Colors.Chartreuse, Colors.RoyalBlue, Colors.Yellow};
	private Label _nombre;
	private TextureRect _fondo;
	private TextureRect _decoracion;
	private TextureRect _icono;
	private int _indice;

    public override void _Ready()
    {
        _nombre = GetNode<Label>("NombreJugador");
		_fondo = GetNode<TextureRect>("Fondo");
		_decoracion = GetNode<TextureRect>("Decoracion");
		_icono = GetNode<TextureRect>("Icono");
    }


	public void SetInsignia(string nombreJugador, string texturaInsignia, int indiceJugador)
	{
		GD.Print($"SetInsignia: nombreJugador={nombreJugador}, texturaInsignia={texturaInsignia}, indiceJugador={indiceJugador}");
		_nombre.Text = nombreJugador;
		_fondo.Modulate = _coloresDisponibles[indiceJugador % _coloresDisponibles.Count];
		_decoracion.Modulate = _coloresDisponibles[indiceJugador % _coloresDisponibles.Count];
		_icono.Texture = GD.Load<Texture2D>(texturaInsignia);
		_indice = indiceJugador;
	}

	public void SetInsigniaSecundaria(ManoCartasView manoAsignada, string texturaInsignia, int indiceJugador)
	{
		GD.Print($"SetInsigniaSecundaria: manoAsignada={manoAsignada}, texturaInsignia={texturaInsignia}, indiceJugador={indiceJugador}");
		Scale = new Vector2(0.75f, 0.75f);
		_nombre.Hide();
		_fondo.Modulate = _coloresDisponibles[indiceJugador % _coloresDisponibles.Count];
		_decoracion.Modulate = _coloresDisponibles[indiceJugador % _coloresDisponibles.Count];
		_icono.Texture = GD.Load<Texture2D>(texturaInsignia);
		_indice = indiceJugador;

		var contenedor = GetNode<HBoxContainer>("ContenedorSecundario");
		foreach (FiguraAsignada figura in manoAsignada.figurasAsignadas)
		{
			var cartaFigura = figura.Figura.RutaImagen;
			var cartaFiguraNode = new TextureRect{Texture = GD.Load<Texture2D>(cartaFigura), CustomMinimumSize = new Vector2(36, 50), CustomMaximumSize = new Vector2(36, 50)};
			contenedor.AddChild(cartaFiguraNode);
		}
	}

	public void SetInsigniaSecundariaJugador(Jugador jugador, string texturaInsignia, int indiceJugador)
	{
		Scale = new Vector2(0.75f, 0.75f);
		_nombre.Hide();
		_fondo.Modulate = _coloresDisponibles[indiceJugador % _coloresDisponibles.Count];
		_decoracion.Modulate = _coloresDisponibles[indiceJugador % _coloresDisponibles.Count];
		_icono.Texture = GD.Load<Texture2D>(texturaInsignia);
		_indice = indiceJugador;

		var contenedor = GetNode<HBoxContainer>("ContenedorSecundario");
		foreach (FiguraAsignada figura in jugador.figurasAArmar.FindAll(f => !f.Completada))
		{
			var cartaFiguraNode = new TextureRect
			{
				Texture = GD.Load<Texture2D>(figura.Figura.RutaImagen),
				CustomMinimumSize = new Vector2(36, 50),
				CustomMaximumSize = new Vector2(36, 50)
			};
			contenedor.AddChild(cartaFiguraNode);
		}
	}

	public void SetDecoracion(int indice)
	{
		if(indice == _indice)
		{
			_decoracion.Show();
		}
		else
		{
			_decoracion.Hide();
		}
	}
}
