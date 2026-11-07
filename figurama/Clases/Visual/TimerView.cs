using Godot;
using System;

public partial class TimerView : Control
{
    [Export] private Label labelTiempo;
    [Export] private Label labelAlerta;

    public override void _Ready()
    {
        if (labelTiempo == null)
        {
            labelTiempo = GetNodeOrNull<Label>("LabelTimer");
        }

        if (labelAlerta == null)
        {
            labelAlerta = GetNodeOrNull<Label>("LabelAlerta");
        }

        Controller controller = Controller.GetInstance();

        controller.TemporizadorActualizado += OnTemporizadorActualizado;
        controller.TiempoAgotado += OnTiempoAgotado;
        controller.TurnoCambiado += OnTurnoCambiado;

        if (labelAlerta != null)
        {
            labelAlerta.Visible = false;
        }
    }

    public override void _Process(double delta)
    {
        var controller = Controller.GetInstance();

        if (controller == null || controller.Jugadores() == null || controller.Jugadores().Length == 0) return;

        if (!DatosPartida.EsMultijugador || Multiplayer.IsServer())
        {
            // Modo local o servidor: la lógica del reloj corre acá
            controller.ActualizarTiempo((float)delta);
        }
        else
        {
            // Cliente: solo muestra el tiempo (el servidor lo corrige cada segundo)
            controller.DescontarTiempoVisual((float)delta);
        }

        if (labelTiempo != null)
        {
            int minutos = (int)controller.TiempoRestante / 60;
            int segundos = (int)controller.TiempoRestante % 60;
            labelTiempo.Text = $"{minutos:D2}:{segundos:D2}";
        }
    }

    private void OnTemporizadorActualizado(float tiempoRestante)
    {
        ActualizarTexto(tiempoRestante);
    }

    private void ActualizarTexto(float tiempoRestante)
    {
        if (labelTiempo != null)
        {
            int minutos = (int)tiempoRestante / 60;
            int segundos = (int)tiempoRestante % 60;
            labelTiempo.Text = $"{minutos:D2}:{segundos:D2}";
        }
    }

    private void OnTiempoAgotado()
    {
        if (labelAlerta != null)
        {
            labelAlerta.Text = "¡SE ACABÓ EL TIEMPO!";
            labelAlerta.Visible = true;
        }
    }

    private void OnTurnoCambiado(int nuevoJugador)
    {
        if (labelAlerta != null)
        {
            labelAlerta.Visible = false;
        }
    }

    public override void _ExitTree()
    {
        if (Controller._instance != null)
        {
            Controller controller = Controller.GetInstance();
            controller.TemporizadorActualizado -= OnTemporizadorActualizado;
            controller.TiempoAgotado -= OnTiempoAgotado;
            controller.TurnoCambiado -= OnTurnoCambiado;
        }
    }
}