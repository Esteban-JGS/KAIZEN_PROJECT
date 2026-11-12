using System;
using System.Linq;
using KaizenLang.Core;
using KaizenLang.UI.Services;
using Xunit;

namespace KaizenLang.Tests;

public class CollectionSemanticTests
{
    [Fact]
    public void ChainsawDeclarationWithoutElementType_ShouldBeRejected()
    {
        var code = @"chainsaw nums = [1,2,3];";
        var cs = new CompilationService();
        var res = cs.CompileCode(code);
        Assert.False(res.IsSuccessful, "Compilation should fail for chainsaw declaration without element type");
        var combinedList = (res.SyntaxErrors ?? new System.Collections.Generic.List<string>()).Concat(res.SemanticErrors ?? new System.Collections.Generic.List<string>()).ToList();
        if (!string.IsNullOrEmpty(res.Output))
            combinedList.Add(res.Output);
        var combined = string.Join("|", combinedList);
        if (res.SemanticErrors != null && res.SemanticErrors.Count > 0)
        {
            Assert.True(
                combined.Contains("Declaración de chainsaw", StringComparison.OrdinalIgnoreCase) ||
                combined.Contains("Declaración de hogyoku", StringComparison.OrdinalIgnoreCase),
                $"Expected semantic error for missing chainsaw/hogyoku element type but got: {combined}");
        }
        else
        {
            Assert.True(
                (res.SyntaxErrors != null && res.SyntaxErrors.Count > 0) ||
                (!string.IsNullOrEmpty(res.Output) && res.Output.Contains("Parser invariant", StringComparison.OrdinalIgnoreCase)),
                $"Expected syntactic failure for malformed chainsaw declaration but got: {combined}");
        }
    }
    [Fact]
    public void RaggedHogyoku_ShouldReportSemanticError()
    {
        var code = @"hogyoku<gear> ragged = [ [1,2], [3] ];";
        var cs = new CompilationService();
        var res = cs.CompileCode(code);
        Assert.False(res.IsSuccessful, "Compilation should fail for ragged hogyoku");
        Assert.Contains("Hogyoku no rectangular", string.Join("|", res.SemanticErrors ?? new System.Collections.Generic.List<string>()));
    }

    [Fact]
    public void ChainsawTypeMismatch_ShouldReportSemanticError()
    {
        var code = @"chainsaw<grimoire> wrong = [ ""a"", ""b"", 5 ];";
        var cs = new CompilationService();
        var res = cs.CompileCode(code);
        Assert.False(res.IsSuccessful, "Compilation should fail for chainsaw type mismatch");
        Assert.Contains("Tipo incompatible en inicialización de chainsaw", string.Join("|", res.SemanticErrors ?? new System.Collections.Generic.List<string>()));
    }
}
