using Godot;
using System;

public partial class Cinematica : VideoStreamPlayer
{
	private void _on_finished()
	{
		QueueFree();
	}
}
