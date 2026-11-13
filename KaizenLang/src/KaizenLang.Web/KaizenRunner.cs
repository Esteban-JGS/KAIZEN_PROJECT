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
    // Tope de nodos evaluados: en el navegador no hay un timeout que corte un programa muy largo.
    public const long MaxExecutionSteps = 2_000_000;

    // La pila de WebAssembly es mucho más chica que la de escritorio: si se agota, la aplicación se cae
    // sin mensaje. Medido en Edge, se cae entre 570 y 650 niveles de evaluación anidados (unas 140 llamadas
    // de una función simple, unas 47 si cada llamada anida bucles e if). En otro navegador basado en Chromium
    // se cayó con unos 370 niveles y funcionó con 209, así que los topes quedan por debajo de eso:
    // - MaxCallDepth corta la recursión común con el mensaje habitual.
    // - MaxNestingDepth corta las funciones "pesadas" antes de que agoten la pila.
    public const int MaxCallDepth = 40;
    public const int MaxNestingDepth = 200;

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
        var interpreter = new Interpreter(_ => lines.Count > 0 ? lines.Dequeue() : null)
        {
            MaxExecutionSteps = MaxExecutionSteps,
            MaxCallDepth = MaxCallDepth,
            MaxNestingDepth = MaxNestingDepth
        };
        var output = interpreter.Execute(ast);
        bool runtimeError = output.Any(line => line.StartsWith("Error de ejecución", StringComparison.Ordinal));
        bool warnings = output.Any(line => line.StartsWith("Advertencia", StringComparison.Ordinal));
        return new RunResult(!runtimeError, diagnostics, output, timer.Elapsed, warnings);
    }
}
