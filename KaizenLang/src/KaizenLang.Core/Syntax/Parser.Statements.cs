namespace KaizenLang.Core;

public partial class Parser
{
    private static bool IsCallStart(List<Token> tokens, int pos)
    {
        return pos + 1 < tokens.Count
            && (tokens[pos].Type == "IDENTIFIER" || tokens[pos].Type == "RESERVED")
            && IsDelimiter(tokens, pos + 1, DelimiterWords.PAREN_OPEN);
    }

    private Node? ParseStatement(List<Token> tokens, ref int pos)
    {
        if (Match(tokens, pos, "RESERVED", ReservedWords.IF))
            return ParseIf(tokens, ref pos);
        if (Match(tokens, pos, "RESERVED", ReservedWords.FOR))
            return ParseFor(tokens, ref pos);
        if (Match(tokens, pos, "RESERVED", ReservedWords.WHILE))
            return ParseWhile(tokens, ref pos);
        if (Match(tokens, pos, "RESERVED", ReservedWords.DO))
            return ParseDoWhile(tokens, ref pos);
        if (Match(tokens, pos, "RESERVED", ReservedWords.RETURN))
            return ParseReturn(tokens, ref pos);
        if (IsFunctionDeclaration(tokens, pos))
            return ParseFunction(tokens, ref pos);

        if (IsVariableDeclaration(tokens, pos))
            return ParseVariableDeclaration(tokens, ref pos);

        if (IsAssignment(tokens, pos))
            return ParseAssignment(tokens, ref pos);

        // Un tipo que no va seguido de un nombre ("gear = 10;", "gear;") es una declaración incompleta.
        if (pos < tokens.Count && tokens[pos].Type == "TYPE")
        {
            var error = ErrorNode($"Declaración incompleta: falta el nombre de la variable después de '{tokens[pos].Value}'.", pos + 1);
            pos++;
            return error;
        }

        // Una llamada "nombre(" se parsea como sentencia aunque esté mal cerrada, para que el error
        // señale el problema real (falta ')' o ';') en vez de "token inesperado" en el nombre.
        if (IsExpressionStatement(tokens, pos) || IsCallStart(tokens, pos))
            return ParseExpressionStatement(tokens, ref pos);

        return null;
    }
    private Node ParseReturn(List<Token> tokens, ref int pos)
    {
        var node = new Node("Return") { Line = tokens[pos].Line, Column = tokens[pos].Column };
        pos++;

        if (pos < tokens.Count && !(IsDelimiter(tokens, pos, DelimiterWords.SEMICOLON)))
        {
            var expr = ParseExpression(tokens, ref pos);
            node.Children.Add(expr);
        }

        if (IsDelimiter(tokens, pos, DelimiterWords.SEMICOLON))
            pos++;
        else
            return MissingSemicolon(node, "Se esperaba ';' después de la sentencia return.", pos);

        return node;
    }
    private Node ParseExpressionStatement(List<Token> tokens, ref int pos)
    {
        var exprNode = ParseExpression(tokens, ref pos);
        var stmtNode = new Node("ExpressionStatement");
        stmtNode.Children.Add(exprNode);

        if (IsDelimiter(tokens, pos, DelimiterWords.SEMICOLON))
            pos++;
        else
            return MissingSemicolon(stmtNode, "Se esperaba ';' después de la expresión.", pos);

        return stmtNode;
    }


    private Node ParseAssignment(List<Token> tokens, ref int pos)
    {
        var varToken = tokens[pos];
        var varNode = new Node("Identifier", new List<Node> { new Node(varToken.Value) }) { Line = varToken.Line, Column = varToken.Column };
        pos++;
        pos++;
        var valueNode = ParseExpression(tokens, ref pos);
        var assignmentNode = new Node("Assignment", new List<Node> { varNode, valueNode }) { Line = varNode.Line, Column = varNode.Column };

        if (IsDelimiter(tokens, pos, DelimiterWords.SEMICOLON))
            pos++;
        else
            return MissingSemicolon(assignmentNode, "Se esperaba ';' después de la asignación.", pos);

        return assignmentNode;
    }

    private Node ParseVariableDeclaration(List<Token> tokens, ref int pos)
    {

        Node typeNode;
        Node nameNode;

        if (tokens[pos].Type == "IDENTIFIER" && TypeWords.CompositeWrappers.Contains(tokens[pos].Value)
            && pos + 4 < tokens.Count
            && (tokens[pos + 1].Type == "DELIMITER" || tokens[pos + 1].Type == "OPERATOR") && tokens[pos + 1].Value == DelimiterWords.ANGLE_OPEN
            && tokens[pos + 2].Type == "TYPE"
            && (tokens[pos + 3].Type == "DELIMITER" || tokens[pos + 3].Type == "OPERATOR") && tokens[pos + 3].Value == DelimiterWords.ANGLE_CLOSE
            && tokens[pos + 4].Type == "IDENTIFIER")
        {
            var wrapper = tokens[pos];
            typeNode = new Node(wrapper.Value) { Line = wrapper.Line, Column = wrapper.Column };
            var inner = new Node(tokens[pos + 2].Value) { Line = tokens[pos + 2].Line, Column = tokens[pos + 2].Column };
            typeNode.Children.Add(inner);
            pos += 4;

            var nameToken = tokens[pos];
            nameNode = new Node("Identifier", new List<Node> { new Node(nameToken.Value) }) { Line = nameToken.Line, Column = nameToken.Column };
            pos++;
        }
        else if (tokens[pos].Type == "IDENTIFIER" && TypeWords.CompositeWrappers.Contains(tokens[pos].Value)
            && pos + 1 < tokens.Count && tokens[pos + 1].Type == "IDENTIFIER")
        {
            var message = $"Declaración de {tokens[pos].Value} requiere tipo de elemento explícito";
            pos += 2;
            return ErrorNode(message, pos - 2);
        }
        else if (tokens[pos].Type == "TYPE" && pos + 1 < tokens.Count && tokens[pos + 1].Type == "IDENTIFIER")
        {
            var typeToken = tokens[pos];
            typeNode = new Node(typeToken.Value) { Line = typeToken.Line, Column = typeToken.Column };
            pos++;
            var nameToken = tokens[pos];
            nameNode = new Node("Identifier", new List<Node> { new Node(nameToken.Value) }) { Line = nameToken.Line, Column = nameToken.Column };
            pos++;
        }
        else
        {
            return ErrorNode("Declaración de variable inválida", pos);
        }

        var declarationNode = new Node("VariableDeclaration", new List<Node> { typeNode, nameNode }) { Line = typeNode.Line, Column = typeNode.Column };

        if (pos < tokens.Count)
        {
            if (tokens[pos].Type == "OPERATOR" && tokens[pos].Value == OperatorWords.ASSIGN)
            {
                pos++;
                var valueNode = ParseExpression(tokens, ref pos);
                declarationNode.Children.Add(new Node("Value", new List<Node> { valueNode }));
            }
            else if (IsDelimiter(tokens, pos, DelimiterWords.SEMICOLON))
            {
            }
            else
            {
                var error = ErrorNode($"Token inesperado '{tokens[pos].Value}' en declaración de variable.", pos);
                pos++;
                return error;
            }

        }

        if (IsDelimiter(tokens, pos, DelimiterWords.SEMICOLON))
        {
            pos++;
        }
        else
        {
            return MissingSemicolon(declarationNode, "Se esperaba ';' al final de la declaración de variable.", pos);
        }

        return declarationNode;
    }
}
