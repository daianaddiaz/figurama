
using Godot;
using System.Collections.Generic;

public partial class MenuPrincipal : Control
{
    // Paneles de navegación
    private VBoxContainer _panelPrincipal;
    private VBoxContainer _contadorJugadores;
    private VBoxContainer _panelNombres;
    private VBoxContainer _panelDificultad;
    private VBoxContainer _inputContainer;

    // Botones
    private Button _playButton;
    private Button _optionsButton;
    private Button _dificultadButton;
    private Button _tutorialButton;
    private Button _exitButton;

    private Button _facilBtn;    
    private Button _medioBtn;   
    private Button _dificilBtn; 
    private Button _backButtonDificultad; 

    private Button _players2Btn;
    private Button _players3Btn;
    private Button _players4Btn;
    private Button _backButtonCount;

    private Button _startGameBtn;
    private Button _backButtonNames;

    private Label _labelDificultadActual;


    // Datos del juego
    private int _selectedPlayerCount = 2;
    private List<LineEdit> _nameInputs = new List<LineEdit>();

    public override void _Ready()
    {
        // Referencias a los paneles
        _panelPrincipal = GetNode<VBoxContainer>("PanelPrincipal");
        _contadorJugadores = GetNode<VBoxContainer>("ContadorJugadores");
        _panelDificultad = GetNode<VBoxContainer>("PanelDificultad");
        _panelNombres = GetNode<VBoxContainer>("PanelNombres");
        _inputContainer = GetNode<VBoxContainer>("PanelNombres/InputContainer");
        

        // Referencias a botones
        _playButton = GetNode<Button>("PanelPrincipal/PlayButton");
        _dificultadButton = GetNode<Button>("PanelPrincipal/DificultadButton");
        _optionsButton = GetNode<Button>("PanelPrincipal/OptionButton");
        _tutorialButton = GetNode<Button>("PanelPrincipal/TutorialButton");
        _exitButton = GetNode<Button>("PanelPrincipal/ExitButton");

        _facilBtn = GetNode<Button>("PanelDificultad/FacilButton");
        _medioBtn = GetNode<Button>("PanelDificultad/MedioButton");
        _dificilBtn = GetNode<Button>("PanelDificultad/DificilButton");
        _backButtonDificultad = GetNode<Button>("PanelDificultad/BackButtonDificultad");

        _players2Btn = GetNode<Button>("ContadorJugadores/Opcion2JugButton");
        _players3Btn = GetNode<Button>("ContadorJugadores/Opcion3JugButton");
        _players4Btn = GetNode<Button>("ContadorJugadores/Opcion4JugButton");
        _backButtonCount = GetNode<Button>("ContadorJugadores/BackButtonCount");

        _startGameBtn = GetNode<Button>("PanelNombres/StartGameButton");
        _backButtonNames = GetNode<Button>("PanelNombres/BackButtonNames");


        // Conectar eventos
        _playButton.Pressed += OnPlayButtonPressed;
        _optionsButton.Pressed += OnOptionsButtonPressed;
        _tutorialButton.Pressed += OnTutorialButtonPressed;
        _exitButton.Pressed += OnExitButtonPressed;
        _dificultadButton.Pressed += () => ShowPanel(_panelDificultad);

        _facilBtn.Pressed += () => OnDificultadSelected(4);
        _medioBtn.Pressed += () => OnDificultadSelected(3);
        _dificilBtn.Pressed += () => OnDificultadSelected(2);
        _backButtonDificultad.Pressed += () => ShowPanel(_panelPrincipal);

        _players2Btn.Pressed += () => OnPlayerCountSelected(2);
        _players3Btn.Pressed += () => OnPlayerCountSelected(3);
        _players4Btn.Pressed += () => OnPlayerCountSelected(4);
        _backButtonCount.Pressed += () => ShowPanel(_panelPrincipal);

        _startGameBtn.Pressed += OnStartGamePressed;
        _backButtonNames.Pressed += () => ShowPanel(_contadorJugadores);

        // Estado inicial
        ShowPanel(_panelPrincipal);
    }

    private void ShowPanel(VBoxContainer panelToShow)
    {
        _panelPrincipal.Visible = (panelToShow == _panelPrincipal);
        _contadorJugadores.Visible = (panelToShow == _contadorJugadores);
        _panelNombres.Visible = (panelToShow == _panelNombres);
        _panelDificultad.Visible = (panelToShow == _panelDificultad);
    }


    private void OnPlayButtonPressed()
    {
        ShowPanel(_contadorJugadores);
    }

    private void OnOptionsButtonPressed()
    {
        // Queda vacío por el momento para agregar opciones más adelante
        GD.Print("Configuración seleccionada (vacío por ahora)");
    }

    private void OnTutorialButtonPressed()
    {
        GetTree().ChangeSceneToFile("res://Objetos/tutorial.tscn");
    }

    private void OnExitButtonPressed()
    {
        GetTree().Quit();
    }

    private void OnPlayerCountSelected(int count)
    {
        _selectedPlayerCount = count;
        GenerateNameInputs(count);
        ShowPanel(_panelNombres);
    }

    private void GenerateNameInputs(int count)
    {
        foreach (Node child in _inputContainer.GetChildren())
        {
            child.QueueFree();
        }
        _nameInputs.Clear();

        // Entrada para los nombres
        for (int i = 0; i < count; i++)
        {
            LineEdit input = new LineEdit
            {
                PlaceholderText = $"Nombre Jugador {i + 1}",
                Text = $"Jugador {i + 1}" 
            };
            _inputContainer.AddChild(input);
            _nameInputs.Add(input);
        }
    }

    private void OnStartGamePressed()
    {
        List<string> playerNames = new List<string>();
        
        foreach (LineEdit input in _nameInputs)
        {
            string name = string.IsNullOrWhiteSpace(input.Text) ? input.PlaceholderText : input.Text;
            playerNames.Add(name);
        }

        // Guardar los nombres en un Autoload/Singleton global antes de cambiar de escena
        Controller.GetInstance().NombresJugadores = playerNames;
        Controller.GetInstance().InicializarJugadores();

        // Cambiar a la escena principal del juego
        GetTree().ChangeSceneToFile("res://Objetos/tablero.tscn");
    }

    private void OnDificultadSelected(int cantidadCartas)
    {
    Controller controller = Controller.GetInstance();
    if (controller != null)
    {
        controller.CantidadCartasMovimiento = cantidadCartas;
    }

    string textoDificultad = cantidadCartas switch
    {
        4 => "Fácil (4 cartas)",
        3 => "Medio (3 cartas)",
        2 => "Difícil (2 cartas)",
        _ => "Medio"
    };

    if (_labelDificultadActual != null)
    {
        _labelDificultadActual.Text = $"Modo: {textoDificultad}";
    }

    GD.Print($"Dificultad configurada en: {textoDificultad}");

    ShowPanel(_contadorJugadores);
}
}
