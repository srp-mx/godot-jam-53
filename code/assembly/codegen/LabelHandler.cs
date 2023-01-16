using System;
using System.Collections.Generic;
using Game.Monads;

namespace Game.Assembly;

public class LabelHandler
{
    private Dictionary<string, int> labels = new();
    private HashSet<string> identifiers = new();
    private Dictionary<string, string> labelPosition = new();

    public void Clean()
    {
        labels.Clear();
        identifiers.Clear();
        labelPosition.Clear();
    }

    public void Add(string identifier, int index)
    {
        labels.TryAdd(identifier, index);
    }

    // NOTE(srp): IsFine is return value
    public bool RegisterLabel(string identifier, int srcColumn, int srcLine)
    {
        if (IsRegistered(identifier))
        {
            return false;
        }
        identifiers.Add(identifier);
        labelPosition.TryAdd(identifier, LexStr.FormatPosition(srcColumn, srcLine));
        return true;
    }

    public bool IsRegistered(string identifier) => identifiers.Contains(identifier);

    public Errable<int> Get(string identifier)
    {
        if (labels.TryGetValue(identifier, out int value))
        {
            return value;
        }

        return Errable<int>.Err($"[PROBLEM][DEV]: The label '{identifier}' failed to get fetched.");
    }

    public Errable<string> GetSrcPos(string identifier)
    {
        if (labelPosition.TryGetValue(identifier, out string value))
        {
            return value;
        }

        return Errable<string>.Err($"[PROBLEM][DEV]: Couldn't get the source code location of the label '{identifier}'.");
    }

}
