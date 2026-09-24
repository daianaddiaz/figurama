using Godot;

public partial class CartaDeFiguraView : TextureRect
{
    private FiguraAsignada _carta;

    public void SetCarta(FiguraAsignada carta)
    {
        _carta = carta;

        if (_carta != null && _carta.Figura != null)
        {
            if (!string.IsNullOrEmpty(_carta.Figura.RutaImagen))
            {
                var textura = GD.Load<Texture2D>(_carta.Figura.RutaImagen);
                if (textura != null)
                {
                    Texture = textura; // Setea directamente la textura del nodo raíz
                }
                else
                {
                    GD.PrintErr($"[CartaDeFiguraView] No se pudo cargar la textura desde: {_carta.Figura.RutaImagen}");
                }
            }
        }

        Actualizar();
    }

    public void Actualizar()
    {
        CambiarColorSegunEstado();
    }

    private void CambiarColorSegunEstado()
    {
        if (_carta != null && _carta.Completada)
        {
            SelfModulate = Colors.Green; // Tiñe la textura de verde al completarse
        }
        else
        {
            SelfModulate = Colors.White; // Color normal
        }
    }
}