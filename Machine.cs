using Godot;
using System;

using Game.Assembly;

public partial class Machine : Node
{
    [Export]
    public long clockPeriod = 0;
    private long clockTime = 0;
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
        Game.ExternDebug.printer = str => debugLog(str);
        machineCtor();
        // TODO(srp): this has to get fed
        string testProgram = @"

; comment

loop:
    cmpkey 0
    jmpf keyf press_0
    cmpkey 40
    jmpf keyf press_up
    cmpkey 41
    jmpf keyf press_down
    cmpkey 43
    jmpf keyf press_left
    cmpkey 42
    jmpf keyf press_right
    cmpkey 36
    jmpf keyf press_space
    jmp loop

press_0:
    rot 138
    print 0
    jmp loop

press_right:
    MOV_R 1
    print 1
    jmp loop

press_left:
    MOV_L 1
    print 2
    jmp loop

press_up:
    MOV_F 1
    print 3
    jmp loop

press_down:
    MOV_B 1
    print 4
    jmp loop

press_space:
    JUMP_UP 255
    print 5
    jmp loop

";
        compileProgram(testProgram);
        debugLogCode();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        if (clockPeriod == 0 || clockTime % clockPeriod == 0)
        {
            // 1k instructions per frame
            // since it's asynchronous this is totally fine lmao
            for (int i = 0; i < 1000; i++)
            {
                checkKeys();
                StepCode();
            }
        }

        clockTime++;
	}

    private void debugLog(string s){} //=> GD.Print(s);

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
        debugLog("stackptr is " + stackPtr);
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
        debugLog("stackptr is " + stackPtr);
        return stack[stackPtr];
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

    private void debugLogMem()
    {
        debugLog("");
        debugLog("REGS");
        string regstr = "";
        for (int i = 0; i < (int)Register.None; i++)
        {
            regstr += $"[{((Register)i).ToString()}: {registers[i].ToString("X2")}] ";
        }
        debugLog(regstr);

        debugLog("");
        debugLog("INSTR");
        debugLog("----");
        for (int y = 0; y < 16; y++)
        {
            string line = "";
            for (int x = 0; x < 16; x++)
            {
                line +=  (code.MethodArea[16*y + x].DisplayInt).ToString("X2") + " ";
            }
            debugLog(line);
        }
        debugLog("");
        debugLog("STACK");
        debugLog("----");
        for (int y = 0; y < 16; y++)
        {
            string line = "";
            for (int x = 0; x < 16; x++)
            {
                line +=  (stack[16*y + x]).ToString("X2") + " ";
            }
            debugLog(line);
        }
        debugLog("");
        debugLog("HEAP");
        debugLog("----");
        for (int y = 0; y < 16; y++)
        {
            string line = "";
            for (int x = 0; x < 16; x++)
            {
                line +=  (heap[16*y + x]).ToString("X2") + " ";
            }
            debugLog(line);
        }
        debugLog("");
    }
}
