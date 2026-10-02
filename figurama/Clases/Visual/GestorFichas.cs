using Godot;
using System.Collections.Generic;

public class GestorFichas
{
    private const int CantidadColores = 9;

    public List<Ficha> CrearFichas(PackedScene fichaScene, Node3D padre, TableroReglas reglas, float sizeCelda)
    {
        var fichasCreadas = new List<Ficha>();

        Color[] colores = { Colors.MediumVioletRed, Colors.Chartreuse, Colors.RoyalBlue, Colors.Yellow };
        ColorFicha[] coloresLogicos = { ColorFicha.Rojo, ColorFicha.Verde, ColorFicha.Azul, ColorFicha.Amarillo };
        int[] contadorColores = { CantidadColores, CantidadColores, CantidadColores, CantidadColores };

        for (int fila = 0; fila < TableroReglas.Filas; fila++)
        {
            for (int columna = 0; columna < TableroReglas.Columnas; columna++)
            {
                Color colorElegido = colores[GD.Randi() % colores.Length];
                colorElegido = VerificarCantidadDeFichas(colorElegido, colores, contadorColores);
                int colorIndice = System.Array.IndexOf(colores, colorElegido);
                Color colorSecundario = colores[(colorIndice + 1) % colores.Length];

                Ficha nodoFicha = fichaScene.Instantiate<Ficha>();
                padre.AddChild(nodoFicha);
                nodoFicha.SetearColor(colorElegido, colorSecundario, colorIndice);

                var datos = new FichaData();
                datos.Color = coloresLogicos[colorIndice];
                nodoFicha.Datos = datos;
                reglas.ColocarFicha(datos, fila, columna);
                nodoFicha.Position = new Vector3(columna * sizeCelda, 0, fila * sizeCelda);

                fichasCreadas.Add(nodoFicha);
            }
        }

        return fichasCreadas;
    }

    private Color VerificarCantidadDeFichas(Color color, Color[] colores, int[] contadorColores)
    {
        int colorIndice = System.Array.IndexOf(colores, color);
        if (contadorColores[colorIndice] > 0)
        {
            contadorColores[colorIndice]--;
            return color;
        }
        else
        {
            Color nuevoColor;
            do
            {
                nuevoColor = colores[GD.Randi() % colores.Length];
                colorIndice = System.Array.IndexOf(colores, nuevoColor);
            } while (contadorColores[colorIndice] <= 0);

            contadorColores[colorIndice]--;
            color = nuevoColor;
            return color;
        }
    }
}