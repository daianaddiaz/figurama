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

        var botonReroll = GetNode<Button>("BotonReroll");
        if (!botonReroll.IsConnected(Button.SignalName.Pressed, Callable.From(OnRerollPresionado)))
    {
        botonReroll.Pressed += OnRerollPresionado;
    }
    }

    private void crearCartasFiguras()
{
    var contenedor = GetNodeOrNull<Control>("CartasFiguras");
    if (contenedor == null || _jugador?.figurasAArmar == null) return;

    // Separo las figuras pendientes (no completadas) y las completadas
    var pendientes = _jugador.figurasAArmar.FindAll(f => !f.Completada);
    var completadas = _jugador.figurasAArmar.FindAll(f => f.Completada);

    // Defino cuales 3 cartas se mostrarán:
    // Toman prioridad hasta 3 pendientes. Si quedan lugares libres, se muestran las completadas.
    var visibles = new List<FiguraAsignada>();

    foreach (var p in pendientes)
    {
        if (visibles.Count < 3) visibles.Add(p);
    }

    foreach (var c in completadas)
    {
        if (visibles.Count < 3) visibles.Add(c);
    }

    // Renderizar en pantalla las cartas seleccionadas
    for (int i = 0; i < visibles.Count; i++)
    {
        FiguraAsignada figura = visibles[i];
        var cartaView = CartaDeFiguraScene.Instantiate<CartaDeFiguraView>();
        cartaView.SetCarta(figura);

        // Si la figura ya fue completada, se resalta en verde
        if (figura.Completada)
        {
            cartaView.Modulate = new Color(0.3f, 1.0f, 0.3f);
        }

        cartaView.Position = new Godot.Vector2(0, (i + 1) * 25);
        contenedor.AddChild(cartaView);
    }
}

   public void ActualizarMano()
{
        LimpiarContenedor("CartasFiguras");
        LimpiarContenedor("CartaReserva");

        crearCartasFiguras();
        RefrescarCartas();
}
private void LimpiarContenedor(string nombreNodo)
    {
        var contenedor = GetNodeOrNull<Control>(nombreNodo);
        if (contenedor == null) return;

        foreach (Node hijo in contenedor.GetChildren())
        {
            hijo.QueueFree();
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

