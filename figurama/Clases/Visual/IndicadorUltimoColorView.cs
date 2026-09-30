using Godot;

public partial class IndicadorUltimoColorView : Control
{
    private TextureRect _rectRojo;
    private TextureRect _rectAzul;
    private TextureRect _rectAmarillo;
    private TextureRect _rectVerde;

    public override void _Ready()
    {
        _rectRojo = GetNode<TextureRect>("ultimoRojo");
        _rectAzul = GetNode<TextureRect>("ultimoAzul");
        _rectAmarillo = GetNode<TextureRect>("ultimoAmarillo");
        _rectVerde = GetNode<TextureRect>("ultimoVerde");

        ApagarTodos();
        Controller.GetInstance().UltimoColorCambiado += ActualizarColor;
    }

    public override void _ExitTree()
    {
        if (Controller._instance != null)
        {
            Controller.GetInstance().UltimoColorCambiado -= ActualizarColor;
        }
    }

    private void ActualizarColor(ColorFicha nuevoColor)
    {
        ApagarTodos();
        switch (nuevoColor)
        {
            case ColorFicha.Rojo: _rectRojo.Visible = true; break;
            case ColorFicha.Azul: _rectAzul.Visible = true; break;
            case ColorFicha.Amarillo: _rectAmarillo.Visible = true; break;
            case ColorFicha.Verde: _rectVerde.Visible = true; break;
        }
    }

    private void ApagarTodos()
    {
        if (_rectRojo != null) _rectRojo.Visible = false;
        if (_rectAzul != null) _rectAzul.Visible = false;
        if (_rectAmarillo != null) _rectAmarillo.Visible = false;
        if (_rectVerde != null) _rectVerde.Visible = false;
    }
}