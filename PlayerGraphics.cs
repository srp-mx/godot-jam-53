using Godot;
using System;

public partial class PlayerGraphics : MeshInstance3D
{
	[Export]
	float amplitude = 2.0f;

	[Export]
	float freq = 0.5f;

	Node3D parent;

	public override void _Ready()
	{
		parent = GetParent<Node3D>();
	}

	double t = 0.0f;
	public override void _Process(double delta)
	{
		Position = new Vector3(Position.x, amplitude*Mathf.Sin((float)t*freq), Position.z);
		t += delta;
	}
}
