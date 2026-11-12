namespace KaizenLang.Core;

public class Symbol
{
    public string Name { get; set; }
    public string Type { get; set; }
    public object? Value { get; set; }
    public bool IsInitialized { get; set; }
    public int Line { get; set; }

    public Symbol(string name, string type, int line)
    {
        Name = name;
        Type = type;
        Line = line;
        IsInitialized = false;
    }
}
