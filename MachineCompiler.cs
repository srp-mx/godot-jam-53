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
    }

    private void compileProgram(string program)
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

        codeLog("[SUCCESS]");
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
