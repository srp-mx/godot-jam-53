using Godot;
using System;

public partial class Bolita : AnimatableBody3D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

		if (Input.IsKeyPressed(Key.D))
		{
			if (this.Position.x <= 4)
			{
				this.Position += new Vector3(0.1f, 0, 0);
			}
		}
		
		
		if (Input.IsKeyPressed(Key.S))
		{
			if (this.Position.z <= 4)
			{
				this.Position += new Vector3(0, 0, 0.1f);
			}
		}
		
		if (Input.IsKeyPressed(Key.A))
		{
			if (this.Position.x >= -4)
			{
				this.Position += new Vector3(-0.1f, 0, 0);
			}
		}
		
		if (Input.IsKeyPressed(Key.W))
		{
			if (this.Position.z >= -4)
			{
				this.Position += new Vector3(0, 0, -0.1f);
			}
		}
	}
}
