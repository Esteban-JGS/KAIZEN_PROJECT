using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KaizenLang.Core;
using KaizenLang.UI.Services;
using Xunit;

namespace KaizenLang.Tests;

// Casos que antes fallaban: cada test describe el comportamiento esperado del lenguaje.
public class RegressionTests
{
    private static List<string> Run(string code)
    {
        var result = new CompilationService().CompileCode(code);
        var errors = (result.SyntaxErrors ?? new List<string>()).Concat(result.SemanticErrors ?? new List<string>());
        Assert.True(result.IsSuccessful, "La compilación debería ser exitosa: " + string.Join(" | ", errors));
        return new Interpreter().Execute(result.AST!);
    }

    private static List<string> CompileErrors(string code)
    {
        // Con timeout: algunos errores de sintaxis colgaban el parser.
        var task = Task.Run(() => new CompilationService().CompileCode(code));
        Assert.True(task.Wait(TimeSpan.FromSeconds(5)), "El compilador no terminó (posible bucle infinito)");
        var result = task.Result;
        Assert.False(result.IsSuccessful, "La compilación debería fallar");
        Assert.Null(result.InternalError);
        return (result.SyntaxErrors ?? new List<string>()).Concat(result.SemanticErrors ?? new List<string>()).ToList();
    }

    [Fact]
    public void Arguments_AreEvaluatedInCallerScope()
    {
        var output = Run(@"
gear sw(gear a, gear b) ying
    output(a + "" "" + b);
    return 0;
yang
gear a = 1;
gear b = 2;
gear r = sw(b, a);");
        Assert.Equal(new[] { "2 1" }, output);
    }

    [Fact]
    public void BuiltinCall_InsideBinaryExpression_KeepsTheRestOfTheExpression()
    {
        var output = Run(@"
chainsaw<gear> arr = [10, 20, 30];
gear n = length(arr) - 1;
output(n);
output(length(arr) + 100);");
        Assert.Equal(new[] { "2", "103" }, output);
    }

    [Fact]
    public void IntegerArithmetic_UsesIntegerDivisionAndModulo()
    {
        var output = Run(@"
gear a = 7;
gear b = 2;
output(a / b);
output(a % b);
output(((9 / 2) * 2) == 9);");
        Assert.Equal(new[] { "3", "1", "false" }, output);
    }

    [Fact]
    public void Output_UsesLanguageNotation_RegardlessOfCulture()
    {
        var previous = System.Globalization.CultureInfo.CurrentCulture;
        System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo("es-CR");
        try
        {
            var output = Run(@"
shikai p = 19.99;
output(""p = "" + p);
output(true);
chainsaw<gear> a = [1, 2, 3];
output(a);");
            Assert.Equal(new[] { "p = 19.99", "true", "[1, 2, 3]" }, output);
        }
        finally
        {
            System.Globalization.CultureInfo.CurrentCulture = previous;
        }
    }

    [Fact]
    public void DoWhile_RepeatsUntilConditionIsFalse()
    {
        var output = Run(@"
gear k = 0;
do ying
    k = k + 1;
yang while (k < 3);
output(k);");
        Assert.Equal(new[] { "3" }, output);
    }

    [Fact]
    public void LogicalOperators_ShortCircuit()
    {
        var output = Run(@"
chainsaw<gear> arr = [1, 2];
gear i = 5;
if (i < length(arr) && arr[i] == 1) ying
    output(""si"");
yang else ying
    output(""no"");
yang");
        Assert.Equal(new[] { "no" }, output);
    }

    [Fact]
    public void Functions_DoNotSeeCallerLocals()
    {
        var errors = CompileErrors(@"
void f() ying
    output(secreto);
yang
void g() ying
    gear secreto = 42;
    f();
yang
g();");
        Assert.Contains(errors, e => e.Contains("'secreto' no declarada") && e.Contains("l3:"));
        Assert.DoesNotContain(errors, e => e.Contains("l6:"));
    }

    [Fact]
    public void SiblingScopes_CanReuseVariableNames()
    {
        var output = Run(@"
for (gear i = 0; i < 2; i++) ying
    output(i);
yang
for (gear i = 0; i < 1; i++) ying
    output(i);
yang
if (true) ying
    gear t = 1;
    output(t);
yang else ying
    gear t = 2;
    output(t);
yang");
        Assert.Equal(new[] { "0", "1", "0", "1" }, output);
    }

    [Fact]
    public void NestingLimit_StopsDeepRecursionWithAMessage()
    {
        // El playground web usa este tope para no agotar la pila de WebAssembly, que es mucho más chica.
        var result = new CompilationService().CompileCode(@"
gear f(gear n) ying
    if (n <= 0) ying
        return 0;
    yang
    return f(n - 1) + 1;
yang
output(f(1000));");
        Assert.True(result.IsSuccessful);
        var output = new Interpreter { MaxCallDepth = 5000, MaxNestingDepth = 200 }.Execute(result.AST!);
        Assert.Single(output);
        Assert.Contains("profundidad máxima de recursión que admite este entorno", output[0]);
    }

    [Fact]
    public void StringsAndIdentifiersContainingError_AreNotSyntaxErrors()
    {
        var output = Run(@"
gear numErrores = 2;
output(""Error grave"");
output(numErrores);");
        Assert.Equal(new[] { "Error grave", "2" }, output);
    }

    [Theory]
    [InlineData("chainsaw<gear> x = [1, 2;", "Se esperaba ',' o ']'")]
    [InlineData("output(\"a\";", "Se esperaba ',' o ')'")]
    [InlineData("while", "Se esperaba '('")]
    [InlineData("gear f(gear a", "Se esperaba ')'")]
    [InlineData("if (true) ying\n output(1);", "Bloque no cerrado")]
    [InlineData("gear = 10;", "falta el nombre de la variable")]
    public void MalformedInput_ReportsSyntaxErrorWithoutHangingOrCrashing(string code, string expected)
    {
        var errors = CompileErrors(code);
        Assert.Contains(errors, e => e.Contains(expected));
    }

    [Fact]
    public void MissingSemicolon_IsReportedOnTheLineWhereItIsMissing()
    {
        var errors = CompileErrors("output(\"Hola\")\ngear x = 5;");
        Assert.Equal(new[] { "Se esperaba ';' después de la expresión. (línea 1, columna 15)" }, errors);
    }

    [Theory]
    [InlineData("gear suma(gear a, gear b) ying return a + b; yang\ngear x = suma(10);", "espera 2 argumentos")]
    [InlineData("gear f() ying return \"texto\"; yang", "Tipo de retorno incompatible")]
    [InlineData("gear f(gear x) ying gear y = x; yang", "debe devolver un valor")]
    [InlineData("chainsaw<gear> n = [1, 2];\ngear v = n[\"a\"];", "El índice debe ser de tipo 'gear'")]
    [InlineData("gear a = 1;\ngrimoire b = \"1\";\nshin c = (a == b);", "Comparación entre tipos incompatibles")]
    [InlineData("gear x = 0;\nx = x + 1.5;", "Tipos incompatibles para asignación")]
    [InlineData("return 1;", "'return' solo puede usarse dentro de una función")]
    public void SemanticErrors_AreDetected(string code, string expected)
    {
        var errors = CompileErrors(code);
        Assert.Contains(errors, e => e.Contains(expected));
    }
}
