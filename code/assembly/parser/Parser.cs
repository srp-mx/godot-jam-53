using System;
using System.Collections.Generic;
using Game.Monads;
namespace Game.Assembly;

public class Parser
{
    private Lexer lexer;
    private Token? lastToken = null;
    private Token? nextToken = null;
    private InstructionSets instructions;
    private ICollection<InstructionSets.Available> availableInstructions;
    private LabelHandler lh;

    private LexStr NextPos = new("");
    private LexStr CurrPos = new("");

    public Parser(InstructionSets instructions)
    {
        this.lexer = new();
        this.lh = new();
        this.instructions = instructions;
    }

    public void SetProgram(string program)
    {
        lastToken = null;
        nextToken = null;
        lexer.SetProgram(program);
        lh.Clean();
        NextPos.Reset(program);
        CurrPos.Reset(program);
    }

    public Errable<ASTNode> Parse(ICollection<InstructionSets.Available> availableInstructions)
    {
        this.availableInstructions = availableInstructions;
        return parsePrimary();
    }

    /// primary ::
    ///     instruction
    ///     label
    ///     EOF
    private Errable<ASTNode> parsePrimary()
    {
        // NOTE(srp): Ensures lastToken and nextToken are never null after
        advanceToken();

        if (lastToken!.Value == TokenValue.EOF)
        {
            return new EofAST();
        }

        // Get word, not accepting numbers as primary
        if (lastToken.Value != TokenValue.Word)
        {
            string srcpos = CurrPos.PosStr();
            return Errable<ASTNode>.Err($"[ERROR] {srcpos}: Label or instruction expected, got '{CurrPos.AsStr()}' at {srcpos}.");
        }
        string word = CurrPos.AsStr();

        // Check if it's an existing (not necessarily available) instruction
        if (instructions.Exists(word))
        {
            return parseInstruction(word)
                    .ErrableMap<ASTNode>(childType => childType as ASTNode);
        }

        // It must be a label definition
        return parseLabel(word)
                        .ErrableMap<ASTNode>(childType => childType as ASTNode); 
    }
    
    /// instruction::
    ///     Word (parameter ','?)*
    private Errable<InstructionAST> parseInstruction(string name)
    {
        int initCol = CurrPos.Column;
        int initLine = CurrPos.Line;
        string initStr = LexStr.FormatPosition(initCol, initLine);

        var instructionQuerry = instructions.Get(name, availableInstructions);

        return instructionQuerry.ErrableMap<InstructionAST>(
        mapping: spec =>
        {
            ErrableList<ParamAST> parameters = getParamList();

            if (parameters.Count != spec.Params)
                return Errable<InstructionAST>.Err($"[ERROR] {initStr}: The instruction '{name}' has {spec.Params} parameters, but you gave it {parameters.Count}.");

            if (spec.Params == 0)
                return InstructionAST.Generate(spec, initCol, initLine, parsePrimary());

            // Link parameters together (not the last since that links to whatever is after)
            var paramsQuery = parameters.ForAllWithNext((curr, nxt) => curr.SetNext(nxt)
                    .ErrableMap<ParamAST>(astNode => astNode as ParamAST));
            var lastParamQuery = parameters.GetTail();

            return Errable<InstructionAST>.ErrableBiMap(
            paramsQuery, lastParamQuery, // check correctness of all params
            (firstParams, lastParam) => // formerly _ and realParam
            {
                
                return Errable<InstructionAST>.ErrableBiMap(
                // TODO(srp): this method vvv
                InstructionAST.Generate(spec, initCol, initLine, parameters.GetHead()
                    .ErrableMap<ASTNode>(paramAst => paramAst as ASTNode)), 
                parsePrimary(),
                (instr, nextNode) =>
                {
                    return lastParam.SetNext(nextNode).ErrableMap<InstructionAST>(
                    mapping: _ => 
                    {
                        return instr;
                    }
                    );
                }
                );
            }
            );
        }, $"[ERROR] {initStr}: You're missing the hardware to use '{name}' ;)\n"
        );
    }

    private ErrableList<ParamAST> getParamList()
    {
        ErrableList<ParamAST> parameters = new();
        while (NextPos.Line == CurrPos.Line && nextToken.Value != TokenValue.EOF)
        {
            parameters.Append(parseParam()); // NOTE(srp): Not recursive, handle with care

            // Eat optional ',' after parameters
            if (nextToken.Character == ',')
            {
                advanceToken();
            }
        }

        return parameters;
    }


    // TODO(srp): Return Errable<ParamAST>
    /// parameter::
    ///     Word
    ///     Number
    private Errable<ParamAST> parseParam()
    {
        advanceToken();

        switch (lastToken.Value)
        {
            case TokenValue.Number:
            case TokenValue.Word:
                return new ParamAST(CurrPos.AsStr(), lh, CurrPos.Column, CurrPos.Line); // TODO(srp): Modify to fit pos params
        }

        // Value at address
        if (lastToken.Character == '[')
        {
            if (nextToken.Value == TokenValue.Word || nextToken.Value == TokenValue.Number)
            {
                advanceToken();
                ParamAST pAst = new ParamAST($"[{CurrPos.AsStr()}]", lh, CurrPos.Column, CurrPos.Line);
                advanceToken(); // Eat the ']'
                if (lastToken.Character != ']') // NOTE(srp): Due to look-ahead, we could ignore this lol
                {
                    string pos = CurrPos.PosStr();
                    return Errable<ParamAST>.Err($"[ERROR] {pos}: You forgot the closing bracket ']' at {pos}!");
                }
                return pAst;
            }
        }

        string posStr = CurrPos.PosStr();
        return Errable<ParamAST>.Err($"[ERROR] {posStr}: There should be a parameter at {posStr}, but there isn't!\n\tYou wrote '{lastToken.Character}', but a parameter must be a number, a label, a register or some sort of memory address.");
    }

    /// label::
    ///     Word ':'
    private Errable<LabelAST> parseLabel(string identifier)
    {
        int labelCol = CurrPos.Column;
        int labelLine = CurrPos.Line;

        advanceToken();
        if (lastToken!.Value == TokenValue.None && lastToken.Character == ':')
        {
            // If the next token is in the next line or the file ends
            if (NextPos.Line > CurrPos.Line || nextToken!.Value == TokenValue.EOF)
            {
                // TODO(srp): check if label at EOF bugs out
                Errable<ASTNode> nextNodeQuery = parsePrimary();

                return nextNodeQuery.ErrableMap<LabelAST>(
                mapping: nextNode =>
                {
                    return LabelAST.Generate(identifier, lh, labelCol, labelLine, nextNode); // TODO(srp): change to accomodate labelLine/Col
                });
            }

            string pos = CurrPos.PosStr();
            return Errable<LabelAST>.Err($"[ERROR] {pos}: The label '{identifier}' must not have stuff in the same line after the ':'");
        }

        string strPos = CurrPos.PosStr();
        // It doesn't have the shape of a label but there's no matching instruction
        // TODO(srp): Lavenshtein distance suggestion?
        return Errable<LabelAST>.Err($"[ERROR] {strPos}: The instruction '{identifier}' does not exist!\n\tIs '{identifier}' a label but you forgot the ':'?\n\tOr maybe you misspelled one of these:\n\t\t{instructions.GetSuggestions(identifier, availableInstructions)}.");
    }

    private Token advanceToken()
    {
        lastToken = nextToken;
        NextPos.CopyTo(CurrPos);

        nextToken = lexer.GetToken();
        ExternDebug.DBPrint("Token: " + nextToken.Value.ToString());
        lexer.Text.CopyTo(NextPos);
        ExternDebug.DBPrint("Txt: " +  NextPos.AsStr());

        if (lastToken == null) return advanceToken();
        return lastToken;
    }

}
