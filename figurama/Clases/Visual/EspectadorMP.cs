using Godot;
using System.Text;

public partial class EspectadorMP : Node3D
{
    // Misma vista cenital que usan los jugadores al elegir una carta
    [Export] public Vector3 PosicionCamara = new Vector3(2.0f, 5.5f, 3.0f);
    [Export] public Vector3 RotacionCamara = new Vector3(-89.5f, 0.0f, 0.0f);

    private TableroMP _tablero;
    private Label _labelEncabezado;
    private Label _labelTurno;
    private Label _labelJugadores;

    public override void _Ready()
    {
        _tablero = GetParent<TableroMP>();

        var camara = new Camera3D
        {
            Position = PosicionCamara,
            RotationDegrees = RotacionCamara
        };
        AddChild(camara);
        camara.MakeCurrent();

        CrearInterfaz();

        _tablero.AccionAplicada += OnAccionAplicada;
        Controller.GetInstance().TurnoCambiado += OnTurnoCambiado;

        Actualizar();
    }

    public override void _ExitTree()
    {
        if (_tablero != null)
        {
            _tablero.AccionAplicada -= OnAccionAplicada;
        }
        if (Controller._instance != null)
        {
            Controller.GetInstance().TurnoCambiado -= OnTurnoCambiado;
        }
    }

    // ---------- Interfaz ----------

    private void CrearInterfaz()
    {
        var capa = new CanvasLayer();
        AddChild(capa);

        var contenedor = new VBoxContainer { Position = new Vector2(24, 20) };
        capa.AddChild(contenedor);

        _labelEncabezado = CrearLabel(contenedor, 24);
        _labelTurno = CrearLabel(contenedor, 22);
        _labelJugadores = CrearLabel(contenedor, 20);

        var botonSalir = new Button { Text = "Salir de la partida" };
        botonSalir.Pressed += Salir;
        contenedor.AddChild(botonSalir);
    }

    private Label CrearLabel(Control padre, int tamano)
    {
        var label = new Label();
        label.AddThemeFontSizeOverride("font_size", tamano);
        label.AddThemeColorOverride("font_outline_color", Colors.Black);
        label.AddThemeConstantOverride("outline_size", 6);
        padre.AddChild(label);
        return label;
    }

    private void Actualizar()
    {
        var controller = Controller.GetInstance();
        Jugador[] jugadores = controller.Jugadores();
        if (jugadores == null || jugadores.Length == 0) return;

        _labelEncabezado.Text = $"ESPECTADOR  |  ID {Multiplayer.GetUniqueId()}  |  {DatosPartida.NombreSala}";

        int actual = controller.JugadorActual;
        _labelTurno.Text = controller.JuegoTerminado
            ? "La partida terminó"
            : $"Turno de: Jugador {actual + 1}: {jugadores[actual].nombre}";

        var texto = new StringBuilder();
        for (int i = 0; i < jugadores.Length; i++)
        {
            Jugador j = jugadores[i];
            int completadas = j.figurasAArmar.FindAll(f => f.Completada).Count;
            int total = j.figurasAArmar.Count + j.figurasEnReserva.Count;
            var pendientes = j.figurasAArmar.FindAll(f => !f.Completada).ConvertAll(f => f.Figura.Nombre);

            string marca = (i == actual && !controller.JuegoTerminado && !j.Eliminado) ? "► " : "    ";
            string estado = j.Eliminado ? "  [ELIMINADO]" : "";

            texto.AppendLine($"{marca}Jugador {i + 1}: {j.nombre} ({j.PersonajeAsignado.Tipo})  -  figuras {completadas}/{total}{estado}");
            if (!j.Eliminado)
            {
                texto.AppendLine($"        Pendientes: {(pendientes.Count > 0 ? string.Join(", ", pendientes) : "ninguna")}");
            }
        }
        _labelJugadores.Text = texto.ToString();
    }

    private void OnAccionAplicada(int indiceJugador)
    {
        Actualizar();
    }

    private void OnTurnoCambiado(int jugadorActual)
    {
        Actualizar();
    }

    private void Salir()
    {
        if (Multiplayer.MultiplayerPeer != null)
        {
            Multiplayer.MultiplayerPeer.Close();
        }
        Multiplayer.MultiplayerPeer = new OfflineMultiplayerPeer();
        GetTree().ChangeSceneToFile("res://Objetos/menuPrincipal.tscn");
    }
}