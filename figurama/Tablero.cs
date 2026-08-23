using Godot;
using System;

public partial class Tablero : Node3D
{
    [Export] public PackedScene FichaScene;
    private const int Filas = 6;
    private const int Columnas = 6;
    private const float SizeCelda = 1.0f;

    private Ficha[,] _grilla = new Ficha[Filas, Columnas];

    private CartaMovimiento _movimiento = new MovimientoLateralContiguo();
    private bool _hayFichaSeleccionada = false;
    private int _filaSeleccionada;
    private int _columnaSeleccionada;

    public override void _Ready()
    {
        Color[] colores = { Colors.Red, Colors.Blue, Colors.Yellow, Colors.Green };

        for (int fila = 0; fila < Filas; fila++)
        {
            for (int columna = 0; columna < Columnas; columna++)
            {
                Ficha ficha = FichaScene.Instantiate<Ficha>();
                AddChild(ficha);
                ficha.SetearColor(colores[GD.Randi() % colores.Length]);
                ficha.Clickeada += OnFichaClickeada;
                ColocarFicha(ficha, fila, columna);
            }
        }
    }

     private void OnFichaClickeada(Ficha ficha)
    {
        if (!_hayFichaSeleccionada)
        {
            _filaSeleccionada = ficha.Fila;
            _columnaSeleccionada = ficha.Columna;
            _hayFichaSeleccionada = true;
            return;
        }

        if (_movimiento.EsValido(this, _filaSeleccionada, _columnaSeleccionada, ficha.Fila, ficha.Columna))
        {
            _movimiento.Ejecutar(this, _filaSeleccionada, _columnaSeleccionada, ficha.Fila, ficha.Columna);
        }

        _hayFichaSeleccionada = false;
    }

    public void ColocarFicha(Ficha ficha, int fila, int columna)
    {
        _grilla[fila, columna] = ficha;
        ficha.Fila = fila;
        ficha.Columna = columna;
        ficha.Position = new Vector3(columna * SizeCelda, 0, fila * SizeCelda);
    }

     public Ficha ObtenerFicha(int fila, int columna)
    {
        return _grilla[fila, columna];
    }

    public void IntercambiarFichas(int filaA, int columnaA, int filaB, int columnaB)
    {
        Ficha fichaA = ObtenerFicha(filaA, columnaA);
        Ficha fichaB = ObtenerFicha(filaB, columnaB);

        ColocarFicha(fichaA, filaB, columnaB);
        ColocarFicha(fichaB, filaA, columnaA);
    }


}