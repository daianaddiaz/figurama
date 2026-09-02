using Godot;
using System;

public partial class CartaMovimientoView : Button
{

	private CartaMovimiento cartaRepresentada;

	public void SetCarta(CartaMovimiento carta)
	{
		cartaRepresentada = carta;
		Text = carta.Nombre; // Muestra el nombre de la clase de la carta
	}

	public override void _Pressed()
	{
		Controller.GetInstance().CambiarCartaSeleccionada(cartaRepresentada);
	}
}
