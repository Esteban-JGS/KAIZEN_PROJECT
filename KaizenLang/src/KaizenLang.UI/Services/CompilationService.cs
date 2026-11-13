using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using KaizenLang.Core;

namespace KaizenLang.UI.Services;

public class CompilationService
{
    private static readonly bool VerboseOutput =
        string.Equals(Environment.GetEnvironmentVariable("KAIZEN_VERBOSE_OUTPUT"), "1", StringComparison.Ordinal);

    private readonly Stopwatch compilationTimer;
    private readonly StringBuilder outputBuilder;

    public CompilationService()
    {
        compilationTimer = new Stopwatch();
        outputBuilder = new StringBuilder();
    }

    public CompilationResult CompileCode(string source)
    {
        KaizenLang.Core.Logging.Logger.Debug("CompilationService.CompileCode START");

        var stageOutcomes = new List<CompilationStageOutcome>();
        outputBuilder.Clear();
        AppendHeader();

        if (string.IsNullOrWhiteSpace(source))
        {
            AppendError("error: no hay código para compilar.");
            var emptyResult = new CompilationResult
            {
                IsSuccessful = false,
                CompilationTime = TimeSpan.Zero
            };

            stageOutcomes.Add(new CompilationStageOutcome(
                "Validacion de entrada",
                false,
                1,
                "No se proporciono codigo fuente."));

            FinalizeResult(emptyResult, stageOutcomes, success: false);
            return emptyResult;
        }

        compilationTimer.Restart();

        try
        {
            var result = PerformCompilationStages(source, stageOutcomes);
            compilationTimer.Stop();

            FinalizeResult(result, stageOutcomes, result.IsSuccessful);
            return result;
        }
        catch (Exception ex)
        {
            compilationTimer.Stop();
            AppendError("error: falla interna del compilador.");
            AppendError(ex.Message ?? "Error no especificado.");
            if (!string.IsNullOrWhiteSpace(ex.StackTrace))
            {
                if (VerboseOutput)
                {
                    AppendError(ex.StackTrace!);
                }
            }

            stageOutcomes.Add(new CompilationStageOutcome(
                "Error inesperado",
                false,
                1,
                "Se produjo una excepcion no controlada.",
                detail: ex.Message,
                exception: ex));

            var failedResult = new CompilationResult
            {
                IsSuccessful = false,
                CompilationTime = compilationTimer.Elapsed,
                InternalError = ex
            };

            FinalizeResult(failedResult, stageOutcomes, success: false);
            return failedResult;
        }
    }

    private CompilationResult PerformCompilationStages(string source, List<CompilationStageOutcome> stageOutcomes)
    {
        KaizenLang.Core.Logging.Logger.Debug("PerformCompilationStages START");

        var result = new CompilationResult();

        if (!PerformLexicalAnalysis(source, out var tokens, result, stageOutcomes))
        {
            return result;
        }

        if (!PerformSyntacticAnalysis(tokens!, out var ast, result, stageOutcomes))
        {
            return result;
        }

        result.AST = ast;

        if (!PerformSemanticAnalysis(ast!, result, stageOutcomes))
        {
            return result;
        }

        result.IsSuccessful = true;
        result.Tokens = tokens;

        ShowCompilationDetails(tokens!, ast!, result);

        return result;
    }

    private bool PerformLexicalAnalysis(
        string source,
        out List<Token>? tokens,
        CompilationResult result,
        List<CompilationStageOutcome> stageOutcomes)
    {
        AppendPhaseHeader("FASE 1: ANALISIS LEXICO");

        try
        {
            var lexer = new Lexer();
            tokens = lexer.Tokenize(source);

            var invalidTokens = tokens.Where(t => t.Type == "INVALID").ToList();
            var reservedTokens = tokens.Where(t => t.Type == "RESERVED").ToList();
            var identifierTokens = tokens.Where(t => t.Type == "IDENTIFIER").ToList();
            var literalTokens = tokens.Where(t => t.Type is "INT" or "FLOAT" or "STRING" or "CHAR" or "LITERAL").ToList();

            if (invalidTokens.Any())
            {
                foreach (var invalidToken in invalidTokens)
                {
                    AppendError($"error léxico: {invalidToken.Value} (línea {invalidToken.Line}, columna {invalidToken.Column})");
                }

                result.LexicalErrors = invalidTokens;
                stageOutcomes.Add(new CompilationStageOutcome(
                    "Analisis lexico",
                    false,
                    invalidTokens.Count,
                    "Se detectaron tokens invalidos."));

                tokens = null;
                return false;
            }

            if (VerboseOutput)
            {
                AppendSuccess("[OK] Analisis lexico completado.");
                AppendInfo("Estadisticas de tokens:");
                AppendInfo($"   Total de tokens: {tokens.Count}");
                AppendInfo($"   Palabras reservadas: {reservedTokens.Count}");
                AppendInfo($"   Identificadores: {identifierTokens.Count}");
                AppendInfo($"   Literales: {literalTokens.Count}");

                ShowTokenSample(tokens);
                AppendNewLine();
            }

            stageOutcomes.Add(new CompilationStageOutcome(
                "Analisis lexico",
                true,
                0,
                $"Tokens generados: {tokens.Count}"));

            return true;
        }
        catch (Exception ex)
        {
            AppendError("[ERROR] Fallo el analisis lexico.");
            AppendError(ex.Message ?? "Error no especificado.");
            stageOutcomes.Add(new CompilationStageOutcome(
                "Analisis lexico",
                false,
                1,
                "Se produjo una excepcion durante el analisis lexico.",
                detail: ex.Message,
                exception: ex));
            result.InternalError = ex;
            tokens = null;
            return false;
        }
    }

    private bool PerformSyntacticAnalysis(
        List<Token> tokens,
        out Node? ast,
        CompilationResult result,
        List<CompilationStageOutcome> stageOutcomes)
    {
        AppendPhaseHeader("FASE 2: ANALISIS SINTACTICO");

        try
        {
            var parser = new Parser();
            ast = parser.Parse(tokens);

            var syntaxErrors = ast.GetAllErrors();
            if (syntaxErrors.Any())
            {
                foreach (var error in syntaxErrors)
                {
                    AppendError($"error sintáctico: {error}");
                }

                result.SyntaxErrors = syntaxErrors;
                ast = null;

                stageOutcomes.Add(new CompilationStageOutcome(
                    "Analisis sintactico",
                    false,
                    syntaxErrors.Count,
                    "Se detectaron errores sintacticos."));

                return false;
            }

            var nodeCount = CountNodes(ast);
            var depth = CalculateDepth(ast);

            if (VerboseOutput)
            {
                AppendSuccess("[OK] Analisis sintactico completado.");
                AppendSuccess("[OK] Se genero el arbol de sintaxis abstracta.");

                AppendInfo("Estadisticas del AST:");
                AppendInfo($"   Nodos totales: {nodeCount}");
                AppendInfo($"   Profundidad maxima: {depth}");
                AppendNewLine();
            }

            stageOutcomes.Add(new CompilationStageOutcome(
                "Analisis sintactico",
                true,
                0,
                $"AST con {nodeCount} nodos y profundidad {depth}."));

            return true;
        }
        catch (Exception ex)
        {
            AppendError("error: fallo el análisis sintáctico.");
            AppendError(ex.Message ?? "Error no especificado.");
            stageOutcomes.Add(new CompilationStageOutcome(
                "Analisis sintactico",
                false,
                1,
                "Se produjo una excepcion durante el analisis sintactico.",
                detail: ex.Message,
                exception: ex));
            result.InternalError = ex;
            ast = null;
            return false;
        }
    }

    private bool PerformSemanticAnalysis(
        Node ast,
        CompilationResult result,
        List<CompilationStageOutcome> stageOutcomes)
    {
        AppendPhaseHeader("FASE 3: ANALISIS SEMANTICO");

        try
        {
            var semanticAnalyzer = new SemanticAnalyzer();
            var semanticErrors = semanticAnalyzer.AnalyzeProgram(ast);

            if (semanticErrors.Any())
            {
                foreach (var error in semanticErrors)
                {
                    AppendError($"error semántico: {error}");
                }

                result.SemanticErrors = semanticErrors;

                stageOutcomes.Add(new CompilationStageOutcome(
                    "Analisis semantico",
                    false,
                    semanticErrors.Count,
                    "Se detectaron errores semanticos."));

                return false;
            }

            if (VerboseOutput)
            {
                AppendSuccess("[OK] Analisis semantico completado.");
                AppendSuccess("[OK] Validaciones de tipos y alcance superadas.");
                AppendNewLine();
            }

            stageOutcomes.Add(new CompilationStageOutcome(
                "Analisis semantico",
                true,
                0,
                "No se detectaron errores semanticos."));

            return true;
        }
        catch (Exception ex)
        {
            AppendError("error: fallo el análisis semántico.");
            AppendError(ex.Message ?? "Error no especificado.");
            stageOutcomes.Add(new CompilationStageOutcome(
                "Analisis semantico",
                false,
                1,
                "Se produjo una excepcion durante el analisis semantico.",
                detail: ex.Message,
                exception: ex));
            result.InternalError = ex;
            return false;
        }
    }

    private void ShowCompilationDetails(List<Token> tokens, Node ast, CompilationResult result)
    {
        if (!VerboseOutput)
        {
            return;
        }

        AppendSectionHeader("DETALLES DE COMPILACION");

        AppendInfo("AST (vista truncada si es necesario):");
        var astString = ast.ToTreeString();
        if (astString.Length > 1000)
        {
            AppendInfo(astString.Substring(0, 1000) + Environment.NewLine + "... (AST truncado para visualizacion)");
        }
        else
        {
            AppendInfo(astString);
        }
        AppendNewLine();

        ShowComplexityMetrics(ast);
    }

    private void ShowComplexityMetrics(Node ast)
    {
        AppendInfo("Metricas de complejidad:");
        AppendInfo("-------------------------");

        var functions = CountNodesByType(ast, "Function");
        var conditionals = CountNodesByType(ast, "If") + CountNodesByType(ast, "While") + CountNodesByType(ast, "DoWhile") + CountNodesByType(ast, "For");
        var variables = CountNodesByType(ast, "VariableDeclaration");

        AppendInfo($"   Funciones definidas: {functions}");
        AppendInfo($"   Estructuras de control: {conditionals}");
        AppendInfo($"   Variables declaradas: {variables}");
        AppendNewLine();
    }

    private void ShowTokenSample(List<Token> tokens)
    {
        AppendInfo("Muestra de tokens:");
        var importantTokens = tokens.Take(15).ToList();
        foreach (var token in importantTokens)
        {
            AppendInfo($"   {token.Type.PadRight(15)}: '{token.Value}' (l{token.Line}:c{token.Column})");
        }

        if (tokens.Count > 15)
        {
            AppendInfo($"   ... y {tokens.Count - 15} tokens adicionales.");
        }
    }

    private void FinalizeResult(CompilationResult result, List<CompilationStageOutcome> stageOutcomes, bool success)
    {
        if (result.CompilationTime == default)
        {
            result.CompilationTime = compilationTimer.Elapsed;
        }

        result.StageOutcomes.Clear();
        result.StageOutcomes.AddRange(stageOutcomes);

        AppendStageSummary(stageOutcomes);

        if (success)
        {
            AppendSuccessMessage();
        }
        else
        {
            AppendFailureMessage();
        }

        result.Output = outputBuilder.ToString();
    }

    private void AppendStageSummary(IEnumerable<CompilationStageOutcome> outcomes)
    {
        if (!VerboseOutput)
        {
            return;
        }

        AppendSectionHeader("RESUMEN DE ETAPAS");
        if (!outcomes.Any())
        {
            AppendInfo("No se registraron etapas.");
            AppendNewLine();
            return;
        }

        foreach (var outcome in outcomes)
        {
            var status = outcome.Succeeded ? "[OK]" : "[FAIL]";
            var summary = string.IsNullOrWhiteSpace(outcome.Summary)
                ? "Sin descripcion."
                : outcome.Summary;

            AppendInfo($"{status} {outcome.Stage}: {summary}");

            if (outcome.ErrorCount > 0)
            {
                AppendInfo($"    Errores detectados: {outcome.ErrorCount}");
            }

            if (!string.IsNullOrWhiteSpace(outcome.Detail))
            {
                AppendInfo($"    Detalle: {outcome.Detail}");
            }

            if (outcome.Exception != null)
            {
                AppendInfo($"    Excepcion: {outcome.Exception.GetType().Name} - {outcome.Exception.Message}");
            }
        }

        AppendNewLine();
    }

    private int CountNodes(Node node)
    {
        var count = 1;
        foreach (var child in node.Children)
        {
            count += CountNodes(child);
        }
        return count;
    }

    private int CalculateDepth(Node node)
    {
        if (!node.Children.Any())
        {
            return 1;
        }

        return 1 + node.Children.Max(child => CalculateDepth(child));
    }

    private int CountNodesByType(Node node, string type)
    {
        var count = node.Type.Equals(type, StringComparison.OrdinalIgnoreCase) ? 1 : 0;
        foreach (var child in node.Children)
        {
            count += CountNodesByType(child, type);
        }
        return count;
    }

    private void AppendHeader()
    {
        if (VerboseOutput)
        {
            outputBuilder.AppendLine("[INIT] PROCESO DE COMPILACION");
            outputBuilder.AppendLine(new string('=', 35));
            outputBuilder.AppendLine();
        }
    }

    private void AppendPhaseHeader(string phase)
    {
        if (!VerboseOutput)
        {
            return;
        }

        outputBuilder.AppendLine($"== {phase}");
        outputBuilder.AppendLine(new string('-', 35));
    }

    private void AppendSectionHeader(string section)
    {
        if (!VerboseOutput)
        {
            return;
        }

        outputBuilder.AppendLine($"== {section}");
        outputBuilder.AppendLine(new string('=', 35));
    }

    private void AppendSuccess(string message)
    {
        if (VerboseOutput)
        {
            outputBuilder.AppendLine(message);
        }
    }

    private void AppendInfo(string message)
    {
        if (VerboseOutput)
        {
            outputBuilder.AppendLine(message);
        }
    }

    private void AppendError(string message) => outputBuilder.AppendLine(message);

    private void AppendNewLine()
    {
        if (VerboseOutput)
        {
            outputBuilder.AppendLine();
        }
    }

    private void AppendSuccessMessage()
    {
        if (VerboseOutput)
        {
            AppendSectionHeader("COMPILACION EXITOSA");
            AppendSuccess("Analisis lexico: OK");
            AppendSuccess("Analisis sintactico: OK");
            AppendSuccess("Analisis semantico: OK");
            AppendSuccess($"Tiempo de compilacion: {compilationTimer.ElapsedMilliseconds} ms");
            AppendNewLine();
            AppendSuccess("El codigo esta listo para ejecutarse.");
        }
        else
        {
            outputBuilder.AppendLine($"Compilation succeeded in {compilationTimer.ElapsedMilliseconds} ms.");
        }
    }

    private void AppendFailureMessage()
    {
        if (VerboseOutput)
        {
            AppendSectionHeader("COMPILACION INCOMPLETA");
            AppendError("Se detectaron errores. Revise la informacion anterior.");
            AppendNewLine();
        }
        else
        {
            outputBuilder.AppendLine($"Compilation failed in {compilationTimer.ElapsedMilliseconds} ms.");
        }
    }
}

public class CompilationResult
{
    public bool IsSuccessful { get; set; }
    public string Output { get; set; } = string.Empty;
    public TimeSpan CompilationTime { get; set; }
    public List<Token>? Tokens { get; set; }
    public Node? AST { get; set; }
    public List<Token>? LexicalErrors { get; set; }
    public List<string> SyntaxErrors { get; set; } = new List<string>();
    public List<string> SemanticErrors { get; set; } = new List<string>();
    public Exception? InternalError { get; set; }
    public List<CompilationStageOutcome> StageOutcomes { get; } = new List<CompilationStageOutcome>();

    public bool HasErrors =>
        (LexicalErrors?.Any() == true) ||
        SyntaxErrors.Any() ||
        SemanticErrors.Any() ||
        InternalError != null;

    public string? FailureStage => StageOutcomes.LastOrDefault(o => !o.Succeeded)?.Stage;
}

public class CompilationStageOutcome
{
    public CompilationStageOutcome(
        string stage,
        bool succeeded,
        int errorCount,
        string summary,
        string? detail = null,
        Exception? exception = null)
    {
        Stage = stage;
        Succeeded = succeeded;
        ErrorCount = errorCount;
        Summary = summary;
        Detail = detail;
        Exception = exception;
    }

    public string Stage { get; }
    public bool Succeeded { get; }
    public int ErrorCount { get; }
    public string Summary { get; }
    public string? Detail { get; }
    public Exception? Exception { get; }
}
