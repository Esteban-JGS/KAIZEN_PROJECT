namespace KaizenLang.Core;

public static class LiteralWords
{
    public const string TRUE = "true";
    public const string FALSE = "false";
    public const string NULL = "null";

    public static readonly HashSet<string> Words = new HashSet<string>
    {
        TRUE, FALSE, NULL
    };
}
