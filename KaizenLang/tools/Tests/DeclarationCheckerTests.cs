using System;
using System.Linq;
using KaizenLang.Core;
using KaizenLang.UI.Services;
using Xunit;

namespace KaizenLang.Tests;

public class DeclarationCheckerTests
{
    [Fact]
    public void VariableDeclaration_HappyPath_ShouldSucceed()
    {
        var code = @"gear x = 5;";
        var cs = new CompilationService();
        var res = cs.CompileCode(code);
        Assert.True(res.IsSuccessful, "Compilation should succeed for valid variable declaration");
    }

    [Fact]
    public void DuplicateVariableDeclaration_ShouldReportSemanticError()
    {
        var code = @"gear x = 1; gear x = 2;";
        var cs = new CompilationService();
        var res = cs.CompileCode(code);
        Assert.False(res.IsSuccessful);
        Assert.Contains("ya declarada", string.Join("|", res.SemanticErrors ?? new System.Collections.Generic.List<string>()));
    }

    [Fact]
    public void MalformedInitializer_LiteralIdentifierConcatenation_ShouldReportError()
    {
        // El lexer marca "123abc" como token inválido (número pegado a un identificador).
        var code = @"gear hola = 123abc;";
        var cs = new CompilationService();
        var res = cs.CompileCode(code);
        Assert.False(res.IsSuccessful);
        Assert.NotNull(res.LexicalErrors);
        Assert.Contains(res.LexicalErrors!, t => t.Value.Contains("123abc"));
    }

    [Fact]
    public void UndeclaredIdentifierInInitializer_ShouldReportError()
    {
        var code = @"gear z = someUndeclaredVar;";
        var cs = new CompilationService();
        var res = cs.CompileCode(code);
        Assert.False(res.IsSuccessful);
        Assert.Contains("no declarada", string.Join("|", res.SemanticErrors ?? new System.Collections.Generic.List<string>()));
    }
}
