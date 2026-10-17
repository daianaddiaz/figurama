using Godot;

public partial class MenuPausa : CanvasLayer
{
    [Export] private Button _botonVolver;
    [Export] private Button _botonMenuPrincipal;
    [Export] private ConfirmationDialog _dialogoConfirmacion;

    [Export(PropertyHint.File, "*.tscn")]
    private string _rutaMenuPrincipal = "res://Objetos/menuPrincipal.tscn";

    private bool _abierto = false;

    public override void _Ready()
    {
        Hide();

        _botonVolver.Pressed += OnVolverPresionado;
        _botonMenuPrincipal.Pressed += OnMenuPrincipalPresionado;
        _dialogoConfirmacion.Confirmed += OnConfirmarSalida;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_cancel"))
        {
            TogglePausa();
            GetViewport().SetInputAsHandled();
        }
    }

    public void TogglePausa()
    {
        _abierto = !_abierto;

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