using Godot;
using System;
/**
 * NOTA: Esto solo funciona adecuadamente si se trabaja
 * desde la escena "main_m" la del botón de play,
 * si se quiere correr desde algún otro lado (ya sea de manera
 * "individual" o dentro de la 3D del juego) es probable que 
 * se requiera cambiar las rutas ya especificadas para los nodos.
 */

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
	private Button menu_button;
	
	// Loaded nodes? (Canvas Layers)
	private CanvasLayer terminal_layer;
	private CanvasLayer sections_layer;
	private CanvasLayer menu_layer;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		//PackedScene para el main menu
		Main_menu = GD.Load<PackedScene>("res://scenes/prefabs/main_m.tscn");
		//PackedScene para el retry
		Game = GD.Load<PackedScene>("res://scenes/levels/test/TestMovement.tscn");
		
		//las caspas para hacer visibles o invisibles 
		terminal_layer = GetNode<CanvasLayer>("/root/Main_M Node/TestMovement/TerminalLayer");
		sections_layer = GetNode<CanvasLayer>("/root/Main_M Node/TestMovement/SectionsLayer");
		menu_layer = GetNode<CanvasLayer>("/root/Main_M Node/TestMovement/MenuLayer");
		
		//los botones de las secciones para activar y desactivar
		terminal_button = GetNode<Button>("/root/Main_M Node/TestMovement/SectionsLayer/Control/Terminal");
		menu_button = GetNode<Button>("/root/Main_M Node/TestMovement/SectionsLayer/Control/Menu");
		
		//Resume button
		Resume = GetNode<Button>("/root/Main_M Node/TestMovement/MenuLayer/Control/VBoxContainer/Resume");
		Resume.Pressed += ResumeOnPressed;
		
		//Retry button
		Retry = GetNode<Button>("/root/Main_M Node/TestMovement/MenuLayer/Control/VBoxContainer/Retry");
		Retry.Pressed += RetryOnPressed;
		
		//Main menu button
		Main_m = GetNode<Button>("/root/Main_M Node/TestMovement/MenuLayer/Control/VBoxContainer/Main_Menu");
		Main_m.Pressed += Main_mOnPressed;

        sections_layer.Visible = true;
	}

	private void ResumeOnPressed()
	{
		terminal_button.Disabled = false;
		GetTree().Paused = false;
		menu_layer.Visible = false;
        menu_button.SetPressed(false);
	}

	private void RetryOnPressed() //HAY ALGO MAL AQUÍ
	{
		GetTree().ChangeSceneToPacked(Game);
		menu_layer.Visible = false;
		terminal_layer.Visible = false;
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
