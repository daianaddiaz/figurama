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

    var visibles = new List<FiguraAsignada>();

    foreach (var p in pendientes)
    {
        if (visibles.Count < _jugador.figurasAArmar.Count) visibles.Add(p);
    }

    foreach (var c in completadas)
    {
        if (visibles.Count < _jugador.figurasAArmar.Count) visibles.Add(c);
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
        actualizarPuntuacion();
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

private void actualizarPuntuacion()
    {
        var puntuacionView = GetNodeOrNull<PuntuacionView>("Puntuacion");
        if (puntuacionView != null)
        {
            int figurasCompletadas = _jugador.figurasAArmar.FindAll(f => f.Completada).Count;
            puntuacionView.ActualizarPuntuacion(figurasCompletadas);
        }
    }

    private void RefrescarCartas()
{
    var contenedor = GetNode<VBoxContainer>("BotonesMovimientos");

    foreach (Node hijo in contenedor.GetChildren())
    {
        hijo.QueueFree();
    }

    if (_jugador?.manoCartas == null) return;

    foreach (CartaMovimiento carta in _jugador.manoCartas)
        {
            var cartaView = CartaDeMovimientoScene.Instantiate<CartaMovimientoView>();
            contenedor.AddChild(cartaView);

            if (carta != null)
            {
                // Carta disponible: Muestra el frente activo
                cartaView.SetCarta(carta);
            }
            else
            {
                // Carta ya jugada: Muestra el dorso directamente (estado ya volteado)
                cartaView.SetEstadoUsadaDirecto();
            }
        }
    }

    public void AnimarCartaUsada(CartaMovimiento carta)
    {
        var contenedor = GetNode<VBoxContainer>("BotonesMovimientos");

        foreach (Node hijo in contenedor.GetChildren())
        {
            if (hijo is CartaMovimientoView cartaView && cartaView.CartaRepresentada == carta)
            {
                cartaView.AnimarVolteoUsada();
                break;
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

