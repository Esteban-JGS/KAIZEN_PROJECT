namespace KaizenLang.Core;

public partial class Interpreter
{
    private object? ExecuteIf(Node node)
    {
        if (node.Children.Count > 0)
        {
            var condition = ExecuteNode(node.Children[0]);
            var conditionBool = Convert.ToBoolean(condition);

            if (conditionBool && node.Children.Count > 1)
            {
                return ExecuteNode(node.Children[1]);
            }
            else if (!conditionBool && node.Children.Count > 2)
            {
                return ExecuteNode(node.Children[2]);
            }
        }
        return null;
    }

    // Tope de iteraciones por bucle para que un bucle infinito no congele el IDE (igual para while, for y do-while).
    public const int MaxLoopIterations = 10000;

    private void WarnLoopLimit(string loopName)
    {
        output.Add($"Advertencia: bucle {loopName} detenido después de {MaxLoopIterations} iteraciones");
    }

    private object? ExecuteWhile(Node node)
    {
        if (node.Children.Count >= 2)
        {
            var conditionNode = node.Children[0];
            var bodyNode = node.Children[1];

            int iterations = 0;
            while (Convert.ToBoolean(ExecuteNode(conditionNode)))
            {
                if (iterations++ >= MaxLoopIterations)
                {
                    WarnLoopLimit(ReservedWords.WHILE);
                    break;
                }
                ExecuteNode(bodyNode);
            }
        }
        return null;
    }

    // do ying ... yang while (cond); ejecuta el cuerpo al menos una vez. Antes no tenía caso propio en
    // ExecuteNode y caía en el default, que corría el cuerpo una sola vez sin volver a evaluar la condición.
    private object? ExecuteDoWhile(Node node)
    {
        if (node.Children.Count >= 2)
        {
            var bodyNode = node.Children[0];
            var conditionNode = node.Children[1];

            int iterations = 0;
            do
            {
                if (iterations++ >= MaxLoopIterations)
                {
                    WarnLoopLimit($"{ReservedWords.DO}-{ReservedWords.WHILE}");
                    break;
                }
                ExecuteNode(bodyNode);
            }
            while (Convert.ToBoolean(ExecuteNode(conditionNode)));
        }
        return null;
    }

    private object? ExecuteFor(Node node)
    {
        if (node.Children.Count >= 4)
        {
            var oldScope = currentScope;
            currentScope = new SymbolTable(currentScope);

            try
            {
                ExecuteNode(node.Children[0]);

                int iterations = 0;
                while (Convert.ToBoolean(ExecuteNode(node.Children[1])))
                {
                    if (iterations++ >= MaxLoopIterations)
                    {
                        WarnLoopLimit(ReservedWords.FOR);
                        break;
                    }
                    ExecuteNode(node.Children[3]);
                    ExecuteNode(node.Children[2]);
                }
            }
            finally
            {
                currentScope = oldScope;
            }
        }
        return null;
    }

    private object? ExecuteFunction(Node node)
    {
        if (node.Children.Count >= 2)
        {
            var nameNode = node.Children[1];
            if (nameNode.Children.Count > 0)
            {
                var functionName = nameNode.Children[0].Type;
                functions[functionName] = node;
                if (VerboseMode)
                    output.Add($"Función '{functionName}' definida");
            }
        }
        return null;
    }

    private object? ExecuteFunctionCall(Node node)
    {
        var fnameNode = node.FindChild("FunctionName");
        string fname = SemanticUtils.ExtractIdentifierName(fnameNode) ?? string.Empty;
        var callArgsNode = node.FindChild("Arguments");

        if (string.Equals(fname, ReservedWords.OUTPUT, System.StringComparison.OrdinalIgnoreCase))
        {
            var outputValues = new List<string>();
            if (callArgsNode != null && callArgsNode.Children.Count > 0)
            {
                foreach (var arg in callArgsNode.Children)
                {
                    outputValues.Add(FormatValue(ExecuteNode(arg)));
                }
            }
            output.Add(string.Join(" ", outputValues));
            return null;
        }

        if (string.Equals(fname, ReservedWords.INPUT, System.StringComparison.OrdinalIgnoreCase))
        {
            string? prompt = null;
            if (callArgsNode != null && callArgsNode.Children.Count > 0)
            {
                var firstArgNode = callArgsNode.Children[0];
                if (firstArgNode.Type == "IDENTIFIER")
                {
                    string varName;
                    if (firstArgNode.Children.Count > 0)
                        varName = firstArgNode.Children[0].Type;
                    else
                        varName = firstArgNode.Type;

                    var tokenLine = ReadNextInputToken(null);
                    if (tokenLine == null)
                        return null;

                    var symbol = currentScope.LookupVariable(varName);
                    object? finalVal = tokenLine;
                    if (symbol != null)
                    {
                        var targetType = ExtractPrimitiveType(symbol.Type);
                        finalVal = ConvertTokenToType(tokenLine, targetType);
                        currentScope.SetVariableValue(varName, finalVal);
                        if (VerboseMode)
                            output.Add($"Variable '{varName}' asignada con valor: {finalVal?.ToString() ?? "null"}");
                    }
                    else
                    {
                        return tokenLine;
                    }

                    return finalVal;
                }

                var firstArg = ExecuteNode(firstArgNode);
                prompt = firstArg?.ToString();
            }

            var token = ReadNextInputToken(prompt);
            return token;
        }

        if (string.Equals(fname, "length", System.StringComparison.OrdinalIgnoreCase))
        {
            return ExecuteLengthBuiltin(callArgsNode);
        }

        if (!string.IsNullOrEmpty(fname) && functions.ContainsKey(fname))
        {
            var fnNode = functions[fname];
            if ((currentCallDepth + 1) > maxCallDepth)
                throw new Exception($"Se excedió la profundidad máxima de recursión permitida ({maxCallDepth}).");

            // Los argumentos se evalúan en el alcance de quien llama y antes de crear el de la función.
            // Antes se evaluaban ya dentro del nuevo alcance: en f(b, a) con parámetros (a, b), el segundo
            // argumento leía el parámetro 'a' recién asignado en vez de la variable 'a' de quien llama.
            var argValues = new List<object?>();
            if (callArgsNode != null)
            {
                foreach (var argNode in callArgsNode.Children)
                    argValues.Add(ExecuteNode(argNode));
            }

            currentCallDepth++;
            var oldScope = currentScope;
            try
            {
                if (VerboseMode)
                {
                    output.Add($"[DBG] Invoking function '{fname}'");
                    KaizenLang.Core.Logging.Logger.Debug($"Invoking function '{fname}'");
                }

                // Alcance léxico: la función ve sus parámetros y las variables globales, no las locales de quien llama.
                currentScope = new SymbolTable(globalScope);

                var paramsNode = fnNode.FindChild("Parameters");
                if (paramsNode != null)
                {
                    for (int i = 0; i < paramsNode.Children.Count; i++)
                    {
                        var p = paramsNode.Children[i];
                        var paramName = SemanticUtils.ExtractIdentifierName(p.FindChild("Identifier"));
                        if (string.IsNullOrEmpty(paramName))
                            continue;

                        var declaredType = p.Children.Count > 0 ? ExtractTypeInfo(p.Children[0]).fullType : string.Empty;
                        var argValue = i < argValues.Count ? argValues[i] : null;
                        var coercedValue = CoerceValueToDeclaredType(argValue, declaredType);

                        currentScope.DeclareVariable(paramName, declaredType, 0);
                        currentScope.SetVariableValue(paramName, coercedValue);
                        if (VerboseMode)
                        {
                            output.Add($"[DBG] Param '{paramName}' = {FormatValue(coercedValue)} (declared: {declaredType})");
                            KaizenLang.Core.Logging.Logger.Debug($"Param '{paramName}' = {coercedValue} (declared: {declaredType})");
                        }
                    }
                }
                if (VerboseMode)
                {
                    output.Add($"[DBG] Executing body of '{fname}'");
                    KaizenLang.Core.Logging.Logger.Debug($"Executing body of '{fname}'");
                }
                var body = fnNode.FindChild("Block") ?? fnNode.Children.FirstOrDefault(c => c.Type == "Body");
                object? result = null;
                try
                {
                    if (body != null)
                        ExecuteNode(body);
                }
                catch (ReturnException rex)
                {
                    result = rex.Value;
                    if (VerboseMode)
                    {
                        output.Add($"[DBG] Function '{fname}' returned: {result}");
                        KaizenLang.Core.Logging.Logger.Debug($"Function '{fname}' returned: {result}");
                    }
                }

                if (VerboseMode)
                {
                    output.Add($"[DBG] Finished invocation of '{fname}' (returning)");
                    KaizenLang.Core.Logging.Logger.Debug($"Finished invocation of '{fname}' (returning)");
                }

                return result;
            }
            finally
            {
                currentCallDepth--;
                // restore to previous scope captured in oldScope
                currentScope = oldScope;
            }
        }

        throw new Exception($"Función '{fname}' no definida");
    }

    private object? ExecuteReturn(Node node)
    {
        // Return may have an expression child
        object? val = null;
        if (node.Children.Count > 0)
            val = ExecuteNode(node.Children[0]);
        throw new ReturnException(val);
    }

    private object? ExecuteBlock(Node node)
    {
        // Crear nuevo scope para el bloque
        var oldScope = currentScope;
        currentScope = new SymbolTable(currentScope);

        try
        {
            foreach (var child in node.Children)
                ExecuteNode(child);
        }
        finally
        {
            currentScope = oldScope;
        }
        return null;
    }
}
