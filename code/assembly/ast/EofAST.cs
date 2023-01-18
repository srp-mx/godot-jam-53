using System;
using Game.Monads;

namespace Game.Assembly;

public class EofAST : ASTNode
{
	public EofAST()
		: base(0, 0, null)
	{

	}

	public override Errable<ASTNode> Codegen(CompiledCode target)
	{
        ExternDebug.DBPrint("Generated EOF");
        return this;
	}
}
