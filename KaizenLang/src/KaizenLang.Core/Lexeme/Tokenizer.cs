using System;
using System.Collections.Generic;
using System.Linq;

namespace KaizenLang.Core;

internal class Tokenizer
{
    private readonly CharStream stream;

    public Tokenizer(string source)
    {
        stream = new CharStream(source ?? string.Empty);
    }

    public List<Token> Tokenize()
    {
        var tokens = new List<Token>();

        var validTypes = new HashSet<string>(TypeWords.Words);

        while (!stream.EOF)
        {
            var ch = stream.Peek();
            if (ch == null)
                break;

            if (char.IsWhiteSpace(ch.Value))
            {
                stream.Read();
                continue;
            }

            if (char.IsLetter(ch.Value))
            {
                var word = stream.ReadWhile(c => char.IsLetterOrDigit(c) || c == '_');
                var tokenLine = stream.Line;
                var tokenColumn = stream.Column - word.Length;
                if (DelimiterWords.MultiCharDelimiters.Contains(word))
                {
                    tokens.Add(new Token("DELIMITER", word, tokenLine, tokenColumn));
                }
                else if (validTypes.Contains(word))
                    tokens.Add(new Token("TYPE", word, tokenLine, tokenColumn));
                else if (ReservedWords.Words.Contains(word))
                    tokens.Add(new Token("RESERVED", word, tokenLine, tokenColumn));
                else if (LiteralWords.Words.Contains(word))
                    tokens.Add(new Token("LITERAL", word, tokenLine, tokenColumn));
                else
                    tokens.Add(new Token("IDENTIFIER", word, tokenLine, tokenColumn));
                continue;
            }

            if (char.IsDigit(ch.Value))
            {
                var startLine = stream.Line;
                var startCol = stream.Column;
                var number = stream.ReadWhile(c => char.IsDigit(c));
                if (stream.Peek() == '.')
                {
                    stream.Read();
                    number += "." + stream.ReadWhile(c => char.IsDigit(c));
                    tokens.Add(new Token("FLOAT", number, startLine, startCol));
                }
                else
                {
                    var nextChar = stream.Peek();
                    if (nextChar != null && (char.IsLetter(nextChar.Value) || nextChar == '_'))
                    {
                        var rest = stream.ReadWhile(c => char.IsLetterOrDigit(c) || c == '_');
                        var full = number + rest;
                        tokens.Add(new Token("INVALID", $"Token inválido: secuencia numérica seguida de identificador '{full}'", startLine, startCol));
                    }
                    else
                    {
                        tokens.Add(new Token("INT", number, startLine, startCol));
                    }
                }
                continue;
            }

            if (ch == '"')
            {
                var startLine = stream.Line;
                var startCol = stream.Column;
                stream.Read();
                var str = stream.ReadWhile(c => c != '"');
                if (!stream.EOF && stream.Peek() == '"')
                {
                    stream.Read();
                    tokens.Add(new Token("STRING", str, startLine, startCol));
                }
                else
                {
                    tokens.Add(new Token("INVALID", $"Cadena sin cierre", startLine, startCol));
                }
                continue;
            }

            if (ch == '\'')
            {
                var startLine = stream.Line;
                var startCol = stream.Column;
                stream.Read();
                var first = stream.Read();
                if (first != null && stream.Peek() == '\'')
                {
                    stream.Read();
                    tokens.Add(new Token("CHAR", first?.ToString() ?? string.Empty, startLine, startCol));
                }
                else
                {
                    tokens.Add(new Token("INVALID", $"Literal de char inválido", startLine, startCol));
                }
                continue;
            }

            if (ch == '/' && stream.Peek(1) == '/')
            {
                stream.Read();
                stream.Read();
                while (!stream.EOF && stream.Peek() != '\n')
                    stream.Read();
                continue;
            }
            if (ch == '/' && stream.Peek(1) == '*')
            {
                var startLine = stream.Line;
                var startCol = stream.Column;
                stream.Read();
                stream.Read();
                while (!stream.EOF && !(stream.Peek() == '*' && stream.Peek(1) == '/'))
                    stream.Read();
                if (!stream.EOF)
                {
                    stream.Read();
                    stream.Read();
                }
                else
                {
                    tokens.Add(new Token("INVALID", $"Comentario de bloque sin cierre", startLine, startCol));
                }
                continue;
            }

            bool matched = false;
            foreach (var op in OperatorWords.Words.OrderByDescending(x => x.Length))
            {
                var candidate = "";
                for (int i = 0; i < op.Length; i++)
                {
                    var p = stream.Peek(i);
                    if (p == null)
                    { candidate = null; break; }
                    candidate += p.Value;
                }
                if (candidate == op)
                {
                    for (int i = 0; i < op.Length; i++)
                        stream.Read();
                    tokens.Add(new Token("OPERATOR", op, stream.Line, stream.Column - op.Length));
                    matched = true;
                    break;
                }
            }
            if (matched)
                continue;

            var single = stream.Peek()?.ToString();
            if (single != null && DelimiterWords.SingleCharDelimiters.Contains(single))
            {
                tokens.Add(new Token("DELIMITER", single, stream.Line, stream.Column));
                stream.Read();
                continue;
            }

            // La posición se toma antes de leer; después de Read() apunta al carácter siguiente.
            var invalidLine = stream.Line;
            var invalidColumn = stream.Column;
            var invalidChar = stream.Read();
            tokens.Add(new Token("INVALID", $"Carácter no reconocido '{invalidChar}'", invalidLine, invalidColumn));
        }

        return tokens;
    }
}
