namespace KaizenLang.Core;

public partial class Parser
{
    private List<Token>? _currentTokens;

    public Node Parse(List<Token> tokens)
    {
        KaizenLang.Core.Logging.Logger.Debug($"Parser.Parse START - tokens: {tokens?.Count ?? 0}");
        _currentTokens = tokens; // Guardar para uso en ErrorNode
        parseDepth = 0;
        nestingErrorReported = false;
        int pos = 0;
        Node root = new Node { Type = "Program" };
        if (tokens == null || tokens.Count == 0)
            return root;

        root.Line = tokens[0].Line;
        root.Column = tokens[0].Column;
        while (pos < tokens.Count)
        {
            KaizenLang.Core.Logging.Logger.Debug($"Parser.Parse loop pos={pos} token='{tokens[pos].Value}' ({tokens[pos].Type})");
            root.Children.Add(ParseStatementAdvancing(tokens, ref pos, "Token inesperado fuera de lugar: '{0}'"));
        }

        CheckTreeDepth(root);
        return root;
    }

    // Profundidad máxima del árbol de una sentencia. Las cadenas largas (1 + 1 + 1 + ...) se parsean
    // sin recursión pero forman árboles igual de profundos, y el análisis semántico los recorre de forma
    // recursiva: sin este tope, un texto pegado podía agotar la pila en el navegador.
    public const int MaxTreeDepth = 150;

    private static void CheckTreeDepth(Node root)
    {
        foreach (var statement in root.Children.ToList())
        {
            if (statement.Depth() <= MaxTreeDepth)
                continue;
            var located = statement.DescendantsAndSelf().FirstOrDefault(n => n.Line > 0);
            root.Children.Add(CreateError(
                $"Expresión demasiado larga o anidada: supera los {MaxTreeDepth} niveles del árbol sintáctico.",
                located?.Line ?? 1,
                located?.Column ?? 1));
        }
    }

    // Parsea una sentencia y garantiza que pos avance. Si la sentencia falla, se recupera saltando
    // hasta el final de la sentencia, para reportar un solo error en vez de uno por cada token restante.
    private Node ParseStatementAdvancing(List<Token> tokens, ref int pos, string unexpectedTokenFormat)
    {
        int before = pos;
        var node = ParseStatement(tokens, ref pos);
        if (node == null)
        {
            node = ErrorNode(string.Format(unexpectedTokenFormat, tokens[pos].Value), pos);
            pos++;
        }
        else if (pos == before)
        {
            pos++;
        }

        if (node.Type == Node.ErrorType && !Equals(node.Value, MissingTerminatorMarker))
            SkipToStatementEnd(tokens, ref pos);
        return node;
    }

    // Recuperación de errores: avanza hasta después del próximo ';'. Si encuentra 'ying', salta ese
    // bloque completo; si encuentra 'yang', se detiene sin consumirlo porque cierra el bloque que lo contiene.
    private static void SkipToStatementEnd(List<Token> tokens, ref int pos)
    {
        while (pos < tokens.Count)
        {
            if (IsDelimiter(tokens, pos, DelimiterWords.SEMICOLON))
            {
                pos++;
                return;
            }
            if (IsDelimiter(tokens, pos, DelimiterWords.BLOCK_END))
                return;
            if (IsDelimiter(tokens, pos, DelimiterWords.BLOCK_START))
            {
                int depth = 0;
                do
                {
                    if (IsDelimiter(tokens, pos, DelimiterWords.BLOCK_START))
                        depth++;
                    else if (IsDelimiter(tokens, pos, DelimiterWords.BLOCK_END))
                        depth--;
                    pos++;
                }
                while (pos < tokens.Count && depth > 0);
                return;
            }
            pos++;
        }
    }
}
