using System.Collections.Generic;
using Game.Monads;
namespace Game.Assembly;

public abstract class ASTNode
{
    public int srcLine, srcColumn;
    public ASTNode next;

    public virtual Errable<ASTNode> Codegen(CompiledCode target)
    {
        return next.Codegen(target);
    }

    protected ASTNode(int srcColumn, int srcLine, ASTNode next)
    {
        this.srcLine = srcLine;
        this.srcColumn = srcColumn;
        this.next = next; // NOTE(srp): The recursive part :)
    }
}
