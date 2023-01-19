using Godot;
public partial class Sections : Control
{
    // Loaded scenes
    PackedScene Terminal;
    PackedScene Menu;
    
    // Loaded buttons?
    private Button terminal;
    private Button menu;
	
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Terminal = GD.Load<PackedScene>("res://terminal.tscn");
        Menu = GD.Load<PackedScene>("res://menu.tscn");
        
        //Shell button
        terminal = GetNode<Button>("Terminal");
        Callable self = new Callable(this, "Show_Terminal");
        terminal.Connect("toggled", self, 0);
		
        //Menu button
        menu = GetNode<Button>("Menu");
        Callable self2 = new Callable(this, "Show_Menu");
        menu.Connect("toggled", self2, 0);

    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
	
    // Shows the Terminal
    public void Show_Terminal(bool condition)
    {
        if (condition == true)
        {
            Control terminal_ins = (Control) Terminal.Instantiate();
            Vector2i position = new Vector2i(0, 0);
            terminal_ins.Position = position;
            this.AddChild(terminal_ins);
        }
        else
        {
            Control terminal_ins = GetNode<Control>("Terminal Node");
            terminal_ins.QueueFree();
        }
    }

    // Shows the pause menu?
    public void Show_Menu(bool condition)
    {
        if (condition == true)
        {
            GetTree().Paused = true;
            Control menu_ins = (Control) Menu.Instantiate();
            Vector2i position = new Vector2i(0, 0);
            menu_ins.Position = position;
            this.AddChild(menu_ins);
            terminal.Disabled = true;
        }
        else
        {
            GetTree().Paused = false;
            Control menu_ins = GetNode<Control>("Menu Node");
            if (menu_ins != null)
            {
                menu_ins.QueueFree();
            }
            terminal.Disabled = false;
        }
    }
}