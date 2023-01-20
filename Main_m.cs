using Godot;
using System;
using GodotPlugins.Game;

public partial class Main_m : Control
{
	// Loaded scenes
	private PackedScene game;
	
	// Loaded buttons?
	private Button Play;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		game = GD.Load<PackedScene>("res://scenes/prefabs/sections.tscn");
		
		Play = GetNode<Button>("Container/Play");
		Play.Pressed += PlayOnPressed;
	}
	private void PlayOnPressed()
	{
		GetTree().ChangeSceneToPacked(game);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
