using System;
namespace Game.Assembly;

public class Lexer
{
    private string program = "";
    private char lastChar = ' ';
    
    public LexStr Text = new("");

    internal void SetProgram(string program)
    {
        lastChar = ' ';
        this.program = program;
        Text.Reset(program);
    }

    public Token GetToken()
    {
        // Skip whitespace
        while (Char.IsWhiteSpace(lastChar))
        {
            advanceChar();
        }

        // Words
        if (Char.IsLetter(lastChar) || lastChar == '_')
        {
            int startIndex = Text.Index - 1;
            int wordLength = 1;
            while (Char.IsLetterOrDigit(advanceChar()))
            {
                wordLength++;
            }

            Text.SetSpan(startIndex, wordLength);
            return new Token(TokenValue.Word);
        }

        // Numbers
        if (Char.IsNumber(lastChar))
        {
            int startIndex = Text.Index - 1;
            int wordLength = 1;

            bool isHex = false; // Special case cause hex has letters :(

            // Support for hex, octal and binary prefixes
            if (lastChar == '0')
            {
                advanceChar();
                wordLength++;

                switch(lastChar)
                {
                    case 'x':
                    case 'X':
                        isHex = true;
                        goto number_prefix;
                    case 'b':
                    case 'B':
                    case 'o':
                    case 'O':
number_prefix:
                        advanceChar();
                        wordLength++;
                        break;
                    default:
                        if (!Char.IsNumber(lastChar))
                        {
                            Text.SetSpan(startIndex, --wordLength);
                            return new Token(TokenValue.Number);
                        }
                        break;
                }
            }

            while (Char.IsNumber(advanceChar()) || isHex)
            {
                //if (!isHex)
                if (Char.IsNumber(lastChar))
                {
                    wordLength++;
                    continue;
                }

                switch (lastChar)
                {
                    case 'a':
                    case 'A':
                    case 'b':
                    case 'B':
                    case 'c':
                    case 'C':
                    case 'd':
                    case 'D':
                    case 'e':
                    case 'E':
                    case 'f':
                    case 'F':
                        wordLength++;
                        continue;
                    default:
                        break;
                }
                break;
            }

            Text.SetSpan(startIndex, wordLength);
            return new Token(TokenValue.Number);
        }

        // Comments
        if (lastChar == ';')
        {
            do
            {
                advanceChar();
            } while (!isEOF() && lastChar != '\n' && lastChar != '\r');

            if (!isEOF())
                return GetToken();
        }

        // EOF
        if (isEOF())
        {
            Text.SetSpan(0,0);
            return new Token(TokenValue.EOF);
        }

        // Unknown, just give the character
        Token tok = new Token(lastChar);
        Text.SetSpan(Text.Index - 1, 1);
        advanceChar();
        return tok;
    }

    private char advanceChar()
    {
        // NOTE(srp): This happening before anything else ensures correctness
        // given that this allows the newline character to be in the same line
        // as the text before it, and we need that to delimit parameters.
        if (lastChar == '\n')
        {
            Text.AdvanceLine();
        }
        else
        {
            Text.AdvanceColumn();
        }
        
        if (Text.Index == program.Length)
            lastChar = '\0';
        else
            lastChar = program[Text.Index++];
        
        return lastChar;
    }

    private bool isEOF() => lastChar == '\0';
    
}
