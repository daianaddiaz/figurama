using Godot;

public partial class MenuPausa : CanvasLayer
{
    [Export] private Button _botonVolver;
    [Export] private Button _botonMenuPrincipal;
    [Export] private ConfirmationDialog _dialogoConfirmacion;

    // Ruta a tu escena de menú principal
    [Export(PropertyHint.File, "*.tscn")]
    private string _rutaMenuPrincipal = "res://Objetos/menuPrincipal.tscn";

    private Button _botonTutorial;
    private bool _abierto = false;

    public override void _Ready()
    {
        Hide();

        CrearBotonTutorial();

        _botonVolver.Pressed += OnVolverPresionado;
        _botonMenuPrincipal.Pressed += OnMenuPrincipalPresionado;
        _dialogoConfirmacion.Confirmed += OnConfirmarSalida;
    }

    // El botón es una copia de "Volver" (mismo estilo), puesta justo debajo
    private void CrearBotonTutorial()
    {
        Node contenedor = _botonVolver.GetParent();

        _botonTutorial = (Button)_botonVolver.Duplicate((int)(DuplicateFlags.Groups | DuplicateFlags.UseInstantiation));
        _botonTutorial.Name = "BotonTutorial";
        _botonTutorial.Text = "Tutorial";

        contenedor.AddChild(_botonTutorial);
        contenedor.MoveChild(_botonTutorial, _botonVolver.GetIndex() + 1);

        _botonTutorial.Pressed += OnTutorialPresionado;
    }

    private void OnTutorialPresionado()
    {
        Tutorial.AbrirSuperpuesto(GetTree());
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_cancel"))
        {
            TogglePausa();
            GetViewport().SetInputAsHandled();
        }
    }

    private ulong _ultimoCambio = ulong.MaxValue;

    public void TogglePausa()
    {
        // Si dos conexiones disparan el cambio en el mismo cuadro (un solo clic), se ignora la segunda
        ulong cuadro = Engine.GetProcessFrames();
        if (cuadro == _ultimoCambio) return;
        _ultimoCambio = cuadro;

        _abierto = !_abierto;
        GD.Print($"[Pausa] TogglePausa -> abierto={_abierto} (cuadro {cuadro})");

        // En multijugador no se pausa: el juego sigue corriendo para los demás
        if (!DatosPartida.EsMultijugador)
        {
            GetTree().Paused = _abierto;
        }
        Visible = _abierto;

        if (!_abierto)
        {
            _dialogoConfirmacion.Hide();
        }
    }

    private void OnVolverPresionado()
    {
        TogglePausa();
    }

    private void OnMenuPrincipalPresionado()
    {
        _dialogoConfirmacion.PopupCentered();
    }

    private void OnConfirmarSalida()
    {
        GetTree().Paused = false;

        if (DatosPartida.EsMultijugador)
        {
            if (Multiplayer.MultiplayerPeer != null)
            {
                Multiplayer.MultiplayerPeer.Close();
            }
            Multiplayer.MultiplayerPeer = new OfflineMultiplayerPeer();
        }

        GetTree().ChangeSceneToFile(_rutaMenuPrincipal);
    }
}