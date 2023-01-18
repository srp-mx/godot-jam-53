using Godot;
using System;

using System.Threading;
using System.Threading.Tasks;

using Game.Assembly;

public partial class Machine : Node
{
    private static readonly string iOobError = "The instruction pointer went out of bounds!\n\tToo many instructions?";

    InstructionSets instructions = new();
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
        instructions.Add(new("ROT", 16, 1, ROT), InstructionSets.Available.Basic);
        instructions.Add(new("INTERACT", 17, 0, INTERACT), InstructionSets.Available.Basic);
        instructions.Add(new("MOV_R", 18, 1, MOV_R), InstructionSets.Available.Basic);
        instructions.Add(new("MOV_L", 19, 1, MOV_L), InstructionSets.Available.Basic);
        instructions.Add(new("JUMP_UP", 20, 1, JUMP_UP), InstructionSets.Available.Basic);
        instructions.Add(new("FLY_UP", 21, 1, FLY_UP), InstructionSets.Available.Basic);
        instructions.Add(new("FLY_DOWN", 22, 1, FLY_DOWN), InstructionSets.Available.Basic);
        instructions.Add(new("FALL", 23, 0, FALL), InstructionSets.Available.Basic);
        instructions.Add(new("BIND_KEY", 24, 2, BIND_KEY), InstructionSets.Available.Basic);
        instructions.Add(new("SHOOT", 25, 0, SHOOT), InstructionSets.Available.Basic);
        instructions.Add(new("COOL", 26, 0, COOL), InstructionSets.Available.Basic);
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
        if (param.GetParamType() == ParamInfo.ParamType.Value)
        {
            err = "";
            return param.Get();
        }

        return getValueFromAddr(param, out err);
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

        GD.Print("Called instruction at " + iptr);

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

        GD.Print("Popped " + retAddr + " from the stack");

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

        int val = param1.Get();
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

        userSetValueAtAddr(memVal, assignValue, out err);

        if (err != "")
            return false;

        iptr++;
        return true;
    }

    private bool ADD(MethodBlock[] fmem, ref int iptr, out string err)
    {
        throw new NotImplementedException();
        if (errorParamBounds(iptr, 2, ref fmem[iptr], out err))
                return false;

    }

    // gets two's complement
    private bool NEG(MethodBlock[] fmem, ref int iptr, out string err)
    {
        throw new NotImplementedException();
        if (errorParamBounds(iptr, 1, ref fmem[iptr], out err))
                return false;

    }

    private bool CMP(MethodBlock[] fmem, ref int iptr, out string err)
    {
        throw new NotImplementedException();
        if (errorParamBounds(iptr, 2, ref fmem[iptr], out err))
                return false;

    }

    // jump if flag
    private bool JMPF(MethodBlock[] fmem, ref int iptr, out string err)
    {
        throw new NotImplementedException();
        if (errorParamBounds(iptr, 2, ref fmem[iptr], out err))
                return false;

    }

    // jump if not flag
    private bool JMPNF(MethodBlock[] fmem, ref int iptr, out string err)
    {
        throw new NotImplementedException();
        if (errorParamBounds(iptr, 2, ref fmem[iptr], out err))
                return false;

    }


    private bool INC(MethodBlock[] fmem, ref int iptr, out string err)
    {
        throw new NotImplementedException();
        if (errorParamBounds(iptr, 1, ref fmem[iptr], out err))
                return false;

    }

    private bool DEC(MethodBlock[] fmem, ref int iptr, out string err)
    {
        throw new NotImplementedException();
        if (errorParamBounds(iptr, 1, ref fmem[iptr], out err))
                return false;

    }

    private bool JMP(MethodBlock[] fmem, ref int iptr, out string err)
    {
        throw new NotImplementedException();
        if (errorParamBounds(iptr, 1, ref fmem[iptr], out err))
                return false;

    }

    private bool ROT(MethodBlock[] fmem, ref int iptr, out string err)
    {
        throw new NotImplementedException();
        if (errorParamBounds(iptr, 1, ref fmem[iptr], out err))
                return false;

    }

    private bool INTERACT(MethodBlock[] fmem, ref int iptr, out string err)
    {
        throw new NotImplementedException();
        if (errorParamBounds(iptr, 0, ref fmem[iptr], out err))
                return false;
    }

    private bool MOV_R(MethodBlock[] fmem, ref int iptr, out string err)
    {
        throw new NotImplementedException();
        if (errorParamBounds(iptr, 1, ref fmem[iptr], out err))
                return false;

    }

    private bool MOV_L(MethodBlock[] fmem, ref int iptr, out string err)
    {
        throw new NotImplementedException();
        if (errorParamBounds(iptr, 1, ref fmem[iptr], out err))
                return false;

    }

    private bool JUMP_UP(MethodBlock[] fmem, ref int iptr, out string err)
    {
        throw new NotImplementedException();
        if (errorParamBounds(iptr, 1, ref fmem[iptr], out err))
                return false;

    }

    private bool FLY_UP(MethodBlock[] fmem, ref int iptr, out string err)
    {
        throw new NotImplementedException();
        if (errorParamBounds(iptr, 0, ref fmem[iptr], out err))
                return false;

    }

    private bool FLY_DOWN(MethodBlock[] fmem, ref int iptr, out string err)
    {
        throw new NotImplementedException();
        if (errorParamBounds(iptr, 0, ref fmem[iptr], out err))
                return false;

    }

    // turn off fly
    private bool FALL(MethodBlock[] fmem, ref int iptr, out string err)
    {
        throw new NotImplementedException();
        if (errorParamBounds(iptr, 0, ref fmem[iptr], out err))
                return false;

    }

    private bool BIND_KEY(MethodBlock[] fmem, ref int iptr, out string err)
    {
        throw new NotImplementedException();
        if (errorParamBounds(iptr, 2, ref fmem[iptr], out err))
                return false;

    }


    private bool SHOOT(MethodBlock[] fmem, ref int iptr, out string err)
    {
        throw new NotImplementedException();
        if (errorParamBounds(iptr, 0, ref fmem[iptr], out err))
                return false;

    }

    private bool RELOAD(MethodBlock[] fmem, ref int iptr, out string err)
    {
        throw new NotImplementedException();
        if (errorParamBounds(iptr, 0, ref fmem[iptr], out err))
                return false;

    }

    private bool COOL(MethodBlock[] fmem, ref int iptr, out string err)
    {
        throw new NotImplementedException();
        if (errorParamBounds(iptr, 0, ref fmem[iptr], out err))
                return false;

    }

}
