using System;
using System.Collections.Generic;
using Game.Monads;
using Game.Algorithm;
namespace Game.Assembly;

public class InstructionSets
{
    public enum Available
    {
        Basic
    }

    private Dictionary<string, InstructionSpec> Basic = new();
    private InstructionSpec[] instructionsByIndex = new InstructionSpec[256];

    private Available[] allSets;

    public InstructionSets()
    {
        allSets = new[]{
            Available.Basic
        };
    }

    public void Add(InstructionSpec instruction, Available iSet)
    {
        instructionsByIndex[instruction.DisplayInt] = instruction;
        switch (iSet)
        {
            case Available.Basic:
                Basic.Add(instruction.Name.ToUpper(), instruction);
                break;
            default:
                throw new Exception("Instruction set missing, check InstructionSets.cs to check how to add it properly.");
        }
    }

    public bool Exists(string instructionName)
    {
        return Get(instructionName, allSets).Match(true, false); // Approved match
    }

    public Errable<InstructionSpec> Get(string instructionName, IEnumerable<Available> setsAvailable)
    {
        instructionName = instructionName.ToUpper();
        foreach (var set in GetSets(setsAvailable))
        {
            if (!set.TryGetValue(instructionName, out InstructionSpec? instruction) || instruction is null)
            {
                continue;
            }
            return instruction;
        }

        return Errable<InstructionSpec>.Err($"[PROBLEM]: Instruction '{instructionName}' not found!");
    }

    public Errable<InstructionSpec> Get(int instructionNum, IEnumerable<Available> setsAvailable)
    {
        if (instructionNum < 0 || instructionNum >= instructionsByIndex.Length)
        {
            return Errable<InstructionSpec>.Err($"[PROBLEM]: Instruction number'{instructionNum}' is out of bounds!");
        }

        if (instructionsByIndex[instructionNum] is null)
        {
            return Errable<InstructionSpec>.Err($"[PROBLEM]: Instruction number'{instructionNum}' not found!");
        }

        string instructionName = instructionsByIndex[instructionNum].Name.ToUpper();

        foreach (var set in GetSets(setsAvailable))
        {
            if (!set.TryGetValue(instructionName, out InstructionSpec? instruction) || instruction is null)
            {
                continue;
            }
            return instruction;
        }

        return Errable<InstructionSpec>.Err($"[PROBLEM]: Instruction number'{instructionNum}' not found!");
    }
   
    public string GetSuggestions(string mistyped, IEnumerable<Available> setsAvailable)
    {
        ReallySortedList<string> candidates = new();
        foreach (var set in GetSets(setsAvailable))
        {
            foreach (var instr in set.Keys)
            {
                int lev = Levenshtein.Get(mistyped.ToUpper(), instr.ToUpper());
                if (lev > 3)
                    continue;

                candidates.Add(lev, instr);
            }
        }

        if (candidates.Count == 0)
            return "hmm, I've got nothing sorry :(";

        string ans = "";
        bool isFirst = true;
        foreach (var candidate in candidates)
		{
            if (!isFirst)
                ans += ",";

            ans += $" {candidate}";
            isFirst = false;
		}
        return ans;
    }

    private IEnumerable<Dictionary<string, InstructionSpec>> GetSets(IEnumerable<Available> setsAvailable)
    {
        List<Dictionary<string, InstructionSpec>> sets = new();
        foreach (Available set in setsAvailable)
        {
            switch (set)
            {
                case Available.Basic: sets.Add(Basic); continue;
            }
        }
        return sets;
    }

}
