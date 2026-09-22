using Godot;

public partial class FichaBloqueada : State
{
	public override void OnEnter()
	{
		var luz = GetParent<StateMachine>().GetParent<Node3D>().GetNode<SpotLight3D>("SpotLight3D");
		luz.Visible = true;
		luz.LightColor = new Color(0.6f, 0.1f, 0.1f); // rojo oscuro, para diferenciarla de "Disponible"/"Completada"
	}

	public override void OnExit()
	{
		var luz = GetParent<StateMachine>().GetParent<Node3D>().GetNode<SpotLight3D>("SpotLight3D");
		luz.LightColor = Colors.White;
	}
}