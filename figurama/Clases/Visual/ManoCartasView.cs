using Godot;
using System;
using System.Collections.Generic;

public partial class ManoCartasView : Control
{
    [Export] public PackedScene CartaDeMovimientoScene;

    [Export] public PackedScene CartaDeFiguraScene; 

    [Export] private Control _contenedorMano;

    public List<FiguraAsignada> figurasAsignadas = new List<FiguraAsignada>();

    private Jugador _jugador;

    private Camera3D _camara;

    private int cantidadCartas = 0;

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed)
        {
            if (@event.IsActionPressed("reroll"))
            {
                OnRerollPresionado();
            }
        }
    }

    public void crearMano(Jugador jugador, Camera3D camara)
    {
        _jugador = jugador;
        _camara = camara;

        crearCartasFiguras();
        RefrescarCartas();

        var botonReroll = GetNode<Button>("BotonReroll");
        Controller.GetInstance().CartaSeleccionadaCambiada += OnCartaSeleccionadaCambiada;
        if (!botonReroll.IsConnected(Button.SignalName.Pressed, Callable.From(OnRerollPresionado)))
        {
            botonReroll.Pressed += OnRerollPresionado;
        }
    }

    private void crearCartasFiguras()
    {
        
        var contenedor = GetNodeOrNull<HBoxContainer>("CartasFiguras"); 

        if (contenedor == null)
        {
            GD.PrintErr("Error: No se encontró el contenedor HBoxContainer 'CartasFiguras'");
            return;
        }

        if (_jugador?.figurasAArmar == null)
        {
            GD.PrintErr("Error: _jugador o figurasAArmar es null");
            return;
        }

        // Separo las figuras pendientes (no completadas) y las completadas
        figurasAsignadas.Clear();
        var pendientes = _jugador.figurasAArmar.FindAll(f => !f.Completada);
        var completadas = _jugador.figurasAArmar.FindAll(f => f.Completada);
        figurasAsignadas.AddRange(pendientes);

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
            
            // Usamos la escena exportada
            var cartaView = CartaDeFiguraScene.Instantiate<CartaDeFiguraView>();
            
            // Asigna la textura (.png) y actualiza el estado interno
            cartaView.SetCarta(figura);

            // Modulamos o resaltamos la carta si está completada
            if (figura.Completada)
            {
                cartaView.SelfModulate = new Color(0.3f, 1.0f, 0.3f);
            }

            //cartaView.Position = new Vector2(0, (i + 1) * 25);
            contenedor.AddChild(cartaView);

            GD.Print($"[OK] Carta añadida: {figura.Figura?.Nombre}. Hijos en contenedor: {contenedor.GetChildCount()}");
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
        contenedor.RemoveChild(hijo); // Se quita del árbol en el acto
        hijo.QueueFree();             // Se marca para liberar memoria
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
        var contenedor = GetNode<HBoxContainer>("BotonesMovimientos");

        GD.Print("RefrescarCartas: destruyendo cartas viejas");

        foreach (Node hijo in contenedor.GetChildren())
        {
            contenedor.RemoveChild(hijo);
            hijo.QueueFree();
            cantidadCartas = 0;
        }
        if (_jugador?.manoCartas == null) return;

        foreach (CartaMovimiento carta in _jugador.manoCartas)
        {
            var cartaView = CartaDeMovimientoScene.Instantiate<CartaMovimientoView>();
            contenedor.AddChild(cartaView);
            cantidadCartas++;

            if (carta != null)
            {
                // Carta disponible
                cartaView.SetCarta(carta, _camara, cantidadCartas);
            }
            else
            {
                // Carta ya jugada
                cartaView.SetEstadoUsadaDirecto();
            }
        }
    }

    private void OnCartaSeleccionadaCambiada(CartaMovimiento cartaActiva)
    {
        var contenedor = GetNode<HBoxContainer>("BotonesMovimientos");
        foreach (Node hijo in contenedor.GetChildren())
        {
            if (hijo is CartaMovimientoView cartaView)
            {
                if (cartaActiva != null && cartaView.CartaRepresentada == cartaActiva)
                    cartaView._on_carta_seleccionada();
                else
                    cartaView._on_carta_no_seleccionada();
            }
        }
    }

    public void AnimarCartaUsada(CartaMovimiento carta)
    {
        var contenedor = GetNode<HBoxContainer>("BotonesMovimientos");

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
            GetNode<AudioStreamPlayer>("Sonidos/RerollSFX").Play();
            RefrescarCartas();
        }
    }
}