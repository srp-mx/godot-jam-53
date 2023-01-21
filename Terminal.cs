using Godot;
using System;
using System.IO;
using FileAccess = Godot.FileAccess;

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
	private Node machine1;

	private Vector2i size;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		machine1 = GetNode<Node>("/root/Main_M Node/TestMovement/Machine");
		//Pop-ups?
		popupOpen = GetNode<FileDialog>("/root/Main_M Node/TestMovement/TerminalLayer/Control/Open File");
		popupSave = GetNode<FileDialog>("/root/Main_M Node/TestMovement/TerminalLayer/Control/Save As File");
		size = new Vector2i(800, 500);
		
		editor = GetNode<TextEdit>("/root/Main_M Node/TestMovement/TerminalLayer/Control/TextEditor");

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

		//Run button??****
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

	private void on_compile_pressed()
	{
		string code = editor.Text;
		//machine1.compileProgram(code);
       // El coso de arribita está mal, lanza un error/excepción [/home/arletpb/jam/godot-jam-53/
      // Terminal.cs(83,12): 'Node' does not contain a 

      //*****[ni idea de pq dice que linea (83,12) según yo eso no es pero ok]******

      //definition for 'compileProgram' and no accessible extension method 'compileProgram' 
      //accepting a first argument of type 'Node' could be found (are you missing a using directive or an assembly reference?)]
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
