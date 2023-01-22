using Godot;
using System;

using Game.Assembly;

public partial class Machine : Node
{
    public long clockPeriod = 0;
    private long clockTime = 0;
    string program;
    public int[] stack = new int[256];
    int stackPtr = 0;
    public int[] heap = new int[256];
    public int[] registers = new int[(int)(Register.None)];

    public bool gotGun = false;

    private void initMem()
    {
        for (int i = 0; i < 256; i++)
        {
            stack[i] = 0;
            heap[i] = 0;
        }

        for (int i = 0; i < registers.Length; i++)
        {
            registers[i] = 0;
        }

        stackPtr = 0;
    }

    private void machineCtor()
    {
        initMem();
        initInstructions();
        initParser(); // Must happen after initInstructions()
        initDocs(); // Must happen after initInstructions()
    }

    [Signal]
    public delegate void CodeLogEventHandler(string txt);
    private void codeLog(string str)
    {
        EmitSignal("CodeLog", str);
    }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        machineCtor();
        // TODO(srp): this has to get fed
	}

    public bool paused = false;
    public bool runnable = false;
    int lastScore = 0;
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        if (availableInstructionSets.Contains(InstructionSets.Available.Final))
            gotGun = true;

        if (lastScore != ScoreThing.collected)
        {
            switch (ScoreThing.collected)
            {
                case 1:
                    AddInstructionSet(InstructionSets.Available.BasicMovement);
                    break;
                case 2:
                    AddInstructionSet(InstructionSets.Available.Control);
                    break;
                case 3:
                    AddInstructionSet(InstructionSets.Available.Decision);
                    break;
                case 4:
                    AddInstructionSet(InstructionSets.Available.Math);
                    break;
                case 5:
                    AddInstructionSet(InstructionSets.Available.AdvancedDecision);
                    break;
                case 6:
                    AddInstructionSet(InstructionSets.Available.Debug);
                    break;
                case 7:
                    AddInstructionSet(InstructionSets.Available.Memory);
                    break;
                case 8:
                    AddInstructionSet(InstructionSets.Available.MidMovement);
                    break;
                case 9:
                    AddInstructionSet(InstructionSets.Available.AdvancedMovement);
                    break;
                case 10:
                    AddInstructionSet(InstructionSets.Available.Input);
                    break;
                case 11:
                    AddInstructionSet(InstructionSets.Available.Final);
                    break;
            }
        }
        lastScore = ScoreThing.collected;

        if (!runnable)
            return;

        if (clockPeriod == 0 || clockTime % clockPeriod == 0)
        {
            // 1k instructions per frame
            // since it's asynchronous this is totally fine lmao
            for (int i = 0; i < (clockPeriod == 0 ? 1000 : 1); i++)
            {
                checkKeys();
                StepCode();
            }
        }

        clockTime++;
	}


    private void stackPush(int x, out string err)
    {
        if (stackPtr == 256)
        {
            err = "[PROBLEM]: Stack overflow!";
            return;
        }

        err = "";
        stack[stackPtr] = x;
        registers[(int)Register.SP] = stackPtr;
        stackPtr++;
    }

    private int stackPop(out string err)
    {
        if (stackPtr == 0)
        {
            err = "[PROBLEM]: Stack underflow!";
            return -1;
        }

        err = "";
        registers[(int)Register.SP] = --stackPtr;
        return stack[stackPtr];
    }

    
}
