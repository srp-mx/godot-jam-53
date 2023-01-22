using Godot;
using System;
using NoiseTest;

public partial class ScoreThing : CSGPolygon3D
{
	float startEnergy;
	OpenSimplexNoise n;
	public static int collected = 0;
	public static int total = 0;
	Node3D player;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GetNode("/root/Main_M Node/TestMovement/Machine").Connect("doINTERACT", new(this, "Collect"));
		player = GetNode<Node3D>("/root/Main_M Node/TestMovement/Machine/CharacterBody3D");
		n = new OpenSimplexNoise();
		total++;
	}

	bool lightOn = true;
	float t = 0.0f;
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		t += (float)delta;
		if(!lightOn)
			return;
		
		Rotation = new Vector3(t, 0, t);
		
	}

	public void Collect()
	{
		if(!lightOn) return;
		if ((player.Position - Position).Length() > 3.0f) return;
		lightOn = false;
		Visible = false;
		collected++;
	}
}
