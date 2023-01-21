using Godot;
using System;

using System.Threading;
using System.Threading.Tasks;

using Game.Assembly;

public partial class Machine : Node
{
    private static readonly string iOobError = "The instruction pointer went out of bounds!\n\tToo many instructions?";

    InstructionSets instructions = new();
    public BoolBox bbox = new();
    private void initInstructions()
    {
        availableInstructionSets.Add(InstructionSets.Available.Basic);


        // InstructionSpec(Name, DisplayInt, Params, Instruction)
        instructions.Add(new("HLT", 1, 0, HLT), InstructionSets.Available.Basic);
        instructions.Add(new("WAIT", 2, 1, WAIT), InstructionSets.Available.Basic);
        instructions.Add(new("RET", 3, 0, RET), InstructionSets.Available.Basic);
        instructions.Add(new("CALL", 4, 1, CALL), InstructionSets.Available.Basic);
        instructions.Add(new("PRINT", 5, 1, PRINT), InstructionSets.Available.Basic);
        instructions.Add(new("NOP", 6, 0, NOP), InstructionSets.Available.Basic);
        instructions.Add(new("MOV", 7, 2, MOV), InstructionSets.Available.Basic);
        instructions.Add(new("ADD", 8, 2, ADD), InstructionSets.Available.Basic);
        instructions.Add(new("NEG", 9, 1, NEG), InstructionSets.Available.Basic);
        instructions.Add(new("CMP", 10, 2, CMP), InstructionSets.Available.Basic);
        instructions.Add(new("JMPF", 11, 2, JMPF), InstructionSets.Available.Basic);
        instructions.Add(new("JMPNF", 12, 2, JMPNF), InstructionSets.Available.Basic);
        instructions.Add(new("INC", 13, 1, INC), InstructionSets.Available.Basic);
        instructions.Add(new("DEC", 14, 1, DEC), InstructionSets.Available.Basic);
        instructions.Add(new("JMP", 15, 1, JMP), InstructionSets.Available.Basic);
        instructions.Add(new("ROT_CLOCK", 16, 1, ROT_CLOCK), InstructionSets.Available.Basic);
        instructions.Add(new("ROT_ANTI", 17, 1, ROT_ANTI), InstructionSets.Available.Basic);
        instructions.Add(new("INTERACT", 18, 0, INTERACT), InstructionSets.Available.Basic);
        instructions.Add(new("MOV_R", 19, 1, MOV_R), InstructionSets.Available.Basic);
        instructions.Add(new("MOV_L", 20, 1, MOV_L), InstructionSets.Available.Basic);
        instructions.Add(new("MOV_F", 21, 1, MOV_F), InstructionSets.Available.Basic);
        instructions.Add(new("MOV_B", 22, 1, MOV_B), InstructionSets.Available.Basic);
        instructions.Add(new("JUMP_UP", 23, 1, JUMP_UP), InstructionSets.Available.Basic);
        instructions.Add(new("FLY_UP", 24, 1, FLY_UP), InstructionSets.Available.Basic);
        instructions.Add(new("FLY_DOWN", 25, 1, FLY_DOWN), InstructionSets.Available.Basic);
        instructions.Add(new("FALL", 26, 0, FALL), InstructionSets.Available.Basic);
        instructions.Add(new("CMPKEY", 27, 1, CMPKEY), InstructionSets.Available.Basic);
        instructions.Add(new("SHOOT", 28, 0, SHOOT), InstructionSets.Available.Basic);
        instructions.Add(new("RELOAD", 29, 0, RELOAD), InstructionSets.Available.Basic);
        instructions.Add(new("COOL", 30, 0, COOL), InstructionSets.Available.Basic);
        instructions.Add(new("PUSH", 31, 1, PUSH), InstructionSets.Available.Basic);
        instructions.Add(new("POP", 32, 1, POP), InstructionSets.Available.Basic);
        instructions.Add(new("CALLF", 33, 2, CALLF), InstructionSets.Available.Basic);
        instructions.Add(new("CALLNF", 34, 2, CALLNF), InstructionSets.Available.Basic);
    }

    private int getValueFromAddr(ParamInfo param, out string err)
    {
        // NOTE(srp): Param value can't be OOB because the parser 
        // checks that parameter values can't be OOB, and we 
        // shouldn't produce interactions such that they go OOB.
        switch (param.GetParamType())
        {
            case ParamInfo.ParamType.HeapAddress:
                err = "";
                return heap[param.Get()];
            case ParamInfo.ParamType.Register:
                err = "";
                return registers[param.Get()];
            case ParamInfo.ParamType.StackAddress:
                err = "";
                return stack[param.Get()];
            case ParamInfo.ParamType.MethodAreaAddress:
                err = "";
                return code.MethodArea[param.Get()].DisplayInt; 
            case ParamInfo.ParamType.RegHeapAddress:
                err = "";
                return heap[registers[param.Get()]];
            case ParamInfo.ParamType.RegStackAddress:
                err = "";
                return stack[registers[param.Get()]];
            case ParamInfo.ParamType.RegMethodAreaAddress:
                err = "";
                return code.MethodArea[registers[param.Get()]].DisplayInt; 
            default:
                err = "[PROBLEM]: Expected some memory address but we did not get that.";
                return -1;
        }
    }

    private bool errorParamBounds(int startPos, int paramCount, ref MethodBlock block, out string err)
    {
        if (startPos < 0 || startPos + paramCount > 255)
        {
            string pos = block.GetSourcePos();
            err = $"[ERROR] {pos}: {iOobError}";
            return true;
        }

        err = "";
        return false;
    }

    private bool moveOneExit(MethodBlock[] fmem, ref int iptr, out string err)
    {
        if (iptr >= 255)
        {
            err = $"[ERROR] {fmem[iptr].GetSourcePos()}: {iOobError}";
            return false;
        }

        err = "";
        iptr++;
        return true;
    }

    private bool userModifyRegister(Register r, int x, out string err)
    {
        if ((int)r > (int)Register.D)
        {
            err = "[PROBLEM]: Tried modifying non-modifiable register.";
                return false;
        }
        registers[(int)r] = x;
        err = "";
        return true;
    }

    private void userSetValueAtAddr(ParamInfo destParam, int x, out string err)
    {
        // NOTE(srp): Param value can't be OOB because the parser 
        // checks that parameter values can't be OOB, and we 
        // shouldn't produce interactions such that they go OOB.
        switch (destParam.GetParamType())
        {
            case ParamInfo.ParamType.HeapAddress:
                err = "";
                heap[destParam.Get()] = x;
                return;
            case ParamInfo.ParamType.Register:
                userModifyRegister((Register)destParam.Get(), x, out err);
                return;
            case ParamInfo.ParamType.StackAddress:
                err = "";
                stack[destParam.Get()] = x;
                return;
            case ParamInfo.ParamType.MethodAreaAddress:
                err = "";
                code.MethodArea[destParam.Get()].Clean(); 
                code.MethodArea[destParam.Get()].GetParamInfo().SetMiscValue(x); 
                return;
            case ParamInfo.ParamType.RegHeapAddress:
                err = "";
                heap[registers[destParam.Get()]] = x;
                return;
            case ParamInfo.ParamType.RegStackAddress:
                err = "";
                stack[registers[destParam.Get()]] = x;
                return;
            case ParamInfo.ParamType.RegMethodAreaAddress:
                err = "";
                code.MethodArea[registers[destParam.Get()]].Clean(); 
                code.MethodArea[registers[destParam.Get()]].GetParamInfo().SetMiscValue(x); 
                return;
            default:
                err = "[PROBLEM]: Expected some memory address but we did not get that.";
                return;
        }
    }

    private int getValue(ParamInfo param, out string err)
    {
        debugLog("getvalue internal " + param.Get());
        if (param.GetParamType() == ParamInfo.ParamType.Value)
        {
            err = "";
            return param.Get();
        }

        int result = getValueFromAddr(param, out err);

        if (err != "")
        {
            err = $"[PROBLEM] : Expected a value or an address to get values from.\n{err}";
            return -1;
        }

        return result;
    }

    private int addWithFlags(int a, int b)
    {
        a = a & 0xff;
        b = b & 0xff;
        int result = (a+b)&0xff;
        registers[(int)Register.CF] = (((a+b)&(~0xff))!=0) ? 1 : 0;
        registers[(int)Register.ZF] = result == 0 ? 1 : 0; 
        return result;
    }

    public void SetAmmoReg(int value)
    {
        registers[(int)Register.AMM] = value;
    }


    // INSTRUCTION METHODS BELOW

    // NOTE(srp): POLICY: Instructions should check their parameter's 
    // memory. Note that the "byte" (block) where they leave the iptr
    // on exit is checked by the IEnumerator.
    private bool HLT(MethodBlock[] fmem, ref int iptr, out string err)
    {
        debugLog("EXEC HLT");
        err = "";
        return false;
    }

    private bool WAIT(MethodBlock[] fmem, ref int iptr, out string err)
    {
        int timeMultiplier = 100; // tenths of a second

        if (errorParamBounds(iptr, 1, ref fmem[iptr], out err))
                return false;

        ParamInfo param1 = fmem[++iptr].GetParamInfo();
        debugLog("Got param at " + iptr);
        debugLog("Got param type " + param1.GetParamType().ToString());
        debugLog("Got param int of " + param1.Get());
        int time = getValue(param1, out err) * timeMultiplier;
        debugLog("Got time value of " + time);
        if (err == "")
        {
            debugLog("EXEC WAIT, waiting " + time + "ms");
            System.Threading.Thread.Sleep(time);
            debugLog("EXEC WAIT, finished");
            return moveOneExit(fmem, ref iptr, out err);
        }

        err = $"[ERROR] {fmem[iptr].GetSourcePos()}: We can only wait a constant or a value in a memory address. \n{err}";
        return false;
    }

    
    private bool CALL(MethodBlock[] fmem, ref int iptr, out string err)
    {
        stackPush(iptr + 2, out err);

        if (err != "")
        {
            err = $"[ERROR] {fmem[iptr].GetSourcePos()}: Failed to call instruction.\n{err}";
            return false;
        }

        if (errorParamBounds(iptr, 1, ref fmem[iptr], out err))
                return false;

        ParamInfo param1 = fmem[++iptr].GetParamInfo();
        
        if (param1.GetParamType() != ParamInfo.ParamType.Label)
        {
            err = $"[ERROR] {fmem[iptr].GetSourcePos()}: Call instruction expected a label.";
            return false;
        }

        iptr = param1.Get();

        debugLog("Called instruction at " + iptr);

        if (iptr > 255)
        {
            err = $"[ERROR] {fmem[255].GetSourcePos()}: The label pointed outside of the memory.";
            return false;
        }

        return true;
    }

    private bool RET(MethodBlock[] fmem, ref int iptr, out string err)
    {
        int retAddr = stackPop(out err);
        if (err != "")
        {
            err = $"[ERROR] {fmem[iptr].GetSourcePos()}: Return failed.\n{err}";
            return false;
        }

        debugLog("Popped " + retAddr + " from the stack");

        string retPos = fmem[iptr].GetSourcePos();
        iptr = retAddr;

        // NOTE(srp): the IEnumerator can handle errors from this if it goes too wrong

        if (iptr < 0 || iptr > 255)
        {
            err = $"[ERROR] {retPos}: Tried to return to a non-reachable address ({iptr}).";
            return false;
        }

        ParamInfo maybeParam = fmem[iptr].GetParamInfo();
        if (maybeParam.GetParamType() != ParamInfo.ParamType.None)
        {
            var instrQuery = instructions.Get(maybeParam.Get(), availableInstructionSets);
            var instr = instrQuery.Match(identity, ()=>null);

            if (instr is null)
            {
                err = $"[ERROR] {fmem[iptr].GetSourcePos()}: The value {maybeParam.Get()} does not correspond with any instruction.\n{instrQuery.GetErrLog()}";
                return false;
            }

            err = "";
            return true; 
        }

        err = "";
        return true;
    }

    private bool PRINT(MethodBlock[] fmem, ref int iptr, out string err)
    {
        if (errorParamBounds(iptr, 1, ref fmem[iptr], out err))
                return false;

        ParamInfo param1 = fmem[++iptr].GetParamInfo();
        if (param1.GetParamType() == ParamInfo.ParamType.None)
        {
            err = $"[ERROR] {fmem[iptr].GetSourcePos()}: Tried to print an instruction somehow..";
            return false;
        }

        int val = getValue(param1, out err);
        
        if (err != "")
        {
            val = param1.Get();
            err = "";
        }

        codeLog($"{val}");
        err = "";
        iptr++;
        return true;
    }

    private bool NOP(MethodBlock[] fmem, ref int iptr, out string err)
    {
        iptr++;
        err = "";
        return true;
    }

    private bool MOV(MethodBlock[] fmem, ref int iptr, out string err)
    {
        if (errorParamBounds(iptr, 2, ref fmem[iptr], out err))
                return false;

        iptr++;
        ParamInfo memVal = fmem[iptr].GetParamInfo();
        ParamInfo.ParamType memTyp = memVal.GetParamType();
        iptr++;
        ParamInfo assign = fmem[iptr].GetParamInfo();
        int assignValue = getValue(assign, out err);

        if (err != "")
        {
            err = $"[ERROR] {fmem[iptr].GetSourcePos()}: MOV's second parameter must be a value or an address we can get a value from.\n{err}";
            return false;
        }

        debugLog($"moving {assignValue} into {memVal.Get()}");
        userSetValueAtAddr(memVal, assignValue, out err);

        if (err != "")
            return false;

        return moveOneExit(fmem, ref iptr, out err);
    }

    private bool ADD(MethodBlock[] fmem, ref int iptr, out string err)
    {
        if (errorParamBounds(iptr, 2, ref fmem[iptr], out err))
                return false;

        ParamInfo param1 = fmem[++iptr].GetParamInfo();
        int param1val = getValueFromAddr(param1, out err);

        if (err != "")
        {
            err = $"[ERROR] {fmem[iptr].GetSourcePos()}: Couldn't perform ADD, check first parameter.\n{err}";
            return false;
        }

        ParamInfo param2 = fmem[++iptr].GetParamInfo();
        int param2val = getValue(param2, out err);

        if (err != "")
        {
            err = $"[ERROR] {fmem[iptr].GetSourcePos()}: Couldn't perform ADD, check second parameter.\n{err}";
            return false;
        }

        userSetValueAtAddr(param1, addWithFlags(param1val, param2val), out err);  

        if (err != "")
        {
            err = $"[ERROR] {fmem[iptr].GetSourcePos()}: Failed to perform addition.\n{err}";
                return false;
        }

        return moveOneExit(fmem, ref iptr, out err);
    }

    // gets two's complement
    private bool NEG(MethodBlock[] fmem, ref int iptr, out string err)
    {
        if (errorParamBounds(iptr, 1, ref fmem[iptr], out err))
                return false;

        ParamInfo param1 = fmem[++iptr].GetParamInfo();
        int x = getValueFromAddr(param1, out err);

        if (err != "")
        {
            err = $"[ERROR] {fmem[iptr].GetSourcePos()}: Failed to negate number at address.\n{err}";
            return false;
        }

        int negate = 0x100-x;

        userSetValueAtAddr(param1, negate, out err);

        if (err != "")
        {
            err = $"[ERROR] {fmem[iptr].GetSourcePos()}: Failed to set number to negated value.\n{err}";
            return false;
        }

        return moveOneExit(fmem, ref iptr, out err);
    }

    private bool CMP(MethodBlock[] fmem, ref int iptr, out string err)
    {
        if (errorParamBounds(iptr, 2, ref fmem[iptr], out err))
                return false;
        
        int p1 = getValue(fmem[++iptr].GetParamInfo(), out err);
        if (err != "")
        {
            err = $"[ERROR] {fmem[iptr].GetSourcePos()}: Can't get value of first parameter of CMP.\n{err}";
            return false;
        }

        int p2 = getValue(fmem[++iptr].GetParamInfo(), out err);
        if (err != "")
        {
            err = $"[ERROR] {fmem[iptr].GetSourcePos()}: Can't get value of second parameter of CMP.\n{err}";
            return false;
        }

        registers[(int)Register.EF] = (p1 == p2) ? 1 : 0;
        registers[(int)Register.LEQF] = (p1 <= p2) ? 1 : 0;
        registers[(int)Register.LEF] = (p1 < p2) ? 1 : 0;
        
        return moveOneExit(fmem, ref iptr, out err);
    }

    // jump if flag
    private bool JMPF(MethodBlock[] fmem, ref int iptr, out string err)
    {
        if (errorParamBounds(iptr, 2, ref fmem[iptr], out err))
                return false;

        // the flag
        ParamInfo param1 = fmem[++iptr].GetParamInfo();
        if (param1.GetParamType() != ParamInfo.ParamType.Register)
        {
            err = $"[ERROR] {fmem[iptr].GetSourcePos()}: JMPF instruction expected a register or flag as the first parameter.";
            return false;
        }
        
        ParamInfo param2 = fmem[++iptr].GetParamInfo();
        if (param2.GetParamType() != ParamInfo.ParamType.Label)
        {
            err = $"[ERROR] {fmem[iptr].GetSourcePos()}: JMPF instruction expected a label as the second parameter.";
            return false;
        }

        if (registers[param1.Get()] == 0)
        {
            return moveOneExit(fmem, ref iptr, out err);
        }

        iptr = param2.Get();

        if (iptr > 255)
        {
            err = $"[ERROR] {fmem[255].GetSourcePos()}: The label pointed outside of the memory.";
            return false;
        }

        return true;
    }

    // jump if not flag
    private bool JMPNF(MethodBlock[] fmem, ref int iptr, out string err)
    {
        if (errorParamBounds(iptr, 2, ref fmem[iptr], out err))
                return false;

        // the flag
        ParamInfo param1 = fmem[++iptr].GetParamInfo();
        if (param1.GetParamType() != ParamInfo.ParamType.Register)
        {
            err = $"[ERROR] {fmem[iptr].GetSourcePos()}: JMPNF instruction expected a register or flag as the first parameter.";
            return false;
        }
        
        ParamInfo param2 = fmem[++iptr].GetParamInfo();
        if (param2.GetParamType() != ParamInfo.ParamType.Label)
        {
            err = $"[ERROR] {fmem[iptr].GetSourcePos()}: JMPNF instruction expected a label as the second parameter.";
            return false;
        }

        if (registers[param1.Get()] != 0)
        {
            return moveOneExit(fmem, ref iptr, out err);
        }

        iptr = param2.Get();

        if (iptr > 255)
        {
            err = $"[ERROR] {fmem[255].GetSourcePos()}: The label pointed outside of the memory.";
            return false;
        }

        return true;
    }


    private bool INC(MethodBlock[] fmem, ref int iptr, out string err)
    {
        if (errorParamBounds(iptr, 1, ref fmem[iptr], out err))
                return false;

        var param1 = fmem[++iptr].GetParamInfo();
        int param1val = getValueFromAddr(param1, out err);

        if (err != "")
        {
            err = $"[ERROR] {fmem[iptr].GetSourcePos()}: Failed to get value from memory.\n{err}";
            return false;
        }

        int result = addWithFlags(param1val, 1);

        userSetValueAtAddr(param1, result, out err);

        if (err != "")
        {
            err = $"[ERROR] {fmem[iptr].GetSourcePos()}: Failed to insert increment in memory.\n{err}";
            return false;
        }

        return moveOneExit(fmem, ref iptr, out err);
    }

    private bool DEC(MethodBlock[] fmem, ref int iptr, out string err)
    {
        if (errorParamBounds(iptr, 1, ref fmem[iptr], out err))
                return false;

        var param1 = fmem[++iptr].GetParamInfo();
        int param1val = getValueFromAddr(param1, out err);

        if (err != "")
        {
            err = $"[ERROR] {fmem[iptr].GetSourcePos()}: Failed to get value from memory.\n{err}";
            return false;
        }

        int result = addWithFlags(param1val, 0xff);

        userSetValueAtAddr(param1, result, out err);

        if (err != "")
        {
            err = $"[ERROR] {fmem[iptr].GetSourcePos()}: Failed to insert increment in memory.\n{err}";
            return false;
        }

        return moveOneExit(fmem, ref iptr, out err);
    }

    private bool JMP(MethodBlock[] fmem, ref int iptr, out string err)
    {
        if (errorParamBounds(iptr, 1, ref fmem[iptr], out err))
                return false;
        
        ParamInfo param1 = fmem[++iptr].GetParamInfo();
        if (param1.GetParamType() != ParamInfo.ParamType.Label)
        {
            err = $"[ERROR] {fmem[iptr].GetSourcePos()}: JMP instruction expected a label as the only parameter.";
            return false;
        }

        iptr = param1.Get();

        if (iptr > 255)
        {
            err = $"[ERROR] {fmem[255].GetSourcePos()}: The label pointed outside of the memory.";
            return false;
        }

        return true;
    }

    [Signal]
    public delegate void doROTEventHandler(int amount);
    private bool ROT_CLOCK(MethodBlock[] fmem, ref int iptr, out string err)
    {
        if (errorParamBounds(iptr, 1, ref fmem[iptr], out err))
                return false;

        var param1 = fmem[++iptr].GetParamInfo();

        int val = getValue(param1, out err);
        if (err != "")
        {
            err = $"[ERROR] {fmem[iptr].GetSourcePos()}: Failed to rotate.\n{err}";
            return false;
        }

        bbox.Set(false);
        EmitSignal("doROT", -val);
        while (bbox.val == false)
        {
            
        }

        bbox.Set(false);
        return moveOneExit(fmem, ref iptr, out err);
    }

    private bool ROT_ANTI(MethodBlock[] fmem, ref int iptr, out string err)
    {
        if (errorParamBounds(iptr, 1, ref fmem[iptr], out err))
                return false;

        var param1 = fmem[++iptr].GetParamInfo();

        int val = getValue(param1, out err);
        if (err != "")
        {
            err = $"[ERROR] {fmem[iptr].GetSourcePos()}: Failed to rotate.\n{err}";
            return false;
        }

        bbox.Set(false);
        EmitSignal("doROT", val);
        while (bbox.val == false)
        {
            
        }

        bbox.Set(false);
        return moveOneExit(fmem, ref iptr, out err);
    }

    [Signal]
    public delegate void doINTERACTEventHandler();
    private bool INTERACT(MethodBlock[] fmem, ref int iptr, out string err)
    {
        if (errorParamBounds(iptr, 0, ref fmem[iptr], out err))
                return false;

        EmitSignal("doINTERACT");
        return moveOneExit(fmem, ref iptr, out err);
    }

    [Signal]
    public delegate void doMOV_EventHandler(int amount, Vector2 dir);
    private bool MOV_R(MethodBlock[] fmem, ref int iptr, out string err)
    {
        if (errorParamBounds(iptr, 1, ref fmem[iptr], out err))
                return false;

        var param1 = fmem[++iptr].GetParamInfo();

        int val = getValue(param1, out err);

        if (err != "")
        {
            err = $"[ERROR] {fmem[iptr].GetSourcePos()}: Could not get value to move to the right.\n{err}";
            return false;
        }

        bbox.Set(false);
        EmitSignal("doMOV_", val, new Vector2(-1, 0));
        while (bbox.val == false)
        {
            
        }

        bbox.Set(false);
        return moveOneExit(fmem, ref iptr, out err);
    }

    private bool MOV_L(MethodBlock[] fmem, ref int iptr, out string err)
    {
        if (errorParamBounds(iptr, 1, ref fmem[iptr], out err))
                return false;

        var param1 = fmem[++iptr].GetParamInfo();

        int val = getValue(param1, out err);

        if (err != "")
        {
            err = $"[ERROR] {fmem[iptr].GetSourcePos()}: Could not get value to move to the left.\n{err}";
            return false;
        }

        bbox.Set(false);
        EmitSignal("doMOV_", val, new Vector2(1, 0));
        while (bbox.val == false)
        {
            
        }

        bbox.Set(false);
        return moveOneExit(fmem, ref iptr, out err);
    }

    private bool MOV_F(MethodBlock[] fmem, ref int iptr, out string err)
    {
        if (errorParamBounds(iptr, 1, ref fmem[iptr], out err))
                return false;

        var param1 = fmem[++iptr].GetParamInfo();

        int val = getValue(param1, out err);

        if (err != "")
        {
            err = $"[ERROR] {fmem[iptr].GetSourcePos()}: Could not get value to move forward.\n{err}";
            return false;
        }

        bbox.Set(false);
        EmitSignal("doMOV_", val, new Vector2(0, 1));
        while (bbox.val == false)
        {
            
        }

        bbox.Set(false);
        return moveOneExit(fmem, ref iptr, out err);

    }

    private bool MOV_B(MethodBlock[] fmem, ref int iptr, out string err)
    {
        if (errorParamBounds(iptr, 1, ref fmem[iptr], out err))
                return false;

        var param1 = fmem[++iptr].GetParamInfo();

        int val = getValue(param1, out err);

        if (err != "")
        {
            err = $"[ERROR] {fmem[iptr].GetSourcePos()}: Could not get value to move backwards.\n{err}";
            return false;
        }

        bbox.Set(false);
        EmitSignal("doMOV_", val, new Vector2(0, -1));
        while (bbox.val == false)
        {
            
        }

        bbox.Set(false);
        return moveOneExit(fmem, ref iptr, out err);
    }

    [Signal]
    public delegate void doJUMP_UPEventHandler(int amount, Vector2 dir);
    private bool JUMP_UP(MethodBlock[] fmem, ref int iptr, out string err)
    {
        if (errorParamBounds(iptr, 1, ref fmem[iptr], out err))
                return false;

        var param1 = fmem[++iptr].GetParamInfo();

        int val = getValue(param1, out err);

        if (err != "")
        {
            err = $"[ERROR] {fmem[iptr].GetSourcePos()}: Could not get value for jumping up.\n{err}";
            return false;
        }

        EmitSignal("doJUMP_UP", val);
        return moveOneExit(fmem, ref iptr, out err);
    }

    [Signal]
    public delegate void doFLY_EventHandler(int amount);
    private bool FLY_UP(MethodBlock[] fmem, ref int iptr, out string err)
    {
        if (errorParamBounds(iptr, 0, ref fmem[iptr], out err))
                return false;

        var param1 = fmem[++iptr].GetParamInfo();

        int val = getValue(param1, out err);
        if (err != "")
        {
            err = $"[ERROR] {fmem[iptr].GetSourcePos()}: Could not get value for flying up.\n{err}";
            return false;
        }

        bbox.Set(false);
        EmitSignal("doFLY_", val);

        while (bbox.val == false)
        {
            
        }

        bbox.Set(false);
        return moveOneExit(fmem, ref iptr, out err);
    }

    private bool FLY_DOWN(MethodBlock[] fmem, ref int iptr, out string err)
    {
        if (errorParamBounds(iptr, 0, ref fmem[iptr], out err))
                return false;

        var param1 = fmem[++iptr].GetParamInfo();

        int val = getValue(param1, out err);
        if (err != "")
        {
            err = $"[ERROR] {fmem[iptr].GetSourcePos()}: Could not get value for flying down.\n{err}";
            return false;
        }

        bbox.Set(false);
        EmitSignal("doFLY_", -val);

        while (bbox.val == false)
        {
            
        }

        bbox.Set(false);
        return moveOneExit(fmem, ref iptr, out err);
    }

    // turn off fly
    [Signal]
    public delegate void doFALLEventHandler();
    private bool FALL(MethodBlock[] fmem, ref int iptr, out string err)
    {
        if (errorParamBounds(iptr, 0, ref fmem[iptr], out err))
                return false;

        EmitSignal("doFALL");
        return moveOneExit(fmem, ref iptr, out err);

    }

    private bool CMPKEY(MethodBlock[] fmem, ref int iptr, out string err)
    {
        if (errorParamBounds(iptr, 1, ref fmem[iptr], out err))
                return false;

        var param1 = fmem[++iptr].GetParamInfo();
        int param1val = getValue(param1, out err);

        if (err != "")
        {
            err = $"[ERROR] {fmem[iptr].GetSourcePos()}: Could not compare key, check parameter.\n{err}";
            return false;
        }

        if (param1val >= keysDown.Length)
        {
            err = $"[ERROR] {fmem[iptr].GetSourcePos()}: Key in KEYCMP does not exist, check parameter.\n{err}";
            return false;
        }

        registers[(int)Register.KEYF] = keysDown[param1val] ? 1 : 0;

        return moveOneExit(fmem, ref iptr, out err);
    }

    [Signal]
    public delegate void doSHOOTEventHandler();
    private bool SHOOT(MethodBlock[] fmem, ref int iptr, out string err)
    {
        if (errorParamBounds(iptr, 0, ref fmem[iptr], out err))
                return false;

        EmitSignal("doSHOOT");

        return moveOneExit(fmem, ref iptr, out err);
    }

    // TODO
    [Signal]
    public delegate void doRELOADEventHandler();
    private bool RELOAD(MethodBlock[] fmem, ref int iptr, out string err)
    {
        if (errorParamBounds(iptr, 0, ref fmem[iptr], out err))
                return false;

        EmitSignal("doRELOAD");
        bbox.Set(false);
        while (bbox.val == false)
        {

        }

        bbox.Set(false);
        return moveOneExit(fmem, ref iptr, out err);
    }

    // TODO
    private bool COOL(MethodBlock[] fmem, ref int iptr, out string err)
    {
        if (errorParamBounds(iptr, 0, ref fmem[iptr], out err))
                return false;

        while (OnCooldown)
        {
            lowerHeat(85);
            System.Threading.Thread.Sleep(1000);
        }

        return moveOneExit(fmem, ref iptr, out err);
    }

    private bool PUSH(MethodBlock[] fmem, ref int iptr, out string err)
    {
        if (errorParamBounds(iptr, 1, ref fmem[iptr], out err))
                return false;

        var param1 = fmem[++iptr].GetParamInfo();

        int p = getValue(param1, out err);

        if (err != "")
        {
            err = $"[ERROR] {fmem[iptr].GetSourcePos()}: Failed to get value to push to the stack.\n{err}";
            return false;
        }

        stackPush(p, out err);

        if (err != "")
        {
            err = $"[ERROR] {fmem[iptr].GetSourcePos()}: Failed to push to the stack. \n{err}";
            return false;
        }

        return moveOneExit(fmem, ref iptr, out err);
    }

    private bool POP(MethodBlock[] fmem, ref int iptr, out string err)
    {
        if (errorParamBounds(iptr, 1, ref fmem[iptr], out err))
                return false;

        var param1 = fmem[++iptr].GetParamInfo();
        int val = stackPop(out err);

        if (err != "")
        {
            err = $"[ERROR] {fmem[iptr].GetSourcePos()}: Failed popping from the stack.\n{err}";
            return false;
        }   

        userSetValueAtAddr(param1, val, out err);

        if (err != "")
        {
            err = $"[ERROR] {fmem[iptr].GetSourcePos()}: Failed setting the popped value from the stack into the destination address.\n{err}";
            return false;
        }

        return moveOneExit(fmem, ref iptr, out err);
    }

    // jump if flag
    private bool CALLF(MethodBlock[] fmem, ref int iptr, out string err)
    {
        stackPush(iptr + 3, out err);

        if (err != "")
        {
            err = $"[ERROR] {fmem[iptr].GetSourcePos()}: Failed to call (conditional) instruction.\n{err}";
            return false;
        }

        if (errorParamBounds(iptr, 2, ref fmem[iptr], out err))
                return false;

        // the flag
        ParamInfo param1 = fmem[++iptr].GetParamInfo();
        if (param1.GetParamType() != ParamInfo.ParamType.Register)
        {
            err = $"[ERROR] {fmem[iptr].GetSourcePos()}: JMPF instruction expected a register or flag as the first parameter.";
            return false;
        }
        
        ParamInfo param2 = fmem[++iptr].GetParamInfo();
        if (param2.GetParamType() != ParamInfo.ParamType.Label)
        {
            err = $"[ERROR] {fmem[iptr].GetSourcePos()}: CALLF instruction expected a label as the second parameter.";
            return false;
        }

        if (registers[param1.Get()] == 0)
        {
            stackPop(out err);
            return moveOneExit(fmem, ref iptr, out err);
        }

        iptr = param2.Get();

        if (iptr > 255)
        {
            err = $"[ERROR] {fmem[255].GetSourcePos()}: The label pointed outside of the memory.";
            return false;
        }

        return true;
    }

    // jump if not flag
    private bool CALLNF(MethodBlock[] fmem, ref int iptr, out string err)
    {
        stackPush(iptr + 3, out err);

        if (errorParamBounds(iptr, 2, ref fmem[iptr], out err))
                return false;

        if (err != "")
        {
            err = $"[ERROR] {fmem[iptr].GetSourcePos()}: Failed to call (negative conditional) instruction.\n{err}";
            return false;
        }

        // the flag
        ParamInfo param1 = fmem[++iptr].GetParamInfo();
        if (param1.GetParamType() != ParamInfo.ParamType.Register)
        {
            err = $"[ERROR] {fmem[iptr].GetSourcePos()}: JMPNF instruction expected a register or flag as the first parameter.";
            return false;
        }
        
        ParamInfo param2 = fmem[++iptr].GetParamInfo();
        if (param2.GetParamType() != ParamInfo.ParamType.Label)
        {
            err = $"[ERROR] {fmem[iptr].GetSourcePos()}: CALLNF instruction expected a label as the second parameter.";
            return false;
        }

        if (registers[param1.Get()] != 0)
        {
            stackPop(out err);
            return moveOneExit(fmem, ref iptr, out err);
        }

        iptr = param2.Get();

        if (iptr > 255)
        {
            err = $"[ERROR] {fmem[255].GetSourcePos()}: The label pointed outside of the memory.";
            return false;
        }

        return true;
    }
}
