using Godot;
using System;

public partial class Menu : Control
{
	// Loaded buttons?
	private Button Resume;
	private Button Retry;
	private Button Main_m;

	//Loaded scenes
	private PackedScene Main_menu;
	private PackedScene Game;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Game = GD.Load<PackedScene>("res://sections.tscn");
		//Resume button
		Resume = GetNode<Button>("VBoxContainer/Resume");
		Resume.Pressed += ResumeOnPressed;
		
		//Retry button
		Retry = GetNode<Button>("VBoxContainer/Retry");
		Retry.Pressed += RetryOnPressed;
		
		//Main menu button
		Main_menu = GD.Load<PackedScene>("res://main_m.tscn");
		Main_m = GetNode<Button>("VBoxContainer/Main_Menu");
		Main_m.Pressed += Main_mOnPressed;
	}

	private void RetryOnPressed()
	{
		GetTree().ChangeSceneToPacked(Game);
		GetTree().Paused = false;
	}

	private void ResumeOnPressed()
	{
		Button terminal_button = GetNode<Button>("/root/Sections Node/Terminal");
		terminal_button.Disabled = false;
		GetTree().Paused = false;
		Button menu_button = GetNode<Button>("/root/Sections Node/Menu");
		// Falta hacer que menu_button se presione para que cuando se vuelva a presionar salga de nuevo el menu
		this.QueueFree();
	}

	private void Main_mOnPressed()
	{
		GetTree().ChangeSceneToPacked(Main_menu);
		GetTree().Paused = false;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
