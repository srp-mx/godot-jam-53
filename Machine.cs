using Godot;
using System;

using Game.Assembly;

public partial class Machine : Node
{
    string program;
    int[] stack = new int[256];
    int stackPtr = 0;
    int[] heap = new int[256];
    int[] registers;

    private void initMem()
    {
        for (int i = 0; i < 256; i++)
        {
            stack[i] = 0;
            heap[i] = 0;
        }
        registers = new int[(int)(Register.None)];
        stackPtr = 0;
    }

    private void machineCtor()
    {
        initMem();
        initInstructions();
        initParser(); // Must happen after initInstruction()
    }

    private void codeLog(string str)
    {
        // TODO(srp): show to player
        GD.Print(str);
        //throw new NotImplementedException("TODO Machine.cs  codeLog method");
    }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        machineCtor();
        // TODO(srp): this has to get fed
        string testProgram = @"; comment\nWAIT 100\nHLT";
        compileProgram(testProgram);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        StepCode();
	}
}
