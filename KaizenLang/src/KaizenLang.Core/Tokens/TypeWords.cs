namespace KaizenLang.Core;

public static class TypeWords
{
    public const string GEAR = "gear"; // INTEGER
    public const string SHIKAI = "shikai"; // FLOAT
    public const string BANKAI = "bankai"; // DOUBLE
    public const string SHIN = "shin"; // BOOL
    public const string GRIMOIRE = "grimoire"; // STRING

    public const string CHAINSAW = "chainsaw"; // ARRAY
    public const string HOGYOKU = "hogyoku"; // MATRIX

    public static readonly HashSet<string> Words = new HashSet<string>
    {
        GEAR,
        SHIKAI,
        BANKAI,
        SHIN,
        GRIMOIRE
    };

    public static readonly HashSet<string> CompositeWrappers = new HashSet<string>
    {
        CHAINSAW,
        HOGYOKU
    };
}
