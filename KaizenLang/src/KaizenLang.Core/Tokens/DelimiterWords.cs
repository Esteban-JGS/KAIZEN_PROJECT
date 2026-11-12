namespace KaizenLang.Core;

public static class DelimiterWords
{
    public const string PAREN_OPEN = "(";
    public const string PAREN_CLOSE = ")";
    public const string SEMICOLON = ";";
    public const string COMMA = ",";
    public const string BRACKET_OPEN = "[";
    public const string BRACKET_CLOSE = "]";
    public const string ANGLE_OPEN = "<";
    public const string ANGLE_CLOSE = ">";
    public const string BLOCK_START = "ying";
    public const string BLOCK_END = "yang";

    public static readonly HashSet<string> SingleCharDelimiters = new HashSet<string>
    {
        PAREN_OPEN,
        PAREN_CLOSE,
        SEMICOLON,
        COMMA,
        BRACKET_OPEN,
        BRACKET_CLOSE,
        ANGLE_OPEN,
        ANGLE_CLOSE
    };

    public static readonly HashSet<string> MultiCharDelimiters = new HashSet<string>
    {
        BLOCK_START, BLOCK_END
    };

    public static readonly HashSet<string> Words = new HashSet<string>(
        SingleCharDelimiters.Concat(new[] { ANGLE_OPEN, ANGLE_CLOSE }).Concat(MultiCharDelimiters)
    );
}
