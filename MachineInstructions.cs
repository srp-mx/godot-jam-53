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
        // InstructionSpec(Name, DisplayInt, Params, Instruction)
        instructions.Add(new("HLT", 1, 0, HLT), InstructionSets.Available.Basic);
        instructions.Add(new("WAIT", 2, 1, WAIT), InstructionSets.Available.Basic);
        instructions.Add(new("RET", 3, 0, RET), InstructionSets.Available.Basic);
        instructions.Add(new("CALL", 4, 1, CALL), InstructionSets.Available.Basic);
        instructions.Add(new("PRINT", 5, 1, PRINT), InstructionSets.Available.Basic);
        availableInstructionSets.Add(InstructionSets.Available.Basic);
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

        GD.Print("A");
        if (errorParamBounds(iptr, 1, ref fmem[iptr], out err))
                return false;

        GD.Print("B");
        ParamInfo param1 = fmem[++iptr].GetParamInfo();
        if (param1.GetParamType() == ParamInfo.ParamType.Value)
        {
            int time = param1.Get() * timeMultiplier;
            debugLog("EXEC WAIT, waiting " + time + "ms");
            System.Threading.Thread.Sleep(time);
            debugLog("EXEC WAIT, finished");
            return moveOneExit(fmem, ref iptr, out err);
        }
        GD.Print("C");

        int addrval = getValueFromAddr(param1, out err);
        if (err != "")
        {
            err = $"[ERROR] {fmem[iptr].GetSourcePos()}: We can only wait a constant or a value in a memory address. \n{err}";
            return false;
        }
        GD.Print("D");

        iptr++; // go to next instruction
        return true;
    }

    
    private bool CALL(MethodBlock[] fmem, ref int iptr, out string err)
    {
        if (stackPtr == 255)
        {
            err = $"[ERROR] {fmem[iptr].GetSourcePos()}: Stack overflow D:";
            return false;
        }

        stack[stackPtr++] = iptr + 2; 

        if (errorParamBounds(iptr, 1, ref fmem[iptr], out err))
                return false;

        ParamInfo param1 = fmem[++iptr].GetParamInfo();
        
        if (param1.GetParamType() != ParamInfo.ParamType.Label)
        {
            err = $"[ERROR] {fmem[iptr].GetSourcePos()}: Call instruction expected a label.";
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

    private bool RET(MethodBlock[] fmem, ref int iptr, out string err)
    {
        if (stackPtr == 0)
        {
            err = $"[ERROR] {fmem[iptr].GetSourcePos()}: Stack underflow :0 (fancy stack overflow because it goes in reverse).";
            return false;
        }
        int retAddr = stack[--stackPtr];
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
        throw new NotImplementedException();
    }

}
