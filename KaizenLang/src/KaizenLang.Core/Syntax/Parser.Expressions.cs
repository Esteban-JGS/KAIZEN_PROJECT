using System.Collections.Generic;

namespace KaizenLang.Core;

public partial class Parser
{
    private int GetPrecedence(string op)
    {
        switch (op)
        {
            case OperatorWords.OR:
                return 1;
            case OperatorWords.AND:
                return 2;
            case OperatorWords.EQUAL:
            case OperatorWords.NOT_EQUAL:
                return 3;
            case OperatorWords.LESS:
            case OperatorWords.LESS_EQUAL:
            case OperatorWords.GREATER:
            case OperatorWords.GREATER_EQUAL:
                return 4;
            case OperatorWords.ADD:
            case OperatorWords.SUBTRACT:
                return 5;
            case OperatorWords.MULTIPLY:
            case OperatorWords.DIVIDE:
            case OperatorWords.MODULO:
                return 6;
            default:
                return 0;
        }
    }

    private Node ParseExpression(List<Token> tokens, ref int pos)
    {
        var tooDeep = EnterNesting(tokens, ref pos);
        if (tooDeep != null)
            return tooDeep;
        try
        {
            return ParseExpressionWithPrecedence(tokens, ref pos, 0);
        }
        finally
        {
            parseDepth--;
        }
    }

    private Node ParseExpressionWithPrecedence(List<Token> tokens, ref int pos, int minPrecedence)
    {
        var left = ParseUnaryOrPrimary(tokens, ref pos);

        while (pos < tokens.Count)
        {
            if (tokens[pos].Type == "DELIMITER" &&
                (tokens[pos].Value == DelimiterWords.SEMICOLON || tokens[pos].Value == DelimiterWords.PAREN_CLOSE ||
                 tokens[pos].Value == DelimiterWords.COMMA || tokens[pos].Value == DelimiterWords.BRACKET_CLOSE || tokens[pos].Value == DelimiterWords.BLOCK_END))
                break;

            if (tokens[pos].Type != "OPERATOR")
                break;

            string op = tokens[pos].Value;

            if (op == OperatorWords.ASSIGN)
                break;

            int precedence = GetPrecedence(op);

            if (precedence < minPrecedence)
                break;

            var opToken = tokens[pos];
            pos++;

            var right = ParseExpressionWithPrecedence(tokens, ref pos, precedence + 1);

            var binaryNode = new Node { Type = "Expression" };
            binaryNode.Line = opToken.Line;
            binaryNode.Column = opToken.Column;
            binaryNode.Children.Add(left);

            var opNode = new Node { Type = "Operator", Children = { new Node { Type = op } } };
            opNode.Line = opToken.Line;
            opNode.Column = opToken.Column;
            binaryNode.Children.Add(opNode);
            binaryNode.Children.Add(right);

            left = binaryNode;
        }

        return left;
    }

    private Node ParseUnaryOrPrimary(List<Token> tokens, ref int pos)
    {
        if (pos < tokens.Count && tokens[pos].Type == "OPERATOR")
        {
            string opValue = tokens[pos].Value;
            if (opValue == OperatorWords.SUBTRACT || opValue == OperatorWords.NOT || opValue == OperatorWords.ADD)
            {
                var unaryNode = new Node { Type = "UnaryExpression" };
                unaryNode.Line = tokens[pos].Line;
                unaryNode.Column = tokens[pos].Column;

                var opNode = new Node { Type = "Operator", Children = { new Node { Type = opValue } } };
                opNode.Line = tokens[pos].Line;
                opNode.Column = tokens[pos].Column;
                unaryNode.Children.Add(opNode);
                pos++;

                // "- - - ... x" también es recursión del parser.
                var tooDeep = EnterNesting(tokens, ref pos);
                if (tooDeep != null)
                {
                    unaryNode.Children.Add(tooDeep);
                    return unaryNode;
                }
                try
                {
                    unaryNode.Children.Add(ParseUnaryOrPrimary(tokens, ref pos));
                }
                finally
                {
                    parseDepth--;
                }
                return unaryNode;
            }
        }

        if (pos + 1 < tokens.Count && tokens[pos].Type == "IDENTIFIER" && tokens[pos + 1].Type == "OPERATOR" &&
            (tokens[pos + 1].Value == OperatorWords.INCREMENT || tokens[pos + 1].Value == OperatorWords.DECREMENT))
        {
            var idTok = tokens[pos];
            var opTok = tokens[pos + 1];

            var targetIdentifier = new Node("IDENTIFIER", new List<Node> { new Node(idTok.Value) })
            {
                Line = idTok.Line,
                Column = idTok.Column
            };

            var lhsIdentifier = new Node("IDENTIFIER", new List<Node> { new Node(idTok.Value) })
            {
                Line = idTok.Line,
                Column = idTok.Column
            };

            var opSymbol = opTok.Value == OperatorWords.INCREMENT ? OperatorWords.ADD : OperatorWords.SUBTRACT;
            var opNode = new Node("Operator", new List<Node> { new Node(opSymbol) })
            {
                Line = opTok.Line,
                Column = opTok.Column
            };

            var literalOne = new Node("INT", new List<Node> { new Node("1") })
            {
                Line = opTok.Line,
                Column = opTok.Column
            };

            var arithmeticExpression = new Node("Expression", new List<Node> { lhsIdentifier, opNode, literalOne })
            {
                Line = idTok.Line,
                Column = idTok.Column
            };

            pos += 2;

            var assignment = new Node("Assignment", new List<Node> { targetIdentifier, arithmeticExpression })
            {
                Line = idTok.Line,
                Column = idTok.Column
            };

            return assignment;
        }

        if (pos < tokens.Count && tokens[pos].Type == "IDENTIFIER")
        {
            int scan = pos + 1;
            while (scan < tokens.Count && tokens[scan].Type == "DELIMITER" && tokens[scan].Value == "[")
            {
                scan++;
                while (scan < tokens.Count && !(tokens[scan].Type == "DELIMITER" && tokens[scan].Value == "]"))
                    scan++;
                if (scan < tokens.Count && tokens[scan].Type == "DELIMITER" && tokens[scan].Value == "]")
                    scan++;
                else
                    break;
            }

            if (scan < tokens.Count && tokens[scan].Type == "OPERATOR" && tokens[scan].Value == OperatorWords.ASSIGN)
            {
                Node lhs;
                if (pos + 1 < tokens.Count && tokens[pos + 1].Type == "DELIMITER" && tokens[pos + 1].Value == "[")
                {
                    lhs = ParseIndexAccess(tokens, ref pos);
                }
                else
                {
                    var idTok = tokens[pos];
                    lhs = new Node("IDENTIFIER", new List<Node> { new Node(idTok.Value) }) { Line = idTok.Line, Column = idTok.Column };
                    pos++;
                }

                if (pos < tokens.Count && tokens[pos].Type == "OPERATOR" && tokens[pos].Value == OperatorWords.ASSIGN)
                    pos++;

                var rhs = ParseExpression(tokens, ref pos);
                var assignNode = new Node("Assignment", new List<Node> { lhs, rhs }) { Line = lhs.Line, Column = lhs.Column };
                return assignNode;
            }
        }

        return ParsePrimaryExpression(tokens, ref pos);
    }

    private Node ParseFunctionCall(List<Token> tokens, ref int pos)
    {
        Node node = new Node { Type = "FunctionCall" };

        var fn = new Node { Type = "FunctionName", Children = { new Node { Type = tokens[pos].Value } } };
        fn.Line = tokens[pos].Line;
        fn.Column = tokens[pos].Column;
        node.Children.Add(fn);
        pos += 2; // nombre y '('

        var argsNode = new Node { Type = "Arguments" };
        ParseCommaSeparated(tokens, ref pos, DelimiterWords.PAREN_CLOSE, argsNode, "los argumentos de la llamada");
        node.Children.Add(argsNode);

        node.Line = fn.Line;
        node.Column = fn.Column;
        return node;
    }

    private Node ParseArrayLiteral(List<Token> tokens, ref int pos)
    {
        Node arrayNode = new Node { Type = "ArrayLiteral" };
        arrayNode.Line = tokens[pos].Line;
        arrayNode.Column = tokens[pos].Column;
        pos++; // '['

        var elementsNode = new Node { Type = "Elements" };
        ParseCommaSeparated(tokens, ref pos, DelimiterWords.BRACKET_CLOSE, elementsNode, "el array");
        arrayNode.Children.Add(elementsNode);
        return arrayNode;
    }

    // Lee "expr, expr, ..." hasta el delimitador de cierre y lo consume. Si encuentra algo que no
    // es una expresión, una coma o el cierre, agrega un nodo de error y se detiene: antes el bucle
    // reintentaba sin avanzar y entradas como "[1, 2;" colgaban el compilador.
    private void ParseCommaSeparated(List<Token> tokens, ref int pos, string closing, Node target, string context)
    {
        if (IsDelimiter(tokens, pos, closing))
        {
            pos++;
            return;
        }

        while (true)
        {
            int before = pos;
            var item = ParseExpression(tokens, ref pos);
            target.Children.Add(item);
            if (pos == before)
                return; // ParseExpression ya dejó un nodo de error

            if (IsDelimiter(tokens, pos, DelimiterWords.COMMA))
            {
                pos++;
                continue;
            }

            if (IsDelimiter(tokens, pos, closing))
            {
                pos++;
                return;
            }

            target.Children.Add(ErrorNode($"Se esperaba ',' o '{closing}' en {context}", pos));
            return;
        }
    }

    private Node ParseIndexAccess(List<Token> tokens, ref int pos)
    {
        if (pos >= tokens.Count || tokens[pos].Type != "IDENTIFIER")
            return new Node("Identifier") { Type = "IDENTIFIER" };

        var idTok = tokens[pos];
        var currentNode = new Node("IDENTIFIER", new List<Node> { new Node(idTok.Value) }) { Line = idTok.Line, Column = idTok.Column };
        pos++;

        while (IsDelimiter(tokens, pos, "["))
        {
            pos++;
            var idxExpr = ParseExpression(tokens, ref pos);
            if (IsDelimiter(tokens, pos, "]"))
            {
                pos++;
            }
            else
            {
                var err = ErrorNode("Se esperaba ']' en indexador", pos);
                var iaErr = new Node { Type = "IndexAccess", Line = idTok.Line, Column = idTok.Column };
                iaErr.Children.Add(currentNode);
                iaErr.Children.Add(idxExpr);
                iaErr.Children.Add(err);
                return iaErr;
            }

            var idxNode = new Node { Type = "IndexAccess", Line = idTok.Line, Column = idTok.Column };
            idxNode.Children.Add(currentNode);
            idxNode.Children.Add(idxExpr);
            currentNode = idxNode;
        }

        return currentNode;
    }

    private Node ParsePrimaryExpression(List<Token> tokens, ref int pos)
    {
        if (pos >= tokens.Count)
            return ErrorNode("Se esperaba una expresión", pos);

        if (pos < tokens.Count && tokens[pos].Type == "IDENTIFIER" &&
            pos + 1 < tokens.Count && tokens[pos + 1].Type == "DELIMITER" && tokens[pos + 1].Value == "(")
        {
            return ParseFunctionCall(tokens, ref pos);
        }

        if (pos < tokens.Count && tokens[pos].Type == "RESERVED" &&
            pos + 1 < tokens.Count && tokens[pos + 1].Type == "DELIMITER" && tokens[pos + 1].Value == "(")
        {
            return ParseFunctionCall(tokens, ref pos);
        }

        if (IsDelimiter(tokens, pos, "("))
        {
            pos++;
            var nested = ParseExpression(tokens, ref pos);
            var par = new Node { Type = "Parentheses", Children = { nested } };
            par.Line = tokens[Math.Max(0, pos - 1)].Line;
            par.Column = tokens[Math.Max(0, pos - 1)].Column;
            if (IsDelimiter(tokens, pos, ")"))
                pos++;
            else
                par.Children.Add(ErrorNode("Se esperaba ')' para cerrar el paréntesis", pos));
            return par;
        }

        if (IsDelimiter(tokens, pos, "["))
        {
            return ParseArrayLiteral(tokens, ref pos);
        }

        if (tokens[pos].Type == "IDENTIFIER" && pos + 1 < tokens.Count &&
            tokens[pos + 1].Type == "DELIMITER" && tokens[pos + 1].Value == "[")
        {
            return ParseIndexAccess(tokens, ref pos);
        }

        if (tokens[pos].Type == "IDENTIFIER")
        {
            var idTok = tokens[pos];
            var idNode = new Node("IDENTIFIER", new List<Node> { new Node(idTok.Value) });
            idNode.Line = idTok.Line;
            idNode.Column = idTok.Column;
            pos++;
            return idNode;
        }

        if (tokens[pos].Type == "INT" || tokens[pos].Type == "FLOAT" ||
            tokens[pos].Type == "BOOL" || tokens[pos].Type == "STRING" ||
            tokens[pos].Type == "LITERAL" || tokens[pos].Type == "CHAR")
        {
            var litTok = tokens[pos];
            var litNode = new Node(litTok.Type, new List<Node> { new Node(litTok.Value) });
            litNode.Line = litTok.Line;
            litNode.Column = litTok.Column;
            pos++;
            return litNode;
        }

        return ErrorNode($"Token inesperado: {tokens[pos].Type} '{tokens[pos].Value}'", pos);
    }
}
