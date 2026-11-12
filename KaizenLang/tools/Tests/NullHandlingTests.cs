using System;
using KaizenLang.Core;
using KaizenLang.UI.Services;
using Xunit;

namespace KaizenLang.Tests;

public class NullHandlingTests
{
    [Fact]
    public void StringVariableInitializedWithNull_ShouldCompile()
    {
        var code = "grimoire nombre = null;";
        var cs = new CompilationService();
        var result = cs.CompileCode(code);

        Assert.True(result.IsSuccessful, "Compilation should allow assigning null to string variables");
        Assert.Empty(result.SemanticErrors ?? new System.Collections.Generic.List<string>());
    }

    [Fact]
    public void ChainsawWithNullElements_ShouldCompile()
    {
        var code = @"chainsaw<grimoire> nombres = [ ""Kai"", null, ""Zen"" ];";
        var cs = new CompilationService();
        var result = cs.CompileCode(code);

        Assert.True(result.IsSuccessful, "Compilation should allow null literals inside chainsaw initializers");
        Assert.Empty(result.SemanticErrors ?? new System.Collections.Generic.List<string>());
    }

    [Fact]
    public void Runtime_NullEqualityBranch_ShouldEmitExpectedOutput()
    {
        var code = @"grimoire usuario = null;
if (usuario == null) ying
    output(""usuario desconocido"");
yang";

        var cs = new CompilationService();
        var compileResult = cs.CompileCode(code);
        Assert.True(compileResult.IsSuccessful, "Code with null equality check should compile");

        var interpreter = new Interpreter();
        var outputs = interpreter.Execute(compileResult.AST!);

        Assert.Contains("usuario desconocido", outputs);
    }

    [Fact]
    public void Runtime_NullInsideChainsaw_ShouldBeComparable()
    {
        var code = @"chainsaw<grimoire> valores = [ ""alpha"", null, ""omega"" ];
if (valores[1] == null) ying
    output(""elemento nulo"");
yang";

        var cs = new CompilationService();
        var compileResult = cs.CompileCode(code);
        Assert.True(compileResult.IsSuccessful, "Chainsaw initializer with null should compile");

        var interpreter = new Interpreter();
        var outputs = interpreter.Execute(compileResult.AST!);

        Assert.Contains("elemento nulo", outputs);
    }
}
