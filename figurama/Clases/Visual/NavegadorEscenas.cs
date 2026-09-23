using Godot;

public static class NavegadorEscenas
{
    public static void CambiarEscenaConPaneo(SceneTree tree, string rutaNuevaEscena, Control escenaActual, float duracion = 0.5f, bool haciaIzquierda = false)
    {
        PackedScene proximaEscenaPacked = GD.Load<PackedScene>(rutaNuevaEscena);
        if (proximaEscenaPacked == null)
        {
            GD.PrintErr($"[NavegadorEscenas] No se pudo cargar la escena: {rutaNuevaEscena}");
            return;
        }

        Node proximaEscena = proximaEscenaPacked.Instantiate();
        Vector2 tamanoPantalla = escenaActual.GetViewportRect().Size;

        float offsetInicialX = haciaIzquierda ? -tamanoPantalla.X : tamanoPantalla.X;
        float offsetFinalActualX = haciaIzquierda ? tamanoPantalla.X : -tamanoPantalla.X;

        bool esAnimable = false;

        // Establecemos la posición inicial según el tipo de nodo 2D (Control o Node2D)
        if (proximaEscena is Control proximaControl)
        {
            proximaControl.Position = new Vector2(offsetInicialX, 0);
            esAnimable = true;
        }
        else if (proximaEscena is Node2D proximaNode2D)
        {
            proximaNode2D.Position = new Vector2(offsetInicialX, 0);
            esAnimable = true;
        }

        if (esAnimable)
        {
            tree.Root.AddChild(proximaEscena);

            Tween tween = escenaActual.CreateTween().SetParallel(true);

            // Mover la escena actual hacia afuera
            tween.TweenProperty(escenaActual, "position:x", offsetFinalActualX, duracion)
                 .SetTrans(Tween.TransitionType.Cubic)
                 .SetEase(Tween.EaseType.Out);

            // Mover la nueva escena hacia el centro
            tween.TweenProperty(proximaEscena, "position:x", 0.0f, duracion)
                 .SetTrans(Tween.TransitionType.Cubic)
                 .SetEase(Tween.EaseType.Out);

            // Al finalizar la animación, remover la escena vieja
            tween.Chain().TweenCallback(Callable.From(() =>
            {
                tree.CurrentScene = proximaEscena;
                escenaActual.QueueFree();
            }));
        }
        else
        {
            // Si no es un nodo 2D o Control, se cambia de forma estándar
            tree.Root.AddChild(proximaEscena);
            tree.CurrentScene = proximaEscena;
            escenaActual.QueueFree();
        }
    }
}