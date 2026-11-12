using System.Linq;

namespace KaizenLang.Core;

public partial class Interpreter
{
    private SymbolTable globalScope;
    private SymbolTable currentScope;
    private List<string> output;
    private Dictionary<string, Node> functions;
    private readonly Func<string?, string?>? inputProvider;
    private readonly Queue<string> inputBuffer = new Queue<string>();
    private readonly object inputLock = new object();
    private int currentCallDepth;
    private int maxCallDepth;

    public bool VerboseMode { get; set; } = false;

    public int MaxCallDepth
    {
        get => maxCallDepth;
        set
        {
            if (value < 1)
                throw new ArgumentOutOfRangeException(nameof(value), "MaxCallDepth debe ser mayor que cero.");
            maxCallDepth = value;
        }
    }

    public Interpreter() : this(null) { }

    public Interpreter(Func<string?, string?>? inputProvider)
    {
        this.inputProvider = inputProvider;
        globalScope = new SymbolTable();
        currentScope = globalScope;
        output = new List<string>();
        functions = new Dictionary<string, Node>(StringComparer.OrdinalIgnoreCase);
        currentCallDepth = 0;
        maxCallDepth = 100;
    }

    // Internal exception used to unwind execution when a 'return' is executed inside a function
    private class ReturnException : Exception
    {
        public object? Value { get; }
        public ReturnException(object? value)
        {
            Value = value;
        }
    }

    // Tokenize an input line into whitespace-separated tokens, respecting quoted strings
    private static List<string> TokenizeInputLine(string? line)
    {
        var tokens = new List<string>();
        if (string.IsNullOrEmpty(line))
            return tokens;

        bool inQuote = false;
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < line.Length; i++)
        {
            var c = line[i];
            if (c == '"')
            {
                inQuote = !inQuote;
                continue; // drop quote chars
            }
            if (!inQuote && char.IsWhiteSpace(c))
            {
                if (sb.Length > 0)
                {
                    tokens.Add(sb.ToString());
                    sb.Clear();
                }
            }
            else
            {
                sb.Append(c);
            }
        }
        if (sb.Length > 0)
            tokens.Add(sb.ToString());
        return tokens;
    }

    // Ensure inputBuffer has tokens: if empty, ask inputProvider for a line.
    // If no inputProvider is supplied, attempt a Console.ReadLine with a short timeout
    // so running snippets in non-interactive contexts doesn't hang indefinitely.
    private string? ReadNextInputToken(string? prompt)
    {
        lock (inputLock)
        {
            if (inputBuffer.Count == 0)
            {
                try
                {
                    string? line = null;

                    if (inputProvider != null)
                    {
                        // UI-driven provider (modal prompt) - call synchronously
                        line = inputProvider.Invoke(prompt);
                    }
                    else
                    {
                        // No provider: attempt Console.ReadLine but with timeout to avoid blocking.
                        try
                        {
                            var readTask = System.Threading.Tasks.Task.Run(() => Console.ReadLine());
                            // Wait a short timeout (2 seconds) to avoid long blocking during tests
                            if (readTask.Wait(TimeSpan.FromSeconds(2)))
                                line = readTask.Result;
                            else
                                line = null; // timeout
                        }
                        catch
                        {
                            line = null;
                        }
                    }

                    if (line == null)
                        return null;

                    var toks = TokenizeInputLine(line);
                    foreach (var t in toks)
                        inputBuffer.Enqueue(t);
                }
                catch
                {
                    return null;
                }
            }

            if (inputBuffer.Count == 0)
                return null;
            return inputBuffer.Dequeue();
        }
    }

    // Convert a token string to the expected semantic type (e.g., integer, float, bool)
    private object? ConvertTokenToType(string? token, string expectedType)
    {
        if (token == null)
            return null;
        if (string.IsNullOrEmpty(expectedType))
            return token;
        switch (expectedType)
        {
            case TypeWords.GEAR:
                if (int.TryParse(token, out var i))
                    return i;
                break;
            case TypeWords.SHIKAI:
            case TypeWords.BANKAI:
                if (double.TryParse(token, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var d))
                    return d;
                break;
            case TypeWords.SHIN:
                if (bool.TryParse(token, out var b))
                    return b;
                var low = token.ToLowerInvariant();
                if (low == "1" || low == LiteralWords.TRUE)
                    return true;
                if (low == "0" || low == LiteralWords.FALSE)
                    return false;
                break;
            case TypeWords.GRIMOIRE:
                return token;
            default:
                return token;
        }
        return token;
    }

    private object? CoerceValueToDeclaredType(object? value, string declaredType)
    {
        if (value == null)
            return null;
        if (string.IsNullOrWhiteSpace(declaredType))
            return value;

        var trimmed = declaredType.Trim();
        var lower = trimmed.ToLowerInvariant();

        // Do not coerce composite collection wrappers
        if (lower.StartsWith(TypeWords.CHAINSAW, System.StringComparison.OrdinalIgnoreCase) ||
            lower.StartsWith(TypeWords.HOGYOKU, System.StringComparison.OrdinalIgnoreCase))
            return value;

        var primitive = ExtractPrimitiveType(trimmed);
        var primitiveLower = primitive.Trim().ToLowerInvariant();

        if (primitiveLower.StartsWith(TypeWords.CHAINSAW, System.StringComparison.OrdinalIgnoreCase) ||
            primitiveLower.StartsWith(TypeWords.HOGYOKU, System.StringComparison.OrdinalIgnoreCase))
            return value;

        try
        {
            switch (primitiveLower)
            {
                case TypeWords.GEAR:
                    if (value is int)
                        return value;
                    if (value is bool boolVal)
                        return boolVal ? 1 : 0;
                    if (value is double doubleVal)
                        return (int)System.Math.Truncate(doubleVal);
                    if (value is float floatVal)
                        return (int)System.Math.Truncate(floatVal);
                    if (value is decimal decimalVal)
                        return (int)System.Math.Truncate(decimalVal);
                    if (value is long or short)
                        return Convert.ToInt32(value, System.Globalization.CultureInfo.InvariantCulture);
                    if (value is string strInt)
                    {
                        if (int.TryParse(strInt, out var parsedInt))
                            return parsedInt;
                        if (double.TryParse(strInt, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var parsedDoubleFromString))
                            return (int)System.Math.Truncate(parsedDoubleFromString);
                    }
                    break;

                case TypeWords.SHIKAI:
                case TypeWords.BANKAI:
                    if (value is double || value is float)
                        return Convert.ToDouble(value, System.Globalization.CultureInfo.InvariantCulture);
                    if (value is int or long or short or decimal)
                        return Convert.ToDouble(value, System.Globalization.CultureInfo.InvariantCulture);
                    if (value is string strDouble && double.TryParse(strDouble, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var parsedDouble))
                        return parsedDouble;
                    break;

                case TypeWords.SHIN:
                    if (value is bool)
                        return value;
                    if (value is int intVal)
                        return intVal != 0;
                    if (value is double boolDouble)
                        return System.Math.Abs(boolDouble) > double.Epsilon;
                    if (value is string strBool)
                    {
                        if (bool.TryParse(strBool, out var parsedBool))
                            return parsedBool;
                        var lowered = strBool.Trim().ToLowerInvariant();
                        if (lowered == "1" || lowered == LiteralWords.TRUE)
                            return true;
                        if (lowered == "0" || lowered == LiteralWords.FALSE)
                            return false;
                    }
                    break;

                case TypeWords.GRIMOIRE:
                    return value.ToString() ?? string.Empty;
            }
        }
        catch
        {
            // If conversion fails, fall back to original value.
        }

        return value;
    }

    private (string fullType, string primitiveType) ExtractTypeInfo(Node? typeNode)
    {
        if (typeNode == null)
            return (string.Empty, string.Empty);

        if (typeNode.Children == null || typeNode.Children.Count == 0)
            return (typeNode.Type, typeNode.Type);

        var childInfos = typeNode.Children.Select(ExtractTypeInfo).ToList();
        var fullChildren = childInfos.Select(info => info.fullType);
        var primitive = childInfos.Last().primitiveType;
        var fullType = string.Format("{0}<{1}>", typeNode.Type, string.Join(",", fullChildren));
        return (fullType, primitive);
    }

    private static string ExtractPrimitiveType(string declaredType)
    {
        if (string.IsNullOrWhiteSpace(declaredType))
            return declaredType;

        var trimmed = declaredType.Trim();
        var lt = trimmed.IndexOf('<');
        if (lt >= 0)
        {
            var gt = trimmed.LastIndexOf('>');
            if (gt > lt)
            {
                var inner = trimmed.Substring(lt + 1, gt - lt - 1).Trim();
                if (inner.Contains(','))
                    return inner.Split(',').Last().Trim();
                return inner;
            }
        }

        return trimmed;
    }

    public List<string> Execute(Node root)
    {
        output.Clear();
        executedSteps = 0;
        try
        {
            ExecuteNode(root);
        }
        catch (Exception ex)
        {
            output.Add($"Error de ejecución: {ex.Message}");
        }
        return output;
    }

    // Límite opcional de nodos evaluados por ejecución (0 = sin límite). Lo usa el playground web, donde no hay
    // un timeout como el del IDE y un programa con bucles anidados podría congelar la pestaña del navegador.
    public long MaxExecutionSteps { get; set; }
    private long executedSteps;

    private object? ExecuteNode(Node node)
    {
        if (MaxExecutionSteps > 0 && ++executedSteps > MaxExecutionSteps)
            throw new Exception($"Se alcanzó el límite de {MaxExecutionSteps} pasos de ejecución (¿bucle muy largo o infinito?)");

        switch (node.Type)
        {
            case "Program":
                foreach (var child in node.Children)
                    ExecuteNode(child);
                return null;

            case "VariableDeclaration":
                return ExecuteVariableDeclaration(node);

            case "Assignment":
                return ExecuteAssignment(node);

            case "Expression":
                return ExecuteExpression(node);

            case "UnaryExpression":
                return ExecuteUnaryExpression(node);

            case "Parentheses":
                // Evaluate the expression inside the parentheses
                if (node.Children.Count > 0)
                    return ExecuteNode(node.Children[0]);
                return null;

            case "If":
                return ExecuteIf(node);

            case "While":
                return ExecuteWhile(node);

            case "DoWhile":
                return ExecuteDoWhile(node);

            case "For":
                return ExecuteFor(node);

            case "Function":
                return ExecuteFunction(node);

            case "FunctionCall":
                // Evaluate a function call expression: either builtin handled in ExecuteExpression
                // or user-defined function lookup and invocation
                return ExecuteFunctionCall(node);

            case "Return":
                return ExecuteReturn(node);

            case "Block":
                return ExecuteBlock(node);

            // Tipos básicos
            case "INT":
                // Parsear como int para mantener el tipo integer
                if (int.TryParse(node.Children[0].Type, out var intValue))
                    return intValue;
                // Si es muy grande, intentar long
                if (long.TryParse(node.Children[0].Type, out var longValue))
                    return longValue;
                throw new Exception($"No se pudo parsear el entero: {node.Children[0].Type}");

            case "FLOAT":
                // Parsear como double para mantener precisión completa
                // El tipo semántico "float" o "double" se maneja en la tabla de símbolos
                if (double.TryParse(node.Children[0].Type, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out var doubleValue))
                    return doubleValue;
                throw new Exception($"No se pudo parsear el número decimal: {node.Children[0].Type}");

            case "STRING":
                return node.Children[0].Type;

            case "LITERAL":
                var literal = node.Children[0].Type;
                if (literal == LiteralWords.TRUE)
                    return true;
                if (literal == LiteralWords.FALSE)
                    return false;
                if (literal == LiteralWords.NULL)
                    return null;
                return literal;

            case "IDENTIFIER":
                var varName = node.Children[0].Type;
                var symbol = currentScope.LookupVariable(varName);
                if (symbol == null)
                {
                    // Antes de lanzar una excepción, verificar si es una función predefinida
                    if (varName == ReservedWords.OUTPUT)
                    {
                        return ReservedWords.OUTPUT; // Devolver un identificador especial para la función 'output'
                    }
                    throw new Exception($"Variable o función '{varName}' no está declarada");
                }
                if (!symbol.IsInitialized)
                {
                    // Opcional: advertir o lanzar error si se usa una variable no inicializada
                    // output.Add($"Advertencia: La variable '{varName}' se está usando sin haber sido inicializada.");
                }
                return symbol.Value;

            case "ArrayLiteral":
                // Construct a runtime list from array literal elements
                var elementsNode = node.Children.FirstOrDefault(c => c.Type == "Elements");
                var list = new List<object?>();
                if (elementsNode != null)
                {
                    foreach (var childExpr in elementsNode.Children)
                    {
                        var v = ExecuteNode(childExpr);
                        list.Add(v);
                    }
                }
                return list;

            case "IndexAccess":
                // children[0] = target (identifier or nested index), children[1] = index expression
                if (node.Children.Count >= 2)
                {
                    var target = ExecuteNode(node.Children[0]);
                    var idx = ExecuteNode(node.Children[1]);
                    if (target is System.Collections.IList listTarget && idx is int i)
                    {
                        if (i < 0 || i >= listTarget.Count)
                            throw new Exception($"Index fuera de rango: {i}");
                        return listTarget[i];
                    }
                    else
                    {
                        throw new Exception($"Operación de indexación no soportada para tipo {target?.GetType().Name}");
                    }
                }
                return null;

            default:
                // Para nodos como 'ExpressionStatement', simplemente ejecutamos sus hijos
                foreach (var child in node.Children)
                    ExecuteNode(child);
                return null;
        }
    }
}
