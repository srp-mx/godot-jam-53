using Godot;
/**
 * NOTA: Esto solo funciona adecuadamente si se trabaja
 * desde la escena "main_m" la del botón de play,
 * si se quiere correr desde algún otro lado (ya sea de manera
 * "individual" o dentro de la 3D del juego) se requiere cambiar
 * las rutas ya especificadas para los nodos.
 */
public partial class Sections : Control
{
	// Loaded buttons?
	private Button terminal;
	private Button menu;
	
	// Loaded nodes? (Canvas Layers)
	private CanvasLayer terminal_layer;
	private CanvasLayer sections_layer;
	private CanvasLayer menu_layer;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		terminal_layer = GetNode<CanvasLayer>("/root/Main_M Node/TestMovement/TerminalLayer");
		sections_layer = GetNode<CanvasLayer>("/root/Main_M Node/TestMovement/SectionsLayer");
		menu_layer = GetNode<CanvasLayer>("/root/Main_M Node/TestMovement/MenuLayer");
		
		//Terminal button
		terminal = GetNode<Button>("/root/Main_M Node/TestMovement/SectionsLayer/Control/Terminal");
		Callable self = new Callable(this, "Show_Terminal");
		terminal.Connect("toggled", self, 0);
		
		//Menu button
		menu = GetNode<Button>("/root/Main_M Node/TestMovement/SectionsLayer/Control/Menu");
		Callable self2 = new Callable(this, "Show_Menu");
		menu.Connect("toggled", self2, 0);
		
	}
	
	// Shows the Terminal
	public void Show_Terminal(bool condition)
	{
		if (condition)
		{
			terminal_layer.Visible = true;
		}
		else
		{
			terminal_layer.Visible = false;
		}
	}

	// Shows the pause menu?
	public void Show_Menu(bool condition)
	{
		if (condition)
		{
			menu_layer.Visible = true;
			//GetTree().Paused = true;
			//terminal.Disabled = true;
		}
		else
		{
			//GetTree().Paused = false;
			//terminal.Disabled = false;
            menu_layer.Visible = false;
		}
	}
	
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

}
