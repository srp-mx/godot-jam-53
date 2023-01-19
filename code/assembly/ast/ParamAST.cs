using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Game.Monads;

namespace Game.Assembly;

public class ParamAST : ASTNode
{
    private string content;
    private static readonly Regex BracketRegex = new(@"^\[.*\]$", RegexOptions.Compiled);
    private int myBlockIdx = -1; // Stored so that we can insert label value on LabelPass
    private LabelHandler lh;

    public ParamAST(string content, LabelHandler lh, int srcColumn, int srcLine)
        : base(srcColumn, srcLine, null)
    {
        this.content = content;
        this.lh = lh;
    }

    public Errable<ASTNode> SetNext(ASTNode node)
    {
        return this.next = node;
    }

    public Errable<int> LabelPass(CompiledCode target)
    {
        ref MethodBlock myBlock = ref target.MethodArea[myBlockIdx];
        if (myBlock.GetParamInfo().GetParamType() != ParamInfo.ParamType.Label)
            return 0; // Fine, just skip since it's not promised to be a label

        ExternDebug.DBPrint("Label pass: " + content);

        var paramInfo = myBlock.GetParamInfo();

        // NOTE(srp): We already established that this label exists 
        // because we registered the ParamType as Label
        var labelQuery = lh.Get(content);
        return labelQuery.NonErrableMap(
        mapping: labelDir =>
        {
            paramInfo.SetLabelAddr(labelDir);
            ExternDebug.DBPrint("labelDir: " + labelDir);
            return labelDir;
        }, $"[ERROR][DEV]: The label '{content}' was accepted earlier, but it was invalid at LabelPass.\n"
        );
    }

    // TODO(srp): check thoroughly
    public override Errable<ASTNode> Codegen(CompiledCode target)
    {
        ExternDebug.DBPrint("Generating param " + content);
        myBlockIdx = target.codegenPtr;

        ref MethodBlock myBlock = ref target.MethodArea[target.codegenPtr++];
        myBlock.Clean();
        var paramInfo = myBlock.GetParamInfo();
        
        // It is some kind of "pointer" (&value)
        if (BracketRegex.IsMatch(content))
        {
            ReadOnlySpan<char> addressCandidate = content.AsSpan(2, content.Length - 3);
            ExternDebug.DBPrint("addr candidate: " + addressCandidate.ToString());
            Errable<int> addressQuery = parseNum(ref addressCandidate);
            Errable<int> regAddressQuery = parseReg(ref addressCandidate);
            // TODO(srp) if it fails, parse register as address dest

            string pos = LexStr.FormatPosition(srcColumn, srcLine);
            return addressQuery.ErrableMap<ASTNode>(
            mapping: addr =>
            {
                switch (content.ToUpper()[1]) // [x???]
                {
                case 'F':
                    paramInfo.SetMethodAddress(addr);
                    break;
                case 'H':
                    paramInfo.SetHeapAddress(addr);
                    break;
                case 'S':
                    paramInfo.SetStackAddress(addr);
                    break;
                default:
                    return regAddressQuery.ErrableMap<ASTNode>(
                    mapping: regAddr =>
                    {
                        switch (content.ToUpper()[1]) // [x???]
                        {
                        case 'F':
                            paramInfo.SetRegMethodAddress(regAddr);
                            break;
                        case 'H':
                            paramInfo.SetRegHeapAddress(regAddr);
                            break;
                        case 'S':
                            paramInfo.SetRegStackAddress(regAddr);
                            break;
                        default:        
                            return Errable<ASTNode>.Err($"[ERROR] {pos}: Parameter looks like a memory address (surrounded by square brackets) but it's not.\n\tMemory addresses look like this: [F14], [H62], [S7], where:\n\t\tF goes to the instruction memory,\n\t\tH goes to the heap memory, and\n\t\tS goes to the stack memory.");
                        }
                        return next.Codegen(target);
                    }
                    );
                }
                return next.Codegen(target);
            });//, $"[ERROR] {pos}: Parameter looks like a memory address (surrounded by square brackets) but it's not.\n\tMemory addresses look like this: [F14], [H62], [S7], where:\n\t\tF goes to the instruction memory,\n\t\tH goes to the heap memory, and\n\t\tS goes to the stack memory.\n");
        }

        // Check registers
        for (int i = 0; i < (int)Register.None; i++)
        {
            Register r = (Register)i;
            if (content.ToUpper() == r.ToString().ToUpper())
            {
                paramInfo.SetReg(r);
                return next.Codegen(target);
            }
        }

        // it better be a number or label now
        ReadOnlySpan<char> numberCandidate = content.AsSpan(0);
        Errable<int> numberQuery = parseNum(ref numberCandidate);

        return numberQuery.Match<Errable<ASTNode>>( // TODO(srp): Disputed match
        good: number =>
        {
            paramInfo.SetMiscValue(number);
            return next.Codegen(target);
        },
        // Not a valid number
        error: () =>
        {
            // Try to make it a label
            if (lh.IsRegistered(content))
            {
                paramInfo.PromiseLabel();
                return next.Codegen(target);
            }
            string pos = LexStr.FormatPosition(srcColumn, srcLine);
            string append = numberQuery.GetErrLog();
            return Errable<ASTNode>.Err($"[ERROR] {pos}: Could not parse parameter '{content}' at {pos}.\n{append}");
        }
        );
    }

    private Errable<int> parseNum(ref ReadOnlySpan<char> substring)
    {

        int number;

        try
        {
            if (substring.StartsWith("0x") || substring.StartsWith("0X"))
                number = Convert.ToInt32(substring.Slice(2).ToString(), 16);
            else if (substring.StartsWith("0b") || substring.StartsWith("0B"))
                number = Convert.ToInt32(substring.Slice(2).ToString(), 2);
            else if (substring.StartsWith("0o") || substring.StartsWith("0O"))
                number = Convert.ToInt32(substring.Slice(2).ToString(), 8);
            else
                number = Convert.ToInt32(substring.ToString(), 10);

            if (number < 0 || number > 255)
            {
                string pos = LexStr.FormatPosition(srcColumn, srcLine);
                return Errable<int>.Err($"[ERROR] {pos}: This is an 8-bit machine! Numbers must lie between 0 and 255, {number} is a bit out there!");
            }
            return number;
        }
        catch
        {
            string pos = LexStr.FormatPosition(srcColumn, srcLine);
            return Errable<int>.Err($"[PROBLEM] {pos}: '{substring.ToString()}' is not a number.");
        }
    }

    private Errable<int> parseReg(ref ReadOnlySpan<char> substring)
    {
        string regCandidate = substring.ToString();
        int regAddr;
        for (int i = 0; i < (int)Register.None; i++)
        {
            if (((Register)i).ToString() == regCandidate)
            {
                return i;
            }
        }
        string pos = LexStr.FormatPosition(srcColumn, srcLine);
        return Errable<int>.Err($"[PROBLEM] {pos}: '{regCandidate}' is not a register.");
    }
}
