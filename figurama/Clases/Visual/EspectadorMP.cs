using Godot;

// Pantalla del espectador: la misma interfaz que la de los jugadores, pero con una vista
// general de la partida (no depende de quién tiene el turno) y sin acciones.
public partial class EspectadorMP : Node3D
{
    [Export] public PackedScene InsigniaViewScene;
    [Export] public PackedScene TransicionScene;

    // Distancia entre el botón de pausa y el borde derecho de la pantalla
    private const float MargenDerechoPausa = 24f;
    private TableroMP _tablero;

    public override void _Ready()
    {
        if (GetNodeOrNull("UITemporal") == null)
        {
            GD.PrintErr("[EspectadorMP] Esta escena no tiene 'UITemporal': hay que usar la copia de jugadorMP.tscn.");
            return;
        }

        _tablero = GetParent<TableroMP>();

        // Lo que es propio de un jugador no existe para el espectador:
        // cartas de movimiento, figuras a formar, reroll, pasar turno y habilidad
        string[] propios =
        {
            "UITemporal/Mano",
            "UITemporal/BotonFinTurno",
            "UITemporal/BotonHabilidad",
            "UITemporal/PopupLuzMala"
        };
        foreach (string ruta in propios)
        {
            GetNodeOrNull(ruta)?.QueueFree();
        }

        Label info = InfoConexion.Agregar(GetNode("UITemporal"), this);
        info.Text = "ESPECTADOR  |  " + info.Text;

        var botonPausa = GetNodeOrNull<BaseButton>("UITemporal/BotonPausa");
        if (botonPausa != null)
        {
            botonPausa.Pressed += OnPausaPresionada;
        }

        _tablero.AccionAplicada += OnAccionAplicada;
        Controller.GetInstance().TurnoCambiado += OnTurnoCambiado;

        ActualizarInsignias();
        Callable.From(ColocarBotonPausa).CallDeferred();
        GetViewport().SizeChanged += ColocarBotonPausa;
    }

    public override void _ExitTree()
    {
        GetViewport().SizeChanged -= ColocarBotonPausa;
        if (_tablero != null)
        {
            _tablero.AccionAplicada -= OnAccionAplicada;
        }
        if (Controller._instance != null)
        {
            Controller.GetInstance().TurnoCambiado -= OnTurnoCambiado;
        }
    }

    private void OnPausaPresionada()
    {
        GetNodeOrNull<MenuPausa>("UITemporal/MenuPausa")?.TogglePausa();
    }

    // El botón de pausa es del tablero compartido: solo en esta pantalla se lleva hacia la derecha
    private void ColocarBotonPausa()
    {
        var boton = _tablero?.GetNodeOrNull<Control>("UITemporal/BotonPausa");
        if (boton == null)
        {
            GD.PrintErr("[EspectadorMP] No se encontró 'UITemporal/BotonPausa' en el tablero.");
            return;
        }

        Vector2 pantalla = GetViewport().GetVisibleRect().Size;
        float ancho = boton.Size.X * boton.Scale.X;
        boton.GlobalPosition = new Vector2(pantalla.X - ancho - MargenDerechoPausa, boton.GlobalPosition.Y);
    }

    // ---------- Insignias de todos los jugadores ----------

    private void ActualizarInsignias()
    {
        var contenedor = GetNodeOrNull<Control>("UITemporal/InsigniaViewContainer/InsigniasSecundarias");
        if (contenedor == null)
        {
            GD.PrintErr("[EspectadorMP] No se encontró 'UITemporal/InsigniaViewContainer/InsigniasSecundarias'.");
            return;
        }

        // Lugar entre insignias para que entre el nombre arriba de cada una
        if (contenedor is BoxContainer caja)
        {
            caja.AddThemeConstantOverride("separation", 44);
        }

        foreach (Node hijo in contenedor.GetChildren())
        {
            contenedor.RemoveChild(hijo);
            hijo.QueueFree();
        }

        Jugador[] jugadores = Controller.GetInstance().Jugadores();
        if (jugadores == null) return;

        for (int i = 0; i < jugadores.Length; i++)
        {
            if (jugadores[i].Eliminado) continue;

            var insignia = InsigniaViewScene.Instantiate<InsigniaView>();
            contenedor.AddChild(insignia);
            insignia.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
            insignia.SetInsigniaSecundariaJugador(
                jugadores[i],
                CargadorAssetsTablero.TexturaInsignia[jugadores[i].PersonajeAsignado.Tipo],
                i
            );

            PonerNombre(insignia, jugadores[i].nombre);
        }

        MarcarTurno(contenedor);
    }

    // El nombre va arriba de la insignia, con su propio label (el de la insignia queda tapado por las cartas)
    private async void PonerNombre(InsigniaView insignia, string nombre)
    {
        // Se espera un frame para que el contenedor termine de acomodar la insignia
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        if (!IsInstanceValid(insignia)) return;

        var etiqueta = new Label
        {
            Text = nombre.ToUpper(),
            MouseFilter = Control.MouseFilterEnum.Ignore
        };

        // Misma fuente que el resto de los textos: se copia del label de nombre de la propia insignia
        var original = insignia.GetNodeOrNull<Label>("NombreJugador");
        if (original != null && original.LabelSettings != null)
        {
            etiqueta.LabelSettings = (LabelSettings)original.LabelSettings.Duplicate();
        }
        else
        {
            if (original != null)
            {
                etiqueta.AddThemeFontOverride("font", original.GetThemeFont("font"));
                etiqueta.AddThemeFontSizeOverride("font_size", original.GetThemeFontSize("font_size"));
                etiqueta.AddThemeColorOverride("font_color", original.GetThemeColor("font_color"));
            }
            else
            {
                etiqueta.AddThemeFontSizeOverride("font_size", 22);
            }
            etiqueta.AddThemeColorOverride("font_outline_color", Colors.Black);
            etiqueta.AddThemeConstantOverride("outline_size", 6);
        }

        insignia.AddChild(etiqueta);

        var fondo = insignia.GetNodeOrNull<TextureRect>("Fondo");
        float x = fondo != null ? fondo.Position.X : 0f;
        float alto = etiqueta.GetCombinedMinimumSize().Y;
        float y = fondo != null ? fondo.Position.Y - alto : -alto;
        etiqueta.Position = new Vector2(x, y);
    }

    private void MarcarTurno(Control contenedor)
    {
        int actual = Controller.GetInstance().IndiceJugadorActual();
        foreach (Node hijo in contenedor.GetChildren())
        {
            if (hijo is InsigniaView insignia)
            {
                insignia.SetDecoracion(actual);
            }
        }
    }

    private void OnAccionAplicada(int indiceJugador)
    {
        ActualizarInsignias();
    }

    private async void OnTurnoCambiado(int jugadorActual)
    {
        ActualizarInsignias();

        if (TransicionScene != null)
        {
            var transicion = TransicionScene.Instantiate<PanelTransicion>();
            AddChild(transicion);

            string nombreProximoJugador = Controller.GetInstance().NombreJugadorActual();
            await transicion.ReproducirTransicionAsync(nombreProximoJugador, 1.5f);
        }
    }
}