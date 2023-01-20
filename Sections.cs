using Godot;
public partial class Sections : Control
{
    // Loaded scenes
    PackedScene Terminal;
    PackedScene Menu;
    
    // Escenas instanciadas
    private Control menu_ins;
    private Control terminal_ins;
    
    // Loaded buttons?
    private Button terminal;
    private Button menu;
	
    private Vector2i position;
    
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Terminal = GD.Load<PackedScene>("res://scenes/prefabs/terminal.tscn");
        Menu = GD.Load<PackedScene>("res://scenes/prefabs/menu.tscn");

        position = new Vector2i(0, 0);

        //Terminal button
        terminal = GetNode<Button>("Terminal");
        Callable self = new Callable(this, "Show_Terminal");
        terminal.Connect("toggled", self, 0);
		
        //Menu button
        menu = GetNode<Button>("Menu");
        Callable self2 = new Callable(this, "Show_Menu");
        menu.Connect("toggled", self2, 0);
        
        // Scenes instances
        terminal_ins = (Control) Terminal.Instantiate();
        menu_ins = (Control) Menu.Instantiate();
        terminal_ins.Visible = false;
        menu_ins.Visible = false;
        this.AddChild(terminal_ins);
        this.AddChild(menu_ins);

        terminal_ins = (Control) Terminal.Instantiate();
        menu_ins = (Control) Menu.Instantiate();

        terminal_ins.Visible = false;
        menu_ins.Visible = false;

        AddChild(terminal_ins);
        AddChild(menu_ins);
        
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
	
    // Shows the Terminal
    public void Show_Terminal(bool condition)
    {
        if (condition)
        {
            terminal_ins.Position = position;
            terminal_ins.Visible = true;
        }
        else
        {
            terminal_ins.Visible = false;
        }
    }

    // Shows the pause menu?
    public void Show_Menu(bool condition)
    {
        if (condition)
        {
            menu_ins.Visible = true;
            GetTree().Paused = true;
            menu_ins.Position = position;
            terminal.Disabled = true;
        }
        else
        {
            GetTree().Paused = false;
            terminal.Disabled = false;
            menu_ins.Visible = false;
        }
    }
}
