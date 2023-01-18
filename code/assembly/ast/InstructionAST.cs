using System;
using Game.Monads;
namespace Game.Assembly;

public class InstructionAST : ASTNode
{
    private InstructionSpec spec;

    public static Errable<InstructionAST> Generate(InstructionSpec spec, int srcColumn, int srcLine, Errable<ASTNode> next)
    {
        return next.ErrableMap<InstructionAST>(
        mapping: nextNode =>
        {
            return new InstructionAST(spec, srcColumn, srcLine, nextNode);
        }
        );
    }

    private InstructionAST(InstructionSpec spec, int srcColumn, int srcLine, ASTNode next)
        : base(srcColumn, srcLine, next)
    {
        this.spec = spec;
    }

    public override Errable<ASTNode> Codegen(CompiledCode target)
    {
        if (target.codegenPtr < 0 || target.codegenPtr > 255)
        {
            string pos = LexStr.FormatPosition(srcColumn, srcLine);
            return Errable<ASTNode>.Err($"[ERROR] {pos}: Too many instructions, ran out of instruction memory. \n\tCutoff at {pos}.");
        }

        ref MethodBlock myBlock = ref target.MethodArea[target.codegenPtr++];
        myBlock.Clean();
        myBlock.SetInstruction(spec, srcLine, srcColumn);
        ExternDebug.DBPrint("Generated instruction " + spec.Name);
        return next.Codegen(target);
    }
}

