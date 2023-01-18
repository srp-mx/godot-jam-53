using Godot;
using System;

public partial class PostProcessColors : ColorRect
{
    [Export]
    Color my_color1;
    [Export]
    Color my_color2;
    [Export]
    Color my_color3;
    [Export]
    Color my_color4;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        (Material as ShaderMaterial).SetShaderParameter("my_color1", my_color1 );
        (Material as ShaderMaterial).SetShaderParameter("my_color2", my_color2 );
        (Material as ShaderMaterial).SetShaderParameter("my_color3", my_color3 );
        (Material as ShaderMaterial).SetShaderParameter("my_color4", my_color4 );
	}
}
