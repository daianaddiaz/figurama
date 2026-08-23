using Godot;
using System;
using System.Collections.Generic;

public partial class StateMachine : Node
{

	[Export] public NodePath InitialState;

	private Dictionary<string, State> _states = new Dictionary<string, State>();
	private State _currentState;
	
	public override void _Ready()
	{
		
		foreach (Node child in GetChildren())
		{
			if (child is State state)
			{
				state.StateMachine = this;
				_states[state.Name] = state;
				state.OnReady();
				state.OnExit();
			}
		}

		if (InitialState != null && _states.TryGetValue(InitialState, out State initialState))
		{
			_currentState = initialState;
			_currentState.OnEnter();
		}

	}

	public override void _Process(double delta)
	{
		if (_currentState != null)
		{
			_currentState.OnUpdate(delta);
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_currentState != null)
		{
			_currentState.OnPhysicsUpdate(delta);
		}
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (_currentState != null)
		{
			_currentState.OnInput(@event);
		}
	}

	public void ChangeState(string newStateName)
	{
		if (newStateName == _currentState?.Name)
		{
			return;
		}

		if (_states.TryGetValue(newStateName, out State newState))
		{
			if (_currentState != null)
			{
				_currentState.OnExit();
			}

			_currentState = newState;
			_currentState.OnEnter();
		}
		else
		{
			GD.PrintErr($"State '{newStateName}' not found in the state machine.");
		}
	}

}
