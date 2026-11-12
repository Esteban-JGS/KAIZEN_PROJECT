using System;
using System.IO;
using KaizenLang.Core;
using KaizenLang.UI.Services;

class Program
{
    static int Main(string[] args)
    {
        // Enhanced runner output formatting (C++-like compilation + console)
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: IDERunner <source file> [--verbose]");
            return 1;
        }

        // Detect --verbose flag
        bool verbose = args.Any(a => a.Equals("--verbose", StringComparison.OrdinalIgnoreCase));

        // First non-flag arg is the path
        var path = args.FirstOrDefault(a => !a.StartsWith("--", StringComparison.OrdinalIgnoreCase));
        if (!File.Exists(path))
        {
            Console.WriteLine($"File not found: {path}");
            return 2;
        }

        var code = File.ReadAllText(path);
        var lineCount = code.Split(new[] { '\n' }, StringSplitOptions.None).Length;

        var execStart = DateTime.UtcNow;

        // Compile
        var cs = new CompilationService();
        var res = cs.CompileCode(code);

        // Compose header similar to C++ compilation output and the console block you provided
        Console.WriteLine("🚀 INICIANDO EJECUCIÓN");
        Console.WriteLine(new string('═', 60));
        Console.WriteLine();
        Console.WriteLine($"⏱ Inicio ejecución: {execStart:O} (UTC)");
        Console.WriteLine($"• Fuente: {Path.GetFileName(path)} | Líneas: {lineCount}");

        var totalErrors = 0;
        totalErrors += res.LexicalErrors?.Count ?? 0;
        totalErrors += res.SyntaxErrors?.Count ?? 0;
        totalErrors += res.SemanticErrors?.Count ?? 0;
        if (res.InternalError != null)
            totalErrors++;

        Console.WriteLine($"• Compilación: {(res.IsSuccessful ? "OK" : "FAILED")} | Errores: {totalErrors}");
        Console.WriteLine();

        // Print compilation output details but keep it compact
        Console.WriteLine("▶ EJECUTANDO PROGRAMA...");
        Console.WriteLine(new string('─', 40));

        var execStopwatch = System.Diagnostics.Stopwatch.StartNew();

        // If compilation produced verbose output, show a short excerpt (first 2000 chars)
        if (!string.IsNullOrEmpty(res.Output))
        {
            // Print compact compilation summary
            Console.WriteLine();
            Console.WriteLine(res.Output);
        }

        // If compilation failed, show errors and abort
        if (!res.IsSuccessful || res.AST == null)
        {
            Console.WriteLine();
            Console.WriteLine("Compilation failed. Fix errors and try again.");
            return 3;
        }

        // Execute the program
        var interpreter = new Interpreter();
        var outputs = interpreter.Execute(res.AST);

        execStopwatch.Stop();
        var execEnd = DateTime.UtcNow;

        Console.WriteLine();
        Console.WriteLine($"⏱ Fin ejecución: {execEnd:O} (UTC)");
        Console.WriteLine();

        // Program output block with numbering and [OUT] prefix
        Console.WriteLine("📤 SALIDA DEL PROGRAMA:");
        if (outputs == null || outputs.Count == 0)
        {
            Console.WriteLine(" (no program output)");
        }
        else
        {
            int printed = 0;
            for (int i = 0; i < outputs.Count; i++)
            {
                var raw = outputs[i] ?? string.Empty;
                var trimmed = raw.Trim();

                // Skip debug lines unless verbose is enabled
                if (trimmed.StartsWith("[DBG]", StringComparison.OrdinalIgnoreCase) && !verbose)
                    continue;

                // Remove common logger prefixes if present
                foreach (var prefix in new[] { "[OUT] ", "[INF] ", "[WRN] ", "[ERR] " })
                {
                    if (trimmed.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    {
                        trimmed = trimmed.Substring(prefix.Length).TrimStart();
                        break;
                    }
                }

                // Also remove any single-character unicode markers sometimes present (e.g., '�')
                trimmed = trimmed.TrimStart('\uFFFD');

                var idx = (printed + 1).ToString().PadLeft(2, '0');
                Console.WriteLine($" {idx}. {trimmed}");
                printed++;
            }

            if (printed == 0)
            {
                Console.WriteLine(" (no program output)");
            }
        }

        Console.WriteLine();
        Console.WriteLine("🎯 EJECUCIÓN COMPLETADA");
        Console.WriteLine($"✅ Estado: {(res.IsSuccessful ? "Éxito" : "Falló")}");
        Console.WriteLine($"⏱️ Tiempo de ejecución: {execStopwatch.ElapsedMilliseconds}ms");
        Console.WriteLine($"🔧 Tiempo de compilación: {res.CompilationTime.TotalMilliseconds}ms");

        return 0;
    }
}
