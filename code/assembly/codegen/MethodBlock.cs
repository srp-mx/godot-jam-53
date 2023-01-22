namespace Game.Assembly;

// NOTE(SRP): false return indicates that the program must stop
// and check for errors
public delegate bool ExecuteInstruction(MethodBlock[] mem, ref int instructionPtr, out string runtimeError);

public struct MethodBlock
{
    public int DisplayInt;
    private int sourceLine;
    private int sourceColumn;
    private ExecuteInstruction instruction = VOID;
    private ParamInfo paramInfo;

    private MethodBlock[] table;
    private int position;

    public MethodBlock(MethodBlock[] table, int position)
    {
        this.position = position;
        this.table = table;
        sourceLine = -1;
        sourceColumn = -1;
        instruction = VOID;
        paramInfo = new ParamInfo(table, position);
        DisplayInt = 0;
    }
    
    public void Clean()
    {
        DisplayInt = 0;
        sourceLine = 0;
        sourceColumn = 0;
        instruction = VOID;
        paramInfo = paramInfo ?? new ParamInfo(table, position);
        paramInfo.Clean();
    }

    public ParamInfo GetParamInfo()
    {
        return paramInfo;
    }

    public bool IsUnassigned()
    {
        bool isNotParam = paramInfo is null || paramInfo.GetParamType() == ParamInfo.ParamType.None;
        bool isNotInstr = instruction is null || instruction == VOID;
        return isNotParam && isNotInstr;
    }

    public void SetInstruction(InstructionSpec spec, int sourceLine, int sourceColumn)
    {
        this.instruction = spec.Instruction;
        this.DisplayInt = spec.DisplayInt;
        this.sourceLine = sourceLine;
        this.sourceColumn = sourceColumn;
    }

    public bool Execute(MethodBlock[] mem, ref int instructionPtr, out string runtimeError)
    {
        if (instruction is null)
            instruction = VOID;

        return instruction(mem, ref instructionPtr, out runtimeError);
    }

    public string GetSourcePos() => LexStr.FormatPosition(sourceColumn, sourceLine);

    private static bool VOID(MethodBlock[] mem, ref int instructionPtr, out string exit)
    {
        exit = $"[ERROR] {mem[instructionPtr].GetSourcePos()}: Tried to execute unassigned instruction memory at runtime.";
        return false;
    }
}
