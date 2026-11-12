using System;
using KaizenLang.Core;
using KaizenLang.UI.Services;
using Xunit;

namespace KaizenLang.Tests;

public class FunctionTests
{
    [Fact]
    public void FunctionExecution_LogsAndResult()
    {
        var code = @"void saludar() ying
    output(""¡Hola desde una función!"");
yang

saludar();

gear sumar(gear a, gear b) ying
    return a + b;
yang

gear resultado = sumar(5, 3);
output(""El resultado de la suma es: "" + resultado);
";

        var cs = new CompilationService();
        var res = cs.CompileCode(code);
        Assert.True(res.IsSuccessful, "Compilation should succeed");

        // VerboseMode hace que el intérprete agregue las trazas [DBG] a su propia salida.
        // Se verifica esa lista en vez de Console, que es global y la comparten tests en paralelo.
        var interpreter = new Interpreter { VerboseMode = true };
        var programOutputs = interpreter.Execute(res.AST!);
        var joined = string.Join("\n", programOutputs);

        Assert.Contains("Invoking function 'saludar'", joined);
        Assert.Contains("Invoking function 'sumar'", joined);
        Assert.Contains("¡Hola desde una función!", joined);
        Assert.Contains("El resultado de la suma es: 8", joined);
    }
}
