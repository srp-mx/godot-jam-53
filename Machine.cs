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
    callf keyf press_0

    cmpkey 9
    callf keyf press_9

    cmpkey 40
    callf keyf press_up

    cmpkey 41
    callf keyf press_down

    cmpkey 43
    callf keyf press_left

    cmpkey 42
    callf keyf press_right

    cmpkey 36
    callf keyf press_space

    cmpkey 0xC
    callf keyf press_c

    cmpkey 0xE
    callf keyf press_e

    cmpkey 0xF
    callf keyf press_f

    cmpkey 37
    callf keyf press_enter

    cmpkey 27
    callf keyf press_r

    jmp loop

press_0:
    rot_clock 10
    ret

press_9:
    rot_anti 10 
    ret

press_right:
    MOV_R 1
    ret

press_left:
    MOV_L 1
    ret

press_up:
    MOV_F 1
    ret

press_down:
    MOV_B 1
    ret

press_space:
    JUMP_UP 255
    ret

press_c:
    FLY_DOWN 1
    ret

press_e:
    FLY_UP 1
    ret

press_f:
    FALL
    ret

press_enter:
    SHOOT
    ret

press_r:
    RELOAD
    ret

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
