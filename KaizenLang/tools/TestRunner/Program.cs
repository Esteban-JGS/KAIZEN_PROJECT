using System;
using System.IO;
using KaizenLang.Core;
using KaizenLang.UI.Services;

if (args.Length == 0)
{
    Console.WriteLine("Uso: TestRunner <archivo.txt>");
    return 1;
}

string filePath = args[0];
if (!File.Exists(filePath))
{
    Console.WriteLine($"Error: Archivo no encontrado: {filePath}");
    return 1;
}

string sourceCode = File.ReadAllText(filePath);

try
{
    var compilationService = new CompilationService();
    var compilationResult = compilationService.CompileCode(sourceCode);

    Console.WriteLine("=== COMPILACIÓN ===");
    Console.WriteLine(compilationResult.Output);

    if (!compilationResult.IsSuccessful || compilationResult.AST == null)
    {
        Console.WriteLine("\n✗ Compilación falló");
        return 1;
    }

    Console.WriteLine("\n✓ Compilación exitosa");

    var interpreter = new Interpreter((prompt) => null);
    interpreter.VerboseMode = false;

    var output = interpreter.Execute(compilationResult.AST);

    Console.WriteLine("\n=== SALIDA DEL PROGRAMA ===");
    if (output != null && output.Count > 0)
    {
        foreach (var line in output)
        {
            Console.WriteLine(line);
        }
    }
    else
    {
        Console.WriteLine("(Sin salida)");
    }

    return 0;
}
catch (Exception ex)
{
    Console.WriteLine($"\n=== ERROR ===");
    Console.WriteLine($"{ex.Message}");
    if (ex.InnerException != null)
    {
        Console.WriteLine($"Detalle: {ex.InnerException.Message}");
    }
    return 1;
}
