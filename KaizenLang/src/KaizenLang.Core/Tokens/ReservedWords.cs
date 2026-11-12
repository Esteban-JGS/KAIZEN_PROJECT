namespace KaizenLang.Core;

public static class ReservedWords
{
    public const string OUTPUT = "output";
    public const string INPUT = "input";
    public const string VOID = "void";
    public const string DO = "do";
    public const string WHILE = "while";
    public const string FOR = "for";
    public const string IF = "if";
    public const string ELSE = "else";
    public const string RETURN = "return";

    public static readonly HashSet<string> Words = new HashSet<string>
    {
        OUTPUT, INPUT, VOID, DO, WHILE, FOR, IF, ELSE, RETURN
    };
}
