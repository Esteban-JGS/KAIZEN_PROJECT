namespace KaizenLang.Core;

public static class OperatorWords
{
    public const string ADD = "+";
    public const string SUBTRACT = "-";
    public const string MULTIPLY = "*";
    public const string DIVIDE = "/";
    public const string MODULO = "%";

    public const string EQUAL = "==";
    public const string NOT_EQUAL = "!=";
    public const string LESS = "<";
    public const string GREATER = ">";
    public const string LESS_EQUAL = "<=";
    public const string GREATER_EQUAL = ">=";

    public const string AND = "&&";
    public const string OR = "||";
    public const string NOT = "!";

    public const string ASSIGN = "=";
    public const string ADD_ASSIGN = "+=";
    public const string SUB_ASSIGN = "-=";
    public const string MUL_ASSIGN = "*=";
    public const string DIV_ASSIGN = "/=";

    public const string INCREMENT = "++";
    public const string DECREMENT = "--";

    public static readonly HashSet<string> Words = new HashSet<string>
    {
        ADD, SUBTRACT, MULTIPLY, DIVIDE, MODULO,
        EQUAL, NOT_EQUAL, LESS, GREATER, LESS_EQUAL, GREATER_EQUAL,
        AND, OR, NOT,
        ASSIGN, ADD_ASSIGN, SUB_ASSIGN, MUL_ASSIGN, DIV_ASSIGN,
        INCREMENT, DECREMENT
    };
}
