using Godot;
using System;
using System.IO;
using FileAccess = Godot.FileAccess;

public partial class Terminal : Control
{
	// Loaded buttons?
	private MenuButton menuFile;
	
	// Loaded nodes?
	private FileDialog popupOpen;
	private FileDialog popupSave;
	private TextEdit editor;

	private Vector2i size;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		//Pop-ups?
		popupOpen = GetNode<FileDialog>("Open File");
		popupSave = GetNode<FileDialog>("Save As File");
		size = new Vector2i(800, 500);
		
		editor = GetNode<TextEdit>("TextEditor");

		//Menu button
		menuFile = GetNode<MenuButton>("Menu File");
		menuFile.GetPopup().AddItem("New File");
		menuFile.GetPopup().AddItem("Open File");
		menuFile.GetPopup().AddItem("Save As File");
		Callable self = new Callable(this, "_on_item_pressed");
		menuFile.GetPopup().Connect("id_pressed", self, 0);
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

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
