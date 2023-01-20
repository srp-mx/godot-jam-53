namespace Game.Assembly;

public class InstructionSpec
{
    public readonly string Name;
    public readonly int DisplayInt;
    public readonly int Params;
    public readonly ExecuteInstruction Instruction;

    public InstructionSpec(string Name, int DisplayInt, int Params, ExecuteInstruction Instruction)
    {
        this.Name = Name.ToUpper();
        this.DisplayInt = DisplayInt;
        this.Params = Params;
        this.Instruction = Instruction;
    }
}
