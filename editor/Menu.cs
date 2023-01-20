using Godot;
using System;

public partial class Menu : Control
{
	// Loaded scenes
	private PackedScene Main_menu;
	private PackedScene Game;
	
	// Loaded buttons?
	private Button Resume;
	private Button Retry;
	private Button Main_m;
	private Button terminal_button;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Main_menu = GD.Load<PackedScene>("res://main_m.tscn");
		Game = GD.Load<PackedScene>("res://sections.tscn");
		
		terminal_button = GetNode<Button>("/root/Sections Node/Terminal");
		
		//Resume button
		Resume = GetNode<Button>("VBoxContainer/Resume");
		Resume.Pressed += ResumeOnPressed;
		
		//Retry button
		Retry = GetNode<Button>("VBoxContainer/Retry");
		Retry.Pressed += RetryOnPressed;
		
		//Main menu button
		Main_m = GetNode<Button>("VBoxContainer/Main_Menu");
		Main_m.Pressed += Main_mOnPressed;
	}

	private void ResumeOnPressed()
	{
		terminal_button.Disabled = false;
		GetTree().Paused = false;
		this.Visible = false;
	}

	private void RetryOnPressed()
	{
		GetTree().ChangeSceneToPacked(Game);
		GetTree().Paused = false;
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
