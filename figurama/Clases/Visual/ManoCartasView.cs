using Godot;
using System;
using System.Collections.Generic;
using System.Numerics;

public partial class ManoCartasView : Control
{

    [Export] public PackedScene CartaDeMovimientoScene;

    private Jugador _jugador;

    public void crearMano(Jugador jugador)
    {
        _jugador = jugador;

        RefrescarCartas();

        GetNode<Button>("BotonReroll").Pressed += OnRerollPresionado;
    }

    private void RefrescarCartas()
    {
        var contenedor = GetNode<VBoxContainer>("BotonesMovimientos");

        foreach (Node hijo in contenedor.GetChildren())
        {
            hijo.QueueFree();
        }

        foreach (CartaMovimiento carta in _jugador.manoCartas)
        {
            var cartaView = CartaDeMovimientoScene.Instantiate<CartaMovimientoView>();
            cartaView.SetCarta(carta);
            contenedor.AddChild(cartaView);
        }
    }

    private void OnRerollPresionado()
    {
        if (Controller.GetInstance().RerollearManoActual())
        {
            RefrescarCartas();
        }
    }
}

