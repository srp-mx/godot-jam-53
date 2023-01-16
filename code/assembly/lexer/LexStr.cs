using System;
namespace Game.Assembly;

public class LexStr
{
    private string src = "";
    // As charIndex
    public int Index = 0;
    // As TextPosition
    public int Column { get; private set; }
    public int Line { get; private set; }
    // As ReadOnlySpan<char>
    private int Start = 0;
    private int Length = 0;

    public LexStr(string src)
    {
        Reset(src);
    }

    public void Reset()
    {
        Column = 0;
        Line = 1;
        Index = 0;
        Length = 0;
        Start = 0;
    }

    public void Reset(string src)
    {
        this.src = src;
        Reset();
    }

    public void CopyTo(LexStr other)
    {
        other.src = this.src;
        other.Index = this.Index;
        other.Column = this.Column;
        other.Line = this.Line;
        other.Start = this.Start;
        other.Length = this.Length;
    }

    // As TextPosition
    public void AdvanceColumn() => Column++;
    public void AdvanceLine()
    {
        Column = 0;
        Line++;
    }

    public string PosStr() => FormatPosition(Column, Line);

    public static string FormatPosition(int Column, int Line)
        => $"(lin:{Line}, col:{Column})";

    // As ReadOnlySpan<char>
    public ReadOnlySpan<char> AsSpan() => src.AsSpan(Start, Length);
    public string AsStr() => AsSpan().ToString();
    public void SetSpan(int start, int len)
    {
        this.Start = start;
        this.Length = len;
    }

}
