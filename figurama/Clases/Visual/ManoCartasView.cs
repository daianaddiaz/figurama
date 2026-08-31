using Godot;
using System;
using System.Collections.Generic;
using System.Numerics;

public partial class ManoCartasView : Node3D
{

    [Export] public PackedScene CartaDeMovimientoScene;

    public void crearMano(List<CartaMovimiento> cartas)
    {
        for (int i = 0; i < cartas.Count; i++)
        {
            var cartaView = CartaDeMovimientoScene.Instantiate<CartaMovimientoView>();
            cartaView.SetCarta(cartas[i]);
            GetNode<VBoxContainer>("BotonesMovimientos").AddChild(cartaView);
        }
    }

}
