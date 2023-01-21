using Godot;
using System;
using GodotPlugins.Game;

public partial class Main_m : Control
{
	// Loaded scenes
	private PackedScene Game;
	
	//Instatiate Scenes
	private Node3D game_ins;
	
	// Loaded buttons?
	private Button Play;

	// Loaded nodes?
	private PanelContainer play_container;
	private CanvasLayer terminal_layer;
	private CanvasLayer sections_layer;
	private CanvasLayer menu_layer;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Game = GD.Load<PackedScene>("res://scenes/levels/test/TestMovement2.tscn");

		play_container = GetNode<PanelContainer>("Container");
		Play = GetNode<Button>("Container/Play");
		Play.Pressed += PlayOnPressed;
	}
	private void PlayOnPressed()
	{
		play_container.Visible = false;
		game_ins = (Node3D)Game.Instantiate();
		this.AddChild(game_ins);
		terminal_layer = GetNode<CanvasLayer>("TestMovement/TerminalLayer");
		sections_layer = GetNode<CanvasLayer>("TestMovement/SectionsLayer");
		menu_layer = GetNode<CanvasLayer>("TestMovement/MenuLayer");
		terminal_layer.Visible = false;
		menu_layer.Visible = false;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
