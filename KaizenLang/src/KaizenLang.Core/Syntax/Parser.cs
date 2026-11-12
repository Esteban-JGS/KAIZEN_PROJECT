namespace KaizenLang.Core;

public partial class Parser
{
    private List<Token>? _currentTokens;

    public Node Parse(List<Token> tokens)
    {
        KaizenLang.Core.Logging.Logger.Debug($"Parser.Parse START - tokens: {tokens?.Count ?? 0}");
        _currentTokens = tokens; // Guardar para uso en ErrorNode
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
        return root;
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
