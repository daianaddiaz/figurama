using Godot;
using System;

public partial class State : Node
{

	public StateMachine StateMachine { get; set; }

	public virtual void OnEnter() { }
	public virtual void OnExit() { }
	public virtual void OnReady() {}
	public virtual void OnUpdate(double delta) { }
	public virtual void OnPhysicsUpdate(double delta) { }
	public virtual void OnInput(InputEvent @event) { }
}
