using Godot;
using System;
using System.IO;
using FileAccess = Godot.FileAccess;
using Game.Assembly;

/**
 * NOTA: Esto solo funciona adecuadamente si se trabaja
 * desde la escena "main_m" la del botón de play,
 * si se quiere correr desde algún otro lado (ya sea de manera
 * "individual" o dentro de la 3D del juego) es probable que 
 * se requiera cambiar las rutas ya especificadas para los nodos.
 */

public partial class Terminal : Control
{
	// Loaded buttons?
	private MenuButton menuFile;
	private Button compile;
	private Button run;
	
	// Loaded nodes?
	private FileDialog popupOpen;
	private FileDialog popupSave;
	private TextEdit editor;
	private Machine machine;

	private Vector2i size;

    private RichTextLabel err;

    private Label[] stack = new Label[256];
    private Label[] heap = new Label[256];
    private Label[] method = new Label[256];
    private Label[] registers = new Label[(int)Register.None];
    private Slider speed;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		machine = (Machine)GetNode("/root/Main_M Node/TestMovement/Machine");
		//Pop-ups?
		popupOpen = GetNode<FileDialog>("/root/Main_M Node/TestMovement/TerminalLayer/Control/Open File");
		popupSave = GetNode<FileDialog>("/root/Main_M Node/TestMovement/TerminalLayer/Control/Save As File");
		size = new Vector2i(800, 500);
		
		editor = GetNode<TextEdit>("/root/Main_M Node/TestMovement/TerminalLayer/Control/TextEditor");

        err = GetNode<RichTextLabel>("RichTextLabel");

		//Menu button
		menuFile = GetNode<MenuButton>("/root/Main_M Node/TestMovement/TerminalLayer/Control/Menu File");
		menuFile.GetPopup().AddItem("New File");
		menuFile.GetPopup().AddItem("Open File");
		menuFile.GetPopup().AddItem("Save As File");
		Callable self = new Callable(this, "_on_item_pressed");
		menuFile.GetPopup().Connect("id_pressed", self, 0);
		
		//Compile Button
		compile = GetNode<Button>("/root/Main_M Node/TestMovement/TerminalLayer/Control/Compile");
		compile.Pressed += on_compile_pressed;

        machine.Connect("CodeLog", new(this, "writeToLog"));

		//Run button??****
		run = GetNode<Button>("/root/Main_M Node/TestMovement/TerminalLayer/Control/Run");
		run.Pressed += on_run_pressed;


        for (int i = 0; i < 256; i++)
        {
            stack[i] = GetNode<Label>($"PanelContainer3/Memory/Stack/{i+1}");
            heap[i] = GetNode<Label>($"PanelContainer3/Memory/Heap/{i+1}");
            method[i] = GetNode<Label>($"PanelContainer3/Memory/Program/{i+1}");
        }

        for (int i = 0; i < (int)Register.None; i++)
        {
            registers[i] = GetNode<Label>($"#Registers/{((Register)i).ToString()}2");
        }

        speed = GetNode<Slider>("SpeedSlider");
	}

	private void _on_item_pressed(int id)
	{
		var itemName = menuFile.GetPopup().GetItemText(id);
		if (itemName == "Open File")
		{
			popupOpen.PopupCentered(size);
			popupOpen.FileSelected += PopupOpenOnFileSelected;
		}
		else if(itemName == "Save As File")
		{
			popupSave.PopupCentered(size);
			popupSave.FileSelected += PopupSaveOnFileSelected;
		}
		else if (itemName == "New File")
		{
			editor.Text = "";
		}
	}

	private void PopupOpenOnFileSelected(string path)
	{
		var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
		editor.Text = file.GetAsText();
	}
	private void PopupSaveOnFileSelected(string path)
	{
		var file = FileAccess.Open(path, FileAccess.ModeFlags.Write);
		string content = editor.Text;
		file.StoreString(content);
		file.Flush();
		editor.Text = "";
	}

    private void writeToLog(string txt)
    {
        err.Text += $"\n{txt}";
    }

	private void on_compile_pressed()
	{
		string code = editor.Text;
        err.Text = "";
        machine.runnable = false;
        machine.compileProgram(code);
		//machine1.compileProgram(code);
       // El coso de arribita está mal, lanza un error/excepción [/home/arletpb/jam/godot-jam-53/
      // Terminal.cs(83,12): 'Node' does not contain a 

      //*****[ni idea de pq dice que linea (83,12) según yo eso no es pero ok]******

      //definition for 'compileProgram' and no accessible extension method 'compileProgram' 
      //accepting a first argument of type 'Node' could be found (are you missing a using directive or an assembly reference?)]
	}

    private void on_run_pressed()
    {
        machine.runnable = true;
    }


    int lastIP = 0;
    int ip = 0;
    int lastSP = 0;
    int sp = 0;
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        for (int i = 0; i < 256; i++)
        {
            stack[i].Text = machine.stack[i].ToString("X2");
            heap[i].Text = machine.heap[i].ToString("X2");
            method[i].Text = machine.code.MethodArea[i].DisplayInt.ToString("X2");
        }

        if (machine.registers[(int)Register.IP] != ip)
            lastIP = ip;
        ip = machine.registers[(int)Register.IP]; 

        if (machine.registers[(int)Register.SP] != sp)
            lastSP = sp;
        sp = machine.registers[(int)Register.SP]; 

        method[lastIP].RemoveThemeColorOverride("font_color");
        method[ip].AddThemeColorOverride("font_color", new Color("#D5ECC2CC"));

        stack[lastSP].RemoveThemeColorOverride("font_color");
        stack[sp].AddThemeColorOverride("font_color", new Color("#FFD3B4CC"));

        for (int i = 0; i < (int)Register.None; i++)
        {
            registers[i].Text = machine.registers[i].ToString("X2");
        }

        machine.clockPeriod = (5L - ((long)speed.Value)) * 2L;
	}
}
