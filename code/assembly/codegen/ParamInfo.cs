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
        RegMethodAreaAddress,
        RegHeapAddress,
        RegStackAddress,
        Label,
        Value
    }

    private int Value;
    private ParamType Type;
    private MethodBlock[] ParentTable;
    private int ParentAddr;

    internal void SetReg(Register r) => Set(ParamType.Register, (int)r);
    internal void SetMethodAddress(int a) => Set(ParamType.MethodAreaAddress, a);
    internal void SetHeapAddress(int a) => Set(ParamType.HeapAddress, a);
    internal void SetStackAddress(int a) => Set(ParamType.StackAddress, a);
    internal void SetRegMethodAddress(int a) => Set(ParamType.RegMethodAreaAddress, a);
    internal void SetRegHeapAddress(int a) => Set(ParamType.RegHeapAddress, a);
    internal void SetRegStackAddress(int a) => Set(ParamType.RegStackAddress, a);
    public void SetMiscValue(int x) => Set(ParamType.Value, x);
    internal void PromiseLabel() => Type = ParamType.Label;
    internal void SetLabelAddr(int a) => Set(ParamType.Label, a);

    private void Set(ParamType t, int x) 
    {
        Type = t;
        if (t == ParamType.None)
            return;

        ParentTable[ParentAddr].DisplayInt = x;
        Value = x;
        Type = t;
    }

    public ParamInfo(MethodBlock[] ParentTable, int ParentAddr)
    {
        Clean();
        this.ParentTable = ParentTable;
        this.ParentAddr = ParentAddr;
    }

    internal void Clean()
    {
        Set(ParamType.None, -1);
    }

    public ParamType GetParamType() => Type;
    public int Get() => Value;
}
