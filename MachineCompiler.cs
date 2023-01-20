/*
TODO(srp):
For some reason we could compile HLT and WAIT before we
ever added the permissions to the Basic instruction set,
so check why that happened and if it was fixed accidentally
or what, since that was no longer the case by the time CALL
and RET were added. This all must have happened at compile time.
 */


using Godot;
using System;
using System.Collections.Generic;

using Game.Assembly;
using Game.Monads;

public partial class Machine : Node
{
    Parser parser;
    CompiledCode code = new();
    private bool codeRejected = true;

    string lastReadProgram = "";

    private List<InstructionSets.Available> availableInstructionSets = new();
    private void initParser()
    {
        parser = new(instructions);
        code.SetInstructionSets(instructions);
        code.SetAvailableSets(availableInstructionSets);
    }

    public void AddInstructionSet(InstructionSets set)
    {
        availableInstructionSets.Add(set);
    }

    public void compileProgram(string program)
    {
        if (program == lastReadProgram)
            return;
    
        lastReadProgram = program;

        codeRejected = false;

        parser.SetProgram(program);
        var astQuery = parser.Parse(availableInstructionSets);
        var ast = astQuery.Match(identity,()=>reject(astQuery));
        if (codeRejected)
            return;
        code.Reset(); // cleanup before refill
        var genQuery = ast.Codegen(code);
        var gen = genQuery.Match(identity, ()=>reject(genQuery));
        if (codeRejected)
            return;

        for (ASTNode n = ast; n is not EofAST; n = n.next)
        {
            if (n is ParamAST)
            {
                var labelPass = (n as ParamAST).LabelPass(code);
                var labelPassValid = labelPass.Match(identity, ()=>reject(labelPass));
                if (codeRejected)
                    return;
            }
        }

        currInstruction = code.GetEnumerator();

        codeLog("[COMPILE SUCCESSFUL]");
    }

    private T identity<T>(T x) => x;
    private T reject<T>(Errable<T> query)
    {
        string err = query.GetErrLog();
        codeLog(err);
        codeRejected = true;
        return default;
    }

}
