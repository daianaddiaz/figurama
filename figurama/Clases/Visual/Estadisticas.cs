using Godot;
using System.Text;

public static class Estadisticas
{
    // Una línea por jugador: nombre y cantidad de figuras formadas
    public static string Construir()
    {
        Jugador[] jugadores = Controller.GetInstance().Jugadores();
        if (jugadores == null) return "";

        var texto = new StringBuilder();
        for (int i = 0; i < jugadores.Length; i++)
        {
            int formadas = jugadores[i].figurasAArmar.FindAll(f => f.Completada).Count;
            texto.Append($"{jugadores[i].nombre}: {formadas} {(formadas == 1 ? "figura" : "figuras")}");
            if (i < jugadores.Length - 1) texto.AppendLine();
        }
        return texto.ToString();
    }

    public static void Mostrar(Label label)
    {
        if (label == null)
        {
            GD.PrintErr("[Estadisticas] No se encontró el nodo 'LabelEstadisticas'.");
            return;
        }

        label.Text = Construir();
        label.Visible = true;
    }
}