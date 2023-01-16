namespace Game.Assembly;

public class Token
{
    public char Character { get; private set; } = '\0';
    public TokenValue Value { get; private set; } = TokenValue.None;

    public bool Identified => Value != TokenValue.None;

    public Token(TokenValue value)
    {
        this.Value = value;
    }

    public Token (char character)
    {
        this.Character = character;
    }
}

public enum TokenValue
{
    None,
    Word,
    Number,
    EOF
}
