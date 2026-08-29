using Godot;

public partial class MenuInicio : Control
{
    public override void _Ready()
    {
        GetNode<Button>("VBoxContainer/BotonJugar").Pressed += OnJugarPresionado;
    }

    private void OnJugarPresionado()
    {
        DatosPartida.NombresJugadores.Clear();
        DatosPartida.NombresJugadores.Add(ObtenerNombre("VBoxContainer/NombreJugador1", "Jugador 1"));
        DatosPartida.NombresJugadores.Add(ObtenerNombre("VBoxContainer/NombreJugador2", "Jugador 2"));
        DatosPartida.NombresJugadores.Add(ObtenerNombre("VBoxContainer/NombreJugador3", "Jugador 3"));
        DatosPartida.NombresJugadores.Add(ObtenerNombre("VBoxContainer/NombreJugador4", "Jugador 4"));

        GetTree().ChangeSceneToFile("res://Objetos/tablero.tscn");
    }
    
    private string ObtenerNombre(string ruta, string nombrePorDefecto)
    {
        string texto = GetNode<LineEdit>(ruta).Text;
        return string.IsNullOrWhiteSpace(texto) ? nombrePorDefecto : texto;
    }
}