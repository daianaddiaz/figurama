using Godot;

public partial class MenuPausa : CanvasLayer
{
    [Export] private Button _botonVolver;
    [Export] private Button _botonMenuPrincipal;
    [Export] private ConfirmationDialog _dialogoConfirmacion;
    
    // Ruta a tu escena de menú principal
    [Export(PropertyHint.File, "*.tscn")] 
    private string _rutaMenuPrincipal = "res://Objetos/menuPrincipal.tscn";

    public override void _Ready()
    {
            // Aseguramos que empiece oculto
            Hide();

        // Conectar eventos de los botones
        _botonVolver.Pressed += OnVolverPresionado;
        _botonMenuPrincipal.Pressed += OnMenuPrincipalPresionado;
        _dialogoConfirmacion.Confirmed += OnConfirmarSalida;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        // Escuchar la tecla ESC / ui_cancel
        if (@event.IsActionPressed("ui_cancel"))
        {
            TogglePausa();
            GetViewport().SetInputAsHandled();
        }
    }

    public void TogglePausa()
    {
        bool nuevoEstadoPausa = !GetTree().Paused;
        GetTree().Paused = nuevoEstadoPausa;
        Visible = nuevoEstadoPausa;

        // Si se despausa, ocultamos también el diálogo de confirmación por si estaba abierto
        if (!nuevoEstadoPausa)
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
        // Abrimos el PopUp emergente de confirmación
        _dialogoConfirmacion.PopupCentered();
    }

    private void OnConfirmarSalida()
    {
        // Quitamos la pausa antes de cambiar de escena para evitar estados inconsistentes
        GetTree().Paused = false;
        GetTree().ChangeSceneToFile(_rutaMenuPrincipal);
    }
}