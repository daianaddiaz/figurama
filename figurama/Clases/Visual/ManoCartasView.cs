using Godot;
using System;
using System.Collections.Generic;
using System.Numerics;

public partial class ManoCartasView : Control
{

    [Export] public PackedScene CartaDeMovimientoScene;
    [Export] public PackedScene CartaDeFiguraScene;

    private Jugador _jugador;

    public void crearMano(Jugador jugador)
    {
        _jugador = jugador;

        crearCartasFiguras();
        RefrescarCartas();

        GetNode<Button>("BotonReroll").Pressed += OnRerollPresionado;
    }

    private void crearCartasFiguras()
    {

        var contenedor = GetNode<Control>("CartasFiguras");
        var i = 1;

        foreach (FiguraAsignada carta in _jugador.figurasAArmar)
        {
            var cartaView = CartaDeFiguraScene.Instantiate<CartaDeFiguraView>();
            cartaView.SetCarta(carta);
            cartaView.Position = new Godot.Vector2(0, i * 25); // Ajusta la posición vertical según el índice
            contenedor.AddChild(cartaView);
            i += 1;
        }
    }

    public void ActualizarMano()
    {

        var figuras = GetNode<Control>("CartasFiguras");

        foreach (Node hijo in figuras.GetChildren())
        {
            if (hijo is CartaDeFiguraView cartaView)
            {
                cartaView.Actualizar();
            }
        }

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

