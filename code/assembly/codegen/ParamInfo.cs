namespace Game.Assembly;

public class ParamInfo
{
    public enum ParamType
    {
        None,
        Register,
        MethodAreaAddress,
        HeapAddress,
        StackAddress,
        Label,
        Value
    }

    private int Value;
    private ParamType Type;

    internal void SetReg(Register r) { Type = ParamType.Register; Value=(int)r; }
    internal void SetMethodAddress(int a) { Type = ParamType.MethodAreaAddress; Value=a; }
    internal void SetHeapAddress(int a) { Type = ParamType.HeapAddress; Value=a; }
    internal void SetStackAddress(int a){ Type = ParamType.StackAddress; Value=a; }
    internal void SetMiscValue(int x){ Type = ParamType.Value; Value=x; }
    internal void PromiseLabel() => Type = ParamType.Label;
    internal void SetLabelAddr(int a) => Value = a;

    public ParamInfo()
    {
        Clean();
    }

    internal void Clean()
    {
        Value = -1;
        Type = ParamType.None;
    }

    public ParamType GetParamType() => Type;
    public int Get() => Value;
}
