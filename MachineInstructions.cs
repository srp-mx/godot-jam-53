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
                err = "[PROBLEM]: Referencing method area addresses is not supported :/.\n\tCan a label suffice?";
                return -1;
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
            return false;
        }

        err = "";
        return true;
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
        err = "";
        return false;
    }

    private bool WAIT(MethodBlock[] fmem, ref int iptr, out string err)
    {
        int timeMultiplier = 100; // tenths of a second

        if (errorParamBounds(iptr, 1, ref fmem[iptr], out err))
                return false;

        ParamInfo param1 = fmem[++iptr].GetParamInfo();
        if (param1.GetParamType() == ParamInfo.ParamType.Value)
        {
            int time = param1.Get() * timeMultiplier;
            System.Threading.Thread.Sleep(time);
            return moveOneExit(fmem, ref iptr, out err);
        }

        int addrval = getValueFromAddr(param1, out err);
        if (err != "")
        {
            err = $"[ERROR] {fmem[iptr].GetSourcePos()}: We can only wait a constant or a value in a memory address. \n{err}";
            return false;
        }

        iptr++; // go to next instruction
        return true;
    }
}
