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
    var contenedor = GetNode<Control>("CartasFiguras");

    foreach (Node hijo in contenedor.GetChildren())
    {
        hijo.QueueFree();
    }

    crearCartasFiguras();
    RefrescarCartas();
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
        if (carta != null)
        {
            var cartaView = CartaDeMovimientoScene.Instantiate<CartaMovimientoView>();
            cartaView.SetCarta(carta);
            contenedor.AddChild(cartaView);
        }
        else
        {
            // Muestra boton deshabilitado indicando "Usada"
            var botonUsado = new Button();
            botonUsado.Text = "Usada";
            botonUsado.Disabled = true;
            contenedor.AddChild(botonUsado);
        }
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

