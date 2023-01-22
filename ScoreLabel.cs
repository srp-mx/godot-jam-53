using Godot;
using System;

public partial class ScoreLabel : Label
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Text = $"Collected {ScoreThing.collected}/{ScoreThing.total}. Right-click to change mouse mode and click on stuff from there! Keep on the lookout for new instruction sets!";
		if (ScoreThing.collected == ScoreThing.total)
			Text = "You Win! Congratulations!";
	}
}
