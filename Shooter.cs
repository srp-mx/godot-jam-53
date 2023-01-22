using Godot;
using System;

public partial class Shooter : MeshInstance3D
{
	Machine machine;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		machine = GetParent().GetParent<Machine>();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Visible = machine.gotGun;
	}
}
