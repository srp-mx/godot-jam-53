using Godot;
using System;
using System.IO;
using FileAccess = Godot.FileAccess;

public partial class Editor : Control
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		//Menu button
		MenuButton menuFile = GetNode<MenuButton>("Menu File");
		menuFile.GetPopup().AddItem("Open File");
		menuFile.GetPopup().AddItem("Save As File");
		Callable self = new Callable(this, "_on_item_pressed");
		menuFile.GetPopup().Connect("id_pressed", self, 0);
	}

	private void _on_item_pressed(int id)
	{
		MenuButton menuFile = GetNode<MenuButton>("Menu File");
		var itemName = menuFile.GetPopup().GetItemText(id);
		if (itemName == "Open File")
		{
			FileDialog popupOpen = GetNode<FileDialog>("Open File");
			Vector2i size = new Vector2i(800, 500);
			popupOpen.PopupCentered(size);
			popupOpen.FileSelected += PopupOpenOnFileSelected;
		}
		else if(itemName == "Save As File")
		{
			FileDialog popupSave = GetNode<FileDialog>("Save As File");
			Vector2i size = new Vector2i(800, 500);
			popupSave.PopupCentered(size);
			popupSave.FileSelected += PopupSaveOnFileSelected;
		}
	}

	
	private void PopupOpenOnFileSelected(string path)
	{
		var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
		TextEdit editor = GetNode<TextEdit>("TextEditor");
		editor.Text = file.GetAsText();
	}
	private void PopupSaveOnFileSelected(string path)
	{
		var file = FileAccess.Open(path, FileAccess.ModeFlags.Write);
		TextEdit editor = GetNode<TextEdit>("TextEditor");
		string content = editor.Text;
		file.StoreString(content);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
