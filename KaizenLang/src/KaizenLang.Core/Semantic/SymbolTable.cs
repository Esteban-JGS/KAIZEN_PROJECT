using System.Collections.Generic;
using System.Linq;

namespace KaizenLang.Core;

public class SymbolTable
{
    private Dictionary<string, Symbol> symbols;
    private SymbolTable? parent;

    public SymbolTable(SymbolTable? parent = null)
    {
        symbols = new Dictionary<string, Symbol>();
        this.parent = parent;
    }

    public bool DeclareVariable(string name, string type, int line)
    {
        if (symbols.ContainsKey(name))
            return false; // Variable ya declarada en este scope

        symbols[name] = new Symbol(name, type, line);
        return true;
    }

    public Symbol? LookupVariable(string name)
    {
        if (symbols.ContainsKey(name))
            return symbols[name];

        return parent?.LookupVariable(name);
    }

    public bool SetVariableValue(string name, object? value)
    {
        var symbol = LookupVariable(name);
        if (symbol == null)
            return false;

        symbol.Value = value;
        symbol.IsInitialized = true;
        return true;
    }

    public List<Symbol> GetAllSymbols()
    {
        return symbols.Values.ToList();
    }
}
