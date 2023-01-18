using Godot;
public partial class Sections : Control
{
    // Loaded scenes
    PackedScene Shell = GD.Load<PackedScene>("res://shell.tscn");
	
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        //Shell button
        Button shell = GetNode<Button>("Shell");
        Callable self = new Callable(this, "Show_Shell");
        shell.Connect("toggled", self, 0);
		
        //Main menu button
        Button main_menu = GetNode<Button>("Main Menu");
        Callable self2 = new Callable(this, "Show_Main_Menu");
        //not_shell.Connect("toggled", self2, 0);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
	
    // Shows the shell
    public void Show_Shell(bool condition)
    {
        if (condition == true)
        {
            Control shell = (Control)Shell.Instantiate();
            Vector2i position = new Vector2i(-50, 0);
            shell.Position = position;
            this.AddChild(shell);
        }
        else
        {
            Control shell = GetNode<Control>("Shell Node");
            shell.QueueFree();
        }
    }
}