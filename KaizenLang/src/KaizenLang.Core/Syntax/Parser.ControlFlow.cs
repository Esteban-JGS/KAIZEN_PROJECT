using System.Collections.Generic;

namespace KaizenLang.Core;

public partial class Parser
{
    private Node ParseIf(List<Token> tokens, ref int pos)
    {
        var ifNode = new Node("If");
        pos++;

        if (IsDelimiter(tokens, pos, DelimiterWords.PAREN_OPEN))
            pos++;
        else
            return ErrorNode("Se esperaba '(' después de 'if'.", pos);

        var condition = ParseExpression(tokens, ref pos);
        ifNode.Children.Add(condition);

        if (IsDelimiter(tokens, pos, DelimiterWords.PAREN_CLOSE))
            pos++;
        else
            return ErrorNode("Se esperaba ')' después de la condición del if.", pos);

        Node thenBranch;
        if (IsDelimiter(tokens, pos, DelimiterWords.BLOCK_START))
        {
            pos++;
            thenBranch = ParseBlock(tokens, ref pos);
        }
        else
        {
            var singleStatement = ParseStatement(tokens, ref pos);
            if (singleStatement != null)
            {
                thenBranch = new Node("Block");
                thenBranch.Children.Add(singleStatement);
            }
            else
            {
                return ErrorNode("Se esperaba un bloque 'ying ... yang' o una sentencia después de la condición del if.", pos);
            }
        }
        ifNode.Children.Add(thenBranch);

        if (pos < tokens.Count && Match(tokens, pos, "RESERVED", ReservedWords.ELSE))
        {
            pos++;
            Node elseBranch;

            if (pos < tokens.Count && Match(tokens, pos, "RESERVED", ReservedWords.IF))
            {
                elseBranch = ParseIf(tokens, ref pos);
            }
            else if (IsDelimiter(tokens, pos, DelimiterWords.BLOCK_START))
            {
                pos++;
                elseBranch = ParseBlock(tokens, ref pos);
            }
            else
            {
                var singleStatement = ParseStatement(tokens, ref pos);
                if (singleStatement != null)
                {
                    elseBranch = new Node("Block");
                    elseBranch.Children.Add(singleStatement);
                }
                else
                {
                    return ErrorNode("Se esperaba un bloque 'ying ... yang', 'if' o una sentencia después de 'else'.", pos);
                }
            }
            ifNode.Children.Add(elseBranch);
        }
        return ifNode;
    }

    private Node ParseFor(List<Token> tokens, ref int pos)
    {
        pos++;
        if (IsDelimiter(tokens, pos, DelimiterWords.PAREN_OPEN))
            pos++;
        else
            return ErrorNode("Se esperaba '(' después de 'for'.", pos);

        Node? init = ParseStatement(tokens, ref pos);
        if (init == null)
        {
            init = new Node("Empty");
        }

        if (IsDelimiter(tokens, pos, DelimiterWords.SEMICOLON))
        {
            pos++;
        }
        else
        {
            if (!(pos > 0 && tokens[pos - 1].Type == "DELIMITER" && tokens[pos - 1].Value == DelimiterWords.SEMICOLON))
            {
                return ErrorAfterPrevious("Se esperaba ';' después de la inicialización del for.", pos);
            }
        }
        var condition = ParseExpression(tokens, ref pos);
        if (IsDelimiter(tokens, pos, DelimiterWords.SEMICOLON))
            pos++;
        else
            return ErrorAfterPrevious("Se esperaba ';' después de la condición del for.", pos);
        var increment = ParseExpression(tokens, ref pos);
        if (IsDelimiter(tokens, pos, DelimiterWords.PAREN_CLOSE))
            pos++;
        else
            return ErrorNode("Se esperaba ')' después de la expresión de incremento del for.", pos);

        Node body;
        if (IsDelimiter(tokens, pos, DelimiterWords.BLOCK_START))
        {
            pos++;
            body = ParseBlock(tokens, ref pos);
        }
        else
        {
            var statementBody = ParseStatement(tokens, ref pos);
            if (statementBody == null)
            {
                return ErrorNode("Se esperaba un cuerpo de bucle (un bloque o una sentencia) después de la cabecera del for.", pos);
            }
            body = new Node("Block", new List<Node> { statementBody });
        }

        return new Node("For", new List<Node> { init, condition, increment, body });
    }

    private Node ParseWhile(List<Token> tokens, ref int pos)
    {
        pos++;
        if (IsDelimiter(tokens, pos, DelimiterWords.PAREN_OPEN))
            pos++;
        else
            return ErrorNode("Se esperaba '(' después de 'while'.", pos);
        var condition = ParseExpression(tokens, ref pos);
        if (IsDelimiter(tokens, pos, DelimiterWords.PAREN_CLOSE))
            pos++;
        else
            return ErrorNode("Se esperaba ')' después de la condición del while.", pos);

        Node body;
        if (IsDelimiter(tokens, pos, DelimiterWords.BLOCK_START))
        {
            pos++;
            body = ParseBlock(tokens, ref pos);
        }
        else
        {
            body = ParseStatement(tokens, ref pos) ?? new Node("Block");
        }

        return new Node("While", new List<Node> { condition, body });
    }

    private Node ParseDoWhile(List<Token> tokens, ref int pos)
    {
        pos++;
        Node body;
        if (IsDelimiter(tokens, pos, DelimiterWords.BLOCK_START))
        {
            pos++;
            body = ParseBlock(tokens, ref pos);
        }
        else
        {
            body = ParseStatement(tokens, ref pos) ?? new Node("Block");
        }

        if (Match(tokens, pos, "RESERVED", ReservedWords.WHILE))
        {
            pos++;
            if (IsDelimiter(tokens, pos, DelimiterWords.PAREN_OPEN))
                pos++;
            else
                return ErrorNode("Se esperaba '(' después de 'while' en un do-while.", pos);
            var condition = ParseExpression(tokens, ref pos);
            if (IsDelimiter(tokens, pos, DelimiterWords.PAREN_CLOSE))
                pos++;
            else
                return ErrorNode("Se esperaba ')' después de la condición del do-while.", pos);
            if (IsDelimiter(tokens, pos, DelimiterWords.SEMICOLON))
                pos++;
            else
                return ErrorAfterPrevious("Se esperaba ';' después del do-while.", pos);
            return new Node("DoWhile", new List<Node> { body, condition });
        }
        return ErrorNode("Se esperaba 'while' después del bloque de un do-while.", pos);
    }

    private Node ParseFunction(List<Token> tokens, ref int pos)
    {
        var typeNode = new Node(tokens[pos].Value) { Line = tokens[pos].Line, Column = tokens[pos].Column };
        pos++;
        var nameNode = new Node("Identifier", new List<Node> { new Node(tokens[pos].Value) });
        pos++;

        if (IsDelimiter(tokens, pos, DelimiterWords.PAREN_OPEN))
            pos++;
        else
            return ErrorNode("Se esperaba '(' después del nombre de la función.", pos);
        var parameters = ParseParams(tokens, ref pos);
        if (IsDelimiter(tokens, pos, DelimiterWords.PAREN_CLOSE))
            pos++;
        else
            return ErrorNode("Se esperaba ')' después de los parámetros de la función.", pos);

        Node body;
        if (IsDelimiter(tokens, pos, DelimiterWords.BLOCK_START))
        {
            pos++;
            body = ParseBlock(tokens, ref pos);
        }
        else
        {
            return ErrorNode("Se esperaba un bloque 'ying ... yang' para el cuerpo de la función.", pos);
        }

        return new Node("Function", new List<Node> { typeNode, nameNode, parameters, body }) { Line = typeNode.Line, Column = typeNode.Column };
    }

    private Node ParseBlock(List<Token> tokens, ref int pos)
    {
        var blockNode = new Node("Block");

        // Con demasiados bloques anidados, EnterNesting ya saltó el contenido; solo falta su 'yang'.
        var tooDeep = EnterNesting(tokens, ref pos);
        if (tooDeep != null)
        {
            blockNode.Children.Add(tooDeep);
            if (IsDelimiter(tokens, pos, DelimiterWords.BLOCK_END))
                pos++;
            return blockNode;
        }

        try
        {
            while (pos < tokens.Count && !IsDelimiter(tokens, pos, DelimiterWords.BLOCK_END))
            {
                blockNode.Children.Add(ParseStatementAdvancing(tokens, ref pos, "Token inesperado '{0}' dentro de un bloque."));
            }

            // El bucle solo termina en 'yang' o al final del archivo; antes un bloque sin cerrar pasaba sin error.
            if (IsDelimiter(tokens, pos, DelimiterWords.BLOCK_END))
                pos++;
            else
                blockNode.Children.Add(ErrorNode($"Bloque no cerrado. Se esperaba '{DelimiterWords.BLOCK_END}'.", pos));

            return blockNode;
        }
        finally
        {
            parseDepth--;
        }
    }
}
