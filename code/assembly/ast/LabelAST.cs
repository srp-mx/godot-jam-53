using System;
using System.Collections.Generic;
using Game.Monads;

namespace Game.Assembly;

public class LabelAST : ASTNode
{
    private string identifier;
    private LabelHandler lh;

    public static Errable<LabelAST> Generate(string identifier, LabelHandler lh, int srcColumn, int srcLine, ASTNode next)
    {
        LabelAST candidate = new(identifier, lh, srcColumn, srcLine, next, out bool collision);
        if (collision)
        {
            var srcQuery = lh.GetSrcPos(identifier);
            string strpos = LexStr.FormatPosition(srcColumn, srcLine);
            return srcQuery.ErrableMap<LabelAST>(
            mapping: other =>
            {
                return Errable<LabelAST>.Err($"[ERROR] {strpos} + {other}: Duplicate label '{identifier}' at {strpos}.\n\tFirst at {other}.");
            }, "[ERROR][DEV]: We detected colliding labels at AST generation, but we can't find them??\n"
            );
        }

        return candidate;
    }

    private LabelAST(string identifier, LabelHandler lh, int srcColumn, int srcLine, ASTNode next, out bool labelCollision)
        : base(srcColumn, srcLine, next)
    {
        this.identifier = identifier;
        this.lh = lh;
        labelCollision = !lh.RegisterLabel(identifier, srcColumn, srcLine);
    }

    public override Errable<ASTNode> Codegen(CompiledCode target)
    {
        // NOTE(srp): Next can only realistically be an instruction,
        // we're adding the current instrptr because the next instruction
        // generated will have that address.
        //
        // NOTE(srp): We can add without fear because when we built the
        // AST we made sure that there are no duplicates.
        lh.Add(identifier, target.codegenPtr);
        ExternDebug.DBPrint("Mapping labels '" + identifier + "' to " + target.codegenPtr);
        return next.Codegen(target);
    }
}
