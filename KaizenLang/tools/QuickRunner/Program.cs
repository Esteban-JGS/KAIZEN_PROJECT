using KaizenLang.Core;
using KaizenLang.UI.Services;

KaizenLang.Core.Logging.Logger.Info("QuickRunner: compiling and running function-call snippet...");

var source = @"void saludar() ying
    output(""¡Hola desde una función!"");
yang

saludar();

integer sumar(integer a, integer b) ying
    return a + b;
yang

integer resultado = sumar(5, 3);
output(""El resultado de la suma es: "" + resultado);
";

// First, compile and print compilation diagnostics immediately
var compilationService = new CompilationService();
var compilationResult = compilationService.CompileCode(source);
KaizenLang.Core.Logging.Logger.Info("--- Compilation Output ---");
KaizenLang.Core.Logging.Logger.Debug(compilationResult.Output);

if (!compilationResult.IsSuccessful || compilationResult.AST == null)
{
    KaizenLang.Core.Logging.Logger.Warn("Compilation failed; aborting execution.");
    if (compilationResult.SyntaxErrors != null && compilationResult.SyntaxErrors.Any())
    {
        KaizenLang.Core.Logging.Logger.Info("--- Syntax Errors ---");
        foreach (var e in compilationResult.SyntaxErrors)
            KaizenLang.Core.Logging.Logger.Info(e);
    }
    if (compilationResult.SemanticErrors != null && compilationResult.SemanticErrors.Any())
    {
        KaizenLang.Core.Logging.Logger.Info("--- Semantic Errors ---");
        foreach (var e in compilationResult.SemanticErrors)
            KaizenLang.Core.Logging.Logger.Info(e);
    }
    return;
}

KaizenLang.Core.Logging.Logger.Info("Starting interpreter.Execute in background task (15s timeout)...");
var interpreter = new Interpreter((prompt) => null);
var execTask = System.Threading.Tasks.Task.Run(() => interpreter.Execute(compilationResult.AST));
if (execTask.Wait(TimeSpan.FromSeconds(15)))
{
    var programOutput = execTask.Result;
    KaizenLang.Core.Logging.Logger.Info("--- Interpreter Program Output ---");
    if (programOutput != null && programOutput.Any())
    {
        foreach (var line in programOutput)
            KaizenLang.Core.Logging.Logger.Info(line);
    }
    else
    {
        KaizenLang.Core.Logging.Logger.Info("(no program output)");
    }
    KaizenLang.Core.Logging.Logger.Info("Interpreter execution finished");
}
else
{
    KaizenLang.Core.Logging.Logger.Warn("Interpreter.Execute timed out after 15s — likely hanging inside interpreter");
}

