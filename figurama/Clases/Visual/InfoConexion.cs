using Godot;

public static class InfoConexion
{
    // Agrega un texto con el ID de red y la sala, arriba a la derecha de la interfaz
    public static void Agregar(Node interfaz, Node contexto)
    {
        var label = new Label
        {
            Text = $"ID {contexto.Multiplayer.GetUniqueId()}  |  {DatosPartida.NombreSala}",
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        label.AddThemeFontSizeOverride("font_size", 18);
        label.AddThemeColorOverride("font_outline_color", Colors.Black);
        label.AddThemeConstantOverride("outline_size", 5);

        interfaz.AddChild(label);
        label.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.TopRight);
        label.GrowHorizontal = Control.GrowDirection.Begin;
        label.OffsetTop = 8;
        label.OffsetRight = -16;
    }
}