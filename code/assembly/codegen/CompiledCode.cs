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

    public void Reset()
    {
        codegenPtr = 0;
        OnRecompile.Invoke();
        foreach (var block in MethodArea)
        {
            block.Clean();
        }
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
        return it;
    }
}
