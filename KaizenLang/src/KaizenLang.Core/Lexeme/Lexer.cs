namespace KaizenLang.Core;

public class Lexer
{
    public List<Token> Tokenize(string source)
    {
        var tokenizer = new Tokenizer(source);
        return tokenizer.Tokenize();
    }
}

