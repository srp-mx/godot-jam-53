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

    private Machine machine;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Game = GD.Load<PackedScene>("res://scenes/levels/test/TestMovement.tscn");

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
        sections_layer.Visible = true;
        start = true;
	}

    bool start = false;
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        if (!start)
            return;

        if (machine is null)
        {
            var node = GetNode("TestMovement/Machine"); 
            machine = node is null ? null : (Machine)node;
            return;
        }

        if (menu_layer.Visible || terminal_layer.Visible)
        {
            machine.paused = true;
        }
        else
        {
            machine.paused = false;
        }
	}
}
