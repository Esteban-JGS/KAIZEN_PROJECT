using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KaizenLang.Core;
using KaizenLang.UI.Services;
using Xunit;

namespace KaizenLang.Tests;

// Ejecuta cada programa de examples/ con los mismos límites que el playground web (BrowserLimits).
// Así se detecta un ejemplo que deja de funcionar en el navegador, como pasó cuando la prueba de
// estrés llamaba a una recursión de 51 niveles y el playground corta en 40.
public class ExampleProgramsTests
{
    // Ejemplos que terminan en error a propósito.
    private static readonly HashSet<string> ExpectedToFail = new()
    {
        "test-infinite-recursion",
        "test-syntax-errors",
    };

    public static IEnumerable<object[]> ExampleNames() =>
        Directory.GetFiles(ExamplesDirectory(), "test-*.txt")
            .Select(Path.GetFileNameWithoutExtension)
            .OrderBy(name => name, StringComparer.Ordinal)
            .Select(name => new object[] { name! });

    [Theory]
    [MemberData(nameof(ExampleNames))]
    public void Example_RunsWithBrowserLimits(string name)
    {
        var (compiled, errors, output) = Run(name);
        var summary = string.Join(Environment.NewLine, errors.Concat(output));

        if (ExpectedToFail.Contains(name))
        {
            Assert.True(!compiled || output.Any(IsRuntimeError), $"'{name}' debería terminar en error:{Environment.NewLine}{summary}");
            return;
        }

        Assert.True(compiled, $"'{name}' no compila:{Environment.NewLine}{summary}");
        Assert.DoesNotContain(output, IsRuntimeError);
        Assert.NotEmpty(output);
    }

    [Fact]
    public void InfiniteRecursionExample_EndsWithTheRecursionLimitMessage()
    {
        var (_, _, output) = Run("test-infinite-recursion");
        Assert.Contains(output, line => line.Contains($"profundidad máxima de recursión permitida ({BrowserLimits.MaxCallDepth})"));
    }

    private static bool IsRuntimeError(string line) => line.StartsWith("Error de ejecución", StringComparison.Ordinal);

    private static (bool Compiled, List<string> Errors, List<string> Output) Run(string name)
    {
        var directory = ExamplesDirectory();
        var source = File.ReadAllText(Path.Combine(directory, name + ".txt"));
        var result = new CompilationService().CompileCode(source);
        var errors = (result.SyntaxErrors ?? new List<string>())
            .Concat(result.SemanticErrors ?? new List<string>())
            .Concat((result.LexicalErrors ?? new List<Token>()).Select(t => t.Value))
            .ToList();
        if (!result.IsSuccessful || result.AST == null)
            return (false, errors, new List<string>());

        // Igual que el playground: input() lee de una cola de líneas (el archivo de inputs/, si existe).
        var inputFile = Path.Combine(directory, "inputs", name.Replace("-input-output", "-input") + ".txt");
        var lines = new Queue<string>(File.Exists(inputFile)
            ? File.ReadAllText(inputFile).Replace("\r", string.Empty).Split('\n')
            : Array.Empty<string>());
        var interpreter = BrowserLimits.Apply(new Interpreter(_ => lines.Count > 0 ? lines.Dequeue() : null));
        return (true, errors, interpreter.Execute(result.AST));
    }

    private static string ExamplesDirectory()
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir != null; dir = dir.Parent)
        {
            if (File.Exists(Path.Combine(dir.FullName, "KaizenLang.sln")))
                return Path.Combine(dir.FullName, "examples");
        }
        throw new DirectoryNotFoundException("No se encontró KaizenLang.sln para ubicar la carpeta examples/.");
    }
}
