namespace KaizenLang.Core;

public partial class Parser
{
    // Nodo de error ubicado en el token 'pos' (o en el último, si el archivo terminó antes).
    private Node ErrorNode(string mensaje, int pos)
    {
        int line = 1, column = 1;
        if (_currentTokens != null && _currentTokens.Count > 0)
        {
            var token = _currentTokens[Math.Min(pos, _currentTokens.Count - 1)];
            line = token.Line;
            column = token.Column;
        }
        return CreateError(mensaje, line, column);
    }

    // Para faltantes como ';': el error se ubica justo después del token anterior y no en el siguiente,
    // que puede estar en otra línea y confundir ("falta ';'" señalando la línea de abajo).
    private Node ErrorAfterPrevious(string mensaje, int pos)
    {
        if (_currentTokens == null || pos <= 0 || pos > _currentTokens.Count)
            return ErrorNode(mensaje, pos);

        var previous = _currentTokens[pos - 1];
        var length = previous.Value.Length + (previous.Type == "STRING" ? 2 : 0); // comillas no incluidas en Value
        var error = CreateError(mensaje, previous.Line, previous.Column + length);
        error.Value = MissingTerminatorMarker;
        return error;
    }

    // Marca los errores de "falta X al final": la sentencia ya se leyó completa y no hay que saltar tokens.
    private const string MissingTerminatorMarker = "missing-terminator";

    // Falta ';' al final de una sentencia. Si la sentencia ya tenía un error propio (p. ej. "[1 2 3]"),
    // ese error explica mejor el problema y se devuelve en lugar del de ';'.
    private Node MissingSemicolon(Node statement, string mensaje, int pos)
    {
        return FirstError(statement) ?? ErrorAfterPrevious(mensaje, pos);
    }

    private static Node? FirstError(Node node)
    {
        return node.DescendantsAndSelf().FirstOrDefault(n => n.Type == Node.ErrorType);
    }

    // Tope de anidamiento del parser (paréntesis, corchetes, operadores unarios y bloques). El parser
    // es recursivo: en el playground web, unos 150 paréntesis anidados agotaban la pila de WebAssembly.
    public const int MaxNestingDepth = 50;
    private int parseDepth;

    // Entra a un nivel de anidamiento. Si se pasa del tope devuelve un error, y antes salta el resto de la
    // construcción anidada para que los niveles exteriores encuentren su cierre y no se repita el error.
    private Node? EnterNesting(List<Token> tokens, ref int pos)
    {
        if (parseDepth < MaxNestingDepth)
        {
            parseDepth++;
            return null;
        }

        // Un solo mensaje por programa: si la condición de un if ya superó el tope, su bloque también lo
        // superará; el segundo nodo no es un error para no repetir el mensaje (la compilación ya falló).
        var result = nestingErrorReported
            ? new Node("Empty")
            : ErrorNode($"Código demasiado anidado: se admiten hasta {MaxNestingDepth} niveles de paréntesis, corchetes, operadores unarios o bloques.", pos);
        nestingErrorReported = true;
        SkipNestedRemainder(tokens, ref pos);
        return result;
    }

    private bool nestingErrorReported;

    // Avanza hasta el cierre que corresponde al nivel actual (sin consumirlo) o hasta un ';' de ese nivel.
    private static void SkipNestedRemainder(List<Token> tokens, ref int pos)
    {
        int depth = 0;
        while (pos < tokens.Count)
        {
            var token = tokens[pos];
            if (token.Type == "DELIMITER")
            {
                if (token.Value is DelimiterWords.PAREN_OPEN or DelimiterWords.BRACKET_OPEN or DelimiterWords.BLOCK_START)
                {
                    depth++;
                }
                else if (token.Value is DelimiterWords.PAREN_CLOSE or DelimiterWords.BRACKET_CLOSE or DelimiterWords.BLOCK_END)
                {
                    if (depth == 0)
                        return;
                    depth--;
                }
                else if (token.Value == DelimiterWords.SEMICOLON && depth == 0)
                {
                    return;
                }
            }
            pos++;
        }
    }

    private static Node CreateError(string mensaje, int line, int column)
    {
        var n = new Node { Type = Node.ErrorType, Line = line, Column = column };
        n.Children.Add(new Node { Type = $"{mensaje} (línea {line}, columna {column})" });
        return n;
    }

    private bool Match(List<Token> tokens, int pos, string type, string value)
    {
        return pos < tokens.Count && tokens[pos].Type == type && tokens[pos].Value == value;
    }

    // Verifica límites antes de leer: el código fuente puede terminar en cualquier punto (p. ej. "while" al final).
    private static bool IsDelimiter(List<Token> tokens, int pos, string value)
    {
        return pos < tokens.Count && tokens[pos].Type == "DELIMITER" && tokens[pos].Value == value;
    }

    private bool IsFunctionDeclaration(List<Token> tokens, int pos)
    {
        return pos + 3 < tokens.Count && (tokens[pos].Type == "TYPE" || tokens[pos].Type == "RESERVED") && tokens[pos + 1].Type == "IDENTIFIER" && tokens[pos + 2].Type == "DELIMITER" && tokens[pos + 2].Value == "(";
    }

    private bool IsAssignment(List<Token> tokens, int pos)
    {
        return pos + 2 < tokens.Count &&
               tokens[pos].Type == "IDENTIFIER" &&
               tokens[pos + 1].Type == "OPERATOR" &&
               tokens[pos + 1].Value == "=";
    }

    private bool IsVariableDeclaration(List<Token> tokens, int pos)
    {
        if (pos + 1 >= tokens.Count)
            return false;

        if (tokens[pos].Type == "TYPE" && tokens[pos + 1].Type == "IDENTIFIER")
            return true;

        if (tokens[pos].Type == "IDENTIFIER" && TypeWords.CompositeWrappers.Contains(tokens[pos].Value))
        {
            if (pos + 4 < tokens.Count
                && (tokens[pos + 1].Type == "DELIMITER" || tokens[pos + 1].Type == "OPERATOR") && tokens[pos + 1].Value == DelimiterWords.ANGLE_OPEN
                && tokens[pos + 2].Type == "TYPE"
                && (tokens[pos + 3].Type == "DELIMITER" || tokens[pos + 3].Type == "OPERATOR") && tokens[pos + 3].Value == DelimiterWords.ANGLE_CLOSE
                && tokens[pos + 4].Type == "IDENTIFIER")
            {
                return true;
            }
            if (pos + 1 < tokens.Count && tokens[pos + 1].Type == "IDENTIFIER")
                return true;
        }

        return false;
    }

    private bool IsExpressionStatement(List<Token> tokens, int pos)
    {
        if (pos >= tokens.Count)
            return false;
        if (IsDelimiter(tokens, pos, DelimiterWords.BLOCK_END))
            return false;
        int tempPos = pos;
        int parenLevel = 0;
        while (tempPos < tokens.Count)
        {
            if (tokens[tempPos].Type == "DELIMITER")
            {
                if (tokens[tempPos].Value == DelimiterWords.PAREN_OPEN)
                    parenLevel++;
                else if (tokens[tempPos].Value == DelimiterWords.PAREN_CLOSE)
                    parenLevel--;
                else if (tokens[tempPos].Value == DelimiterWords.SEMICOLON && parenLevel == 0)
                    return true;
                else if (tokens[tempPos].Value == DelimiterWords.BLOCK_START && parenLevel == 0)
                    return false;
            }
            tempPos++;
        }
        return false;
    }
    private Node ParseParams(List<Token> tokens, ref int pos)
    {
        var parameters = new Node("Parameters");
        while (pos < tokens.Count && !IsDelimiter(tokens, pos, DelimiterWords.PAREN_CLOSE))
        {
            if (tokens[pos].Type == "TYPE" || (tokens[pos].Type == "IDENTIFIER" && TypeWords.CompositeWrappers.Contains(tokens[pos].Value)))
            {
                Node typeNode = new Node("Unknown");

                if (tokens[pos].Type == "TYPE")
                {
                    var typeToken = tokens[pos];
                    typeNode = new Node(typeToken.Value) { Line = typeToken.Line, Column = typeToken.Column };
                    pos++;
                }
                else
                {
                    var wrapper = tokens[pos];
                    typeNode = new Node(wrapper.Value) { Line = wrapper.Line, Column = wrapper.Column };
                    pos++;
                    if (!(pos < tokens.Count && (tokens[pos].Type == "DELIMITER" || tokens[pos].Type == "OPERATOR") && tokens[pos].Value == DelimiterWords.ANGLE_OPEN))
                    {
                        parameters.Children.Add(ErrorNode("Se esperaba '<' en tipo compuesto", pos));
                        break;
                    }
                    pos++;
                    if (!(pos < tokens.Count && tokens[pos].Type == "TYPE"))
                    {
                        parameters.Children.Add(ErrorNode("Se esperaba tipo base dentro de '<>'", pos));
                        break;
                    }
                    var inner = new Node(tokens[pos].Value) { Line = tokens[pos].Line, Column = tokens[pos].Column };
                    typeNode.Children.Add(inner);
                    pos++;
                    if (!(pos < tokens.Count && (tokens[pos].Type == "DELIMITER" || tokens[pos].Type == "OPERATOR") && tokens[pos].Value == DelimiterWords.ANGLE_CLOSE))
                    {
                        parameters.Children.Add(ErrorNode("Se esperaba '>' en tipo compuesto", pos));
                        break;
                    }
                    pos++;
                }

                if (pos < tokens.Count && tokens[pos].Type == "IDENTIFIER")
                {
                    var nameToken = tokens[pos];
                    var nameNode = new Node("Identifier", new List<Node> { new Node(nameToken.Value) }) { Line = nameToken.Line, Column = nameToken.Column };
                    var paramNode = new Node("Param", new List<Node> { typeNode, nameNode }) { Line = typeNode.Line, Column = typeNode.Column };
                    parameters.Children.Add(paramNode);
                    pos++;
                    if (IsDelimiter(tokens, pos, DelimiterWords.COMMA))
                        pos++;
                    else if (!IsDelimiter(tokens, pos, DelimiterWords.PAREN_CLOSE))
                    {
                        parameters.Children.Add(ErrorNode("Se esperaba ',' o ')' después del parámetro", pos));
                        break;
                    }
                }
                else
                {
                    parameters.Children.Add(ErrorNode("Falta nombre de parámetro", pos));
                    break;
                }
            }
            else if (tokens[pos].Type == "IDENTIFIER")
            {
                parameters.Children.Add(ErrorNode($"El parámetro '{tokens[pos].Value}' necesita un tipo", pos));
                break;
            }
            else if (IsDelimiter(tokens, pos, DelimiterWords.COMMA))
            {
                return ErrorNode("Coma inesperada o parámetro faltante.", pos);
            }
            else
            {
                parameters.Children.Add(ErrorNode($"Token inesperado '{tokens[pos].Value}' en parámetros", pos));
                pos++;
                break;
            }
        }
        return parameters;
    }
}
