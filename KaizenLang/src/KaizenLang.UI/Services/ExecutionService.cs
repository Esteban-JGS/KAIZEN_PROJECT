using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using KaizenLang.Core;

namespace KaizenLang.UI.Services;

public class ExecutionService
{
    private readonly CompilationService compilationService;
    private readonly Stopwatch executionTimer;
    public Func<string?, string?>? InputProvider { get; set; }

    // Tiempo máximo de ejecución del programa, sin contar la espera de input().
    public static readonly TimeSpan ExecutionTimeLimit = TimeSpan.FromSeconds(5);

    public ExecutionService()
    {
        compilationService = new CompilationService();
        executionTimer = new Stopwatch();
    }

    public ExecutionResult ExecuteCode(string source)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            return new ExecutionResult
            {
                IsSuccessful = false,
                Output = "error: no hay código para ejecutar.",
                ExecutionTime = TimeSpan.Zero
            };
        }

        var outputBuilder = new StringBuilder();

        try
        {
            var compilationResult = compilationService.CompileCode(source);
            outputBuilder.AppendLine(compilationResult.Output.TrimEnd());

            if (!compilationResult.IsSuccessful || compilationResult.AST == null)
            {
                outputBuilder.AppendLine("execution aborted: el código tiene errores de compilación.");
                return new ExecutionResult
                {
                    IsSuccessful = false,
                    Output = outputBuilder.ToString(),
                    CompilationResult = compilationResult
                };
            }

            executionTimer.Restart();
            outputBuilder.AppendLine("Running program...");

            // El límite de tiempo no debe incluir lo que el usuario tarda en responder a input():
            // se mide la espera en el diálogo y se descuenta del tiempo de ejecución.
            var inputWait = new Stopwatch();
            Func<string?, string?>? provider = null;
            if (InputProvider != null)
            {
                var userProvider = InputProvider;
                provider = prompt =>
                {
                    inputWait.Start();
                    try
                    {
                        return userProvider(prompt);
                    }
                    finally
                    {
                        inputWait.Stop();
                    }
                };
            }

            var interpreter = new Interpreter(provider)
            {
                VerboseMode = false
            };

            List<string>? executionOutput = null;
            var execTask = Task.Run(() => interpreter.Execute(compilationResult.AST));
            bool completed;
            while (!(completed = execTask.Wait(TimeSpan.FromMilliseconds(50))))
            {
                if (executionTimer.Elapsed - inputWait.Elapsed > ExecutionTimeLimit)
                    break;
            }
            if (!completed)
            {
                executionTimer.Stop();
                outputBuilder.AppendLine("error: la ejecución excedió el límite de 5 segundos.");
                return new ExecutionResult
                {
                    IsSuccessful = false,
                    Output = outputBuilder.ToString(),
                    ExecutionTime = executionTimer.Elapsed,
                    CompilationResult = compilationResult,
                    ProgramOutput = new List<string> { "execution timeout (5s)" }
                };
            }

            executionOutput = execTask.Result;
            executionTimer.Stop();

            if (executionOutput != null && executionOutput.Any())
            {
                foreach (var line in executionOutput)
                {
                    outputBuilder.AppendLine(line);
                }
            }
            else
            {
                outputBuilder.AppendLine("(sin salida)");
            }

            outputBuilder.AppendLine($"Execution finished in {executionTimer.ElapsedMilliseconds} ms.");

            return new ExecutionResult
            {
                IsSuccessful = true,
                Output = outputBuilder.ToString(),
                ExecutionTime = executionTimer.Elapsed,
                CompilationResult = compilationResult,
                ProgramOutput = executionOutput
            };
        }
        catch (Exception ex)
        {
            executionTimer.Stop();
            outputBuilder.AppendLine($"runtime error: {ex.Message}");
            if (ex.InnerException != null)
            {
                outputBuilder.AppendLine($"causado por: {ex.InnerException.Message}");
            }

            return new ExecutionResult
            {
                IsSuccessful = false,
                Output = outputBuilder.ToString(),
                ExecutionTime = executionTimer.Elapsed,
                RuntimeError = ex
            };
        }
    }

}

public class ExecutionResult
{
    public bool IsSuccessful { get; set; }
    public string Output { get; set; } = string.Empty;
    public TimeSpan ExecutionTime { get; set; }
    public CompilationResult? CompilationResult { get; set; }
    public List<string>? ProgramOutput { get; set; }
    public Exception? RuntimeError { get; set; }

    public bool HasErrors => !IsSuccessful ||
                            CompilationResult?.HasErrors == true ||
                            RuntimeError != null;
}
