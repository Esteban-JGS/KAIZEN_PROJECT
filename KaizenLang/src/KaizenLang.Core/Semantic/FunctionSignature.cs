using System;

namespace KaizenLang.Core;

public class FunctionSignature
{
    public string Name { get; set; } = string.Empty;
    public int Arity { get; set; }
    public bool IsBuiltin { get; set; }
    // Optional return type (e.g., "integer", "string", etc.)
    public string ReturnType { get; set; } = string.Empty;
}
