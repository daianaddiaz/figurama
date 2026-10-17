using Godot;

public partial class PartidaMPPrueba : Control
{
    public override void _Ready()
    {
        int indice = DatosPartida.IndiceJugadorLocal;
        var label = GetNode<Label>("Contenedor/LabelInfo");

        label.Text = $"Soy el jugador {indice + 1} de {DatosPartida.NombresJugadores.Count}: " +
                     $"{DatosPartida.NombresJugadores[indice]} ({DatosPartida.PersonajesElegidos[indice]})\n" +
                     $"Todos: {string.Join(", ", DatosPartida.NombresJugadores)}";

        GetNode<Button>("Contenedor/BotonVolver").Pressed += Volver;
    }

    private void Volver()
    {
        if (Multiplayer.MultiplayerPeer != null)
        {
            Multiplayer.MultiplayerPeer.Close();
        }
        Multiplayer.MultiplayerPeer = new OfflineMultiplayerPeer();
        GetTree().ChangeSceneToFile("res://Objetos/menuPrincipal.tscn");
    }
}