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
        GD.Print("CODELOG: " + str.Replace("\n", "\n         "));
        //throw new NotImplementedException("TODO Machine.cs  codeLog method");
    }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        Game.ExternDebug.printer = str => GD.Print(str);
        machineCtor();
        // TODO(srp): this has to get fed
        string testProgram = @"

; comment

label1:
    call label1

";
        compileProgram(testProgram);
        debugLogCode();
        Timer timer = this.GetNode<Timer>("CpuClock");
        timer.Connect("timeout", new Callable(this, "on_timeout"));
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

    private void debugLog(string s) => GD.Print(s);

    private void stackPush(int x, out string err)
    {
        if (stackPtr == 255)
        {
            err = "[PROBLEM]: Stack overflow!";
            return;
        }

        err = "";
        stack[stackPtr++] = x;
    }

    private int stackPop(out string err)
    {
        if (stackPtr == 0)
        {
            err = "[PROBLEM]: Stack underflow!";
            return -1;
        }

        err = "";
        return stack[--stackPtr];
    }

    private void debugLogCode()
    {
        debugLog("");
        debugLog("=== DEBUGGING COMPILED CODE ===");
        debugLog("");
        debugLog($"Is code null? {(code is null).ToString()}");
        debugLog("");
        debugLog("Display ints");
        for (int y = 0; y < 16; y++)
        {
            string line = "";
            for (int x = 0; x < 16; x++)
            {
                line +=  (code.MethodArea[16*y + x].DisplayInt).ToString("X2") + " ";
            }
            debugLog(line);
        }
    }
}
