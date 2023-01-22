using Godot;
using System;
using Game.Assembly;

public partial class Machine : Node
{

    public string GetDoc(string instr)
    {
        var specQ = instructions.Get(instr, availableInstructionSets);
        var spec = specQ.Match(instr => instr, ()=> null);
        if (spec is null)
            return "You have not unlocked this yet!";

        return docs[spec.Name] + "\nInstruction number: " + spec.DisplayInt;
    }

    public string GetDisplayName(string instr)
    {
        var specQ = instructions.Get(instr, availableInstructionSets);
        var spec = specQ.Match(instr => instr, ()=> null);
        if (spec is null)
            return "???";

        return spec.Name;
    }

    System.Collections.Generic.Dictionary<string, string> docs = new();

    void initDocs()
    {
        docs["HLT"] = "HLT\nStops and finishes the program.";
        docs["WAIT"] = "WAIT <VALUE>\nWaits for a specified amount of tenths of a second.";
        docs["RET"] = "RET\nGoes to the instruction in the (program) memory address specified at the top of the stack.\nSee also: CALL.";
        docs["CALL"] = "CALL <LABEL>\nPushes the address of the next instruction to the stack and jumps to a specified label.\nIntended to be used alongside RET, where we jump to a label which ends in RET.";
        docs["PRINT"] = "PRINT <VALUE>\nPrints the value to the output.";
        docs["NOP"] = "NOP\nDoes nothing lmao.\nIts intended use is for waiting an amount of instructions, since it does take a clock cycle to execute.";
        docs["MOV"] = "MOV <ADDRESS> <VALUE>\nStores a value in a memory address.";
        docs["ADD"] = "ADD <ADDRESS> <VALUE>\nAdds the value specified and the value at the address specified, replacing its value with the result.";
        docs["NEG"] = "NEG <ADDRESS>\nChanges the value at the address specified with its two's complement (negative value).\nIt will not show as a negative number, but it will work as such when adding because maths.";
        docs["CMP"] = "CMP <VALUE> <VALUE>\nWill compare two values.\nThe result of the comparison will be found on flags/registers.\n\tEF: 1 if equal, 0 if not.\n\tLEQF: 1 if the first value is less than or equal to the second, otherwise 0.\n\tLEF: 1 if the first value is less than the second, otherwise 0.";
        docs["JMPF"] = "JMPF <REGISTER/FLAG> <LABEL>\nIf the flag/register is not zero, we will go to the instruction at the label specified.\nSee CMP, JMP, JMPNF.";
        docs["JMPNF"] = "JMPNF <REGISTER/FLAG> <LABEL>\nIf the flag/register is zero, we will go to the instruction at the label specified.\nSee CMP, JMP, JMPF.";
        docs["INC"] = "INC <ADDRESS>\nIncreases the value in the specified address by one.";
        docs["DEC"] = "DEC <ADDRESS>\nDecreases the value in the specified address by one.";
        docs["JMP"] = "JMP <LABEL>\nWe will continue the program at the first instruction after the label specified.";
        docs["ROT_CLOCK"] = "ROT_CLOCK <VALUE>\nRotates clockwise a specified amount of degrees.";
        docs["ROT_ANTI"] = "ROT_ANTI <VALUE>\nRotates anticlockwise a specified amount of degrees.";
        docs["INTERACT"] = "INTERACT\nInteracts with the nearest interactive object.";
        docs["MOV_R"] = "MOV_R <VALUE>\nMoves to the right for a specified amount of time (tenths of a second).";
        docs["MOV_L"] = "MOV_L <VALUE>\nMoves to the left for a specified amount of time (tenths of a second).";
        docs["MOV_F"] = "MOV_F <VALUE>\nMoves forward for a specified amount of time (tenths of a second).";
        docs["MOV_B"] = "MOV_B <VALUE>\nMoves backwards for a specified amount of time (tenths of a second).";
        docs["JUMP_UP"] = "JUMP_UP <VALUE>\n Jumps with the strength of the specified value.";
        docs["FLY_UP"] = "FLY_UP <VALUE>\n Moves upwards for a specified amount of time (tenths of a second) and keeps the stabilizers enabled so we stay in place.\nSee also: FALL.";
        docs["FLY_DOWN"] = "FLY_DOWN <VALUE>\n Moves downwards for a specific amount of time (tenths of a second= and keeps the stabilizers enabled so we stay in place.\nSee also: FALL.";
        docs["FALL"] = "FALL\nDisables the stabilizers, so if they were enabled we'll fall.";
        docs["CMPKEY"] = "CMPKEY <VALUE>\nIf the key with the key-code specified is held, it sets the KEYF flag/register to 1, otherwise to 0.\nHere are the key-codes:\n" + printKeycodes();
        docs["SHOOT"] = "SHOOT\nShoots. Check ammunition count at the AMM flag/register, if there is none you'll have to RELOAD.";
        docs["RELOAD"] = "RELOAD\nReloads. Sets the AMM flag back to its maximum value.\nTake note that it will block instructions while it runs.";
        docs["COOL"] = "COOL\nCools down. Resets the HTM flag/register since we're now chill.\nTake note that it will block instructions while it runs.";
        docs["PUSH"] = "PUSH <VALUE>\nPushes the value specified at the top of the stack.";
        docs["POP"] = "POP <ADDRESS>\nPuts the value at the top of the stack in the address specified and moves the stack backwards.";
        docs["CALLF"] = "CALLF <REGISTER/FLAG> <LABEL>\nIf the register/flag specified is not zero, we'll continue at the instruction at the label specified and push the next instruction to the stack, otherwise we'll go to the next instruction.\nIntended to be used with RET.\nSee also: CALL, RET, CMP, CMPKEY, JMPF, CALLNF.";
        docs["CALLNF"] = "CALLNF <REGISTER/FLAG> <LABEL>\nIf the register/flag specified is zero, we'll continue at the instruction at the label specified and push the next instruction to the stack, otherwise we'll go to the next instruction.\nIntended to be used with RET.\nSee also: CALL, RET, CMP, CMPKEY, JMPNF, CALLF.";
    }

    private string printKeycodes()
    {
        string ret = "";
        ret += "\t0-9: Keyboard/Keypad 0-9\n";
        ret += "\t10 (0xA): A\n";
        ret += "\t11 (0xB): B\n";
        ret += "\t12 (0xC): C\n";
        ret += "\t13 (0xD): D\n";
        ret += "\t14 (0xE): E\n";
        ret += "\t15 (0xF): F\n";
        for (int i = 16; i <= 34; i++)
        {
            char c = (char)((char)(i - 16) + 'G');
            ret += $"\t{i}: {c}\n";
        }
        ret += "\t36: Space\n";
        ret += "\t37: Enter\n";
        ret += "\t38: Tab\n";
        ret += "\t39: Shift\n";
        ret += "\t40: Up arrow\n";
        ret += "\t41: Down arrow\n";
        ret += "\t42: Right arrow\n";
        ret += "\t43: Left arrow\n";
        return ret;
    }
}
