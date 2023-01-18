using System;
using System.Collections;
using System.Collections.Generic;
using Game.Monads;
namespace Game.Assembly;

public partial class CompiledCode : IEnumerable
{
    public MethodBlock[] MethodArea = new MethodBlock[256];
    public int codegenPtr = 0;
    private Action OnRecompile = () => { };

    // NOTE(srp): To check error on exit, check if null or empty string
    private string runtimeError = "";
    private static readonly string success = "[SUCCESS]";

    private InstructionSets sets;
    private ICollection<InstructionSets.Available> availSets;

    public void Reset()
    {
        codegenPtr = 0;
        OnRecompile.Invoke();
        foreach (var block in MethodArea)
        {
            block.Clean();
        }
    }

    public void SetInstructionSets(InstructionSets sets)
    {
        this.sets = sets;
    }

    public void SetAvailableSets(ICollection<InstructionSets.Available> availSets)
    {
        this.availSets = availSets;
    }

    public ReadOnlySpan<char> GetExitStatus()
    {
        if (runtimeError is null && runtimeError == "")
            return success.AsSpan(0);
        return runtimeError.AsSpan(0);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return (IEnumerator)GetEnumerator();
    }

    public IEnumerator GetEnumerator()
    {
        var it = new VMIterator(this, that => { OnRecompile -= that.OnRecompile; });
        OnRecompile += it.OnRecompile;
        it.SetInstructionSets(sets);
        it.SetAvailableSets(availSets);
        return it;
    }
}
