using Godot;
using System;

public partial class CartaDeFiguraView : Label
{

	private FiguraAsignada _carta;

	public void SetCarta(FiguraAsignada carta)
	{
		_carta = carta;
		Text = _carta.Figura.Nombre; // Muestra el nombre de la clase de la carta
	}

	public void Actualizar()
	{
		CambiarColorSegunEstado();
	}

	private void CambiarColorSegunEstado()
	{
		if (_carta != null && _carta.Completada)
		{
			SelfModulate = new Color(0, 1, 0); // Verde si está completada
		}
	}
}
