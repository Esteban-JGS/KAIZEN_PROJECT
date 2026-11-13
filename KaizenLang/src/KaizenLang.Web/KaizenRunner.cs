using System.Diagnostics;
using KaizenLang.Core;

namespace KaizenLang.Web;

// HasWarnings: la ejecución terminó, pero el intérprete cortó algo (p. ej. un bucle en el tope de
// iteraciones), así que la salida puede estar incompleta.
public sealed record RunResult(bool Success, IReadOnlyList<string> Diagnostics, IReadOnlyList<string> Output, TimeSpan Elapsed, bool HasWarnings = false);

// Las mismas fases que CompilationService del IDE (léxico, sintáctico, semántico y ejecución),
// sin dependencias de WinForms para que corran en el navegador.
public static class KaizenRunner
{
    public static RunResult Run(string source, string input, bool execute)
    {
        var timer = Stopwatch.StartNew();
        var diagnostics = new List<string>();

        if (string.IsNullOrWhiteSpace(source))
            return new RunResult(false, new[] { "No hay código para compilar." }, Array.Empty<string>(), timer.Elapsed);

        var tokens = new Lexer().Tokenize(source);
        foreach (var token in tokens.Where(t => t.Type == "INVALID"))
            diagnostics.Add($"error léxico: {token.Value} (línea {token.Line}, columna {token.Column})");
        if (diagnostics.Count > 0)
            return new RunResult(false, diagnostics, Array.Empty<string>(), timer.Elapsed);

        var ast = new Parser().Parse(tokens);
        foreach (var error in ast.GetAllErrors())
            diagnostics.Add($"error sintáctico: {error}");
        if (diagnostics.Count > 0)
            return new RunResult(false, diagnostics, Array.Empty<string>(), timer.Elapsed);

        foreach (var error in new SemanticAnalyzer().AnalyzeProgram(ast))
            diagnostics.Add($"error semántico: {error}");
        if (diagnostics.Count > 0)
            return new RunResult(false, diagnostics, Array.Empty<string>(), timer.Elapsed);

        if (!execute)
            return new RunResult(true, diagnostics, Array.Empty<string>(), timer.Elapsed);

        // input() lee del cuadro de entrada: cada llamada toma la siguiente palabra o línea.
        var lines = new Queue<string>(input.Replace("\r", string.Empty).Split('\n'));
        // Los topes de la pila de WebAssembly están documentados en BrowserLimits (KaizenLang.Core).
        var interpreter = BrowserLimits.Apply(new Interpreter(_ => lines.Count > 0 ? lines.Dequeue() : null));
        var output = interpreter.Execute(ast);
        bool runtimeError = output.Any(line => line.StartsWith("Error de ejecución", StringComparison.Ordinal));
        bool warnings = output.Any(line => line.StartsWith("Advertencia", StringComparison.Ordinal));
        return new RunResult(!runtimeError, diagnostics, output, timer.Elapsed, warnings);
    }
}
