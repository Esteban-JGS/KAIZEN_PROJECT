namespace KaizenLang.Core;

// Límites con los que corre el playground web (KaizenLang.Web). Están en Core para que los tests
// ejecuten los ejemplos con los mismos valores que el navegador.
//
// La pila de WebAssembly es mucho más chica que la de escritorio y, si se agota, la aplicación se cae
// sin mensaje. Medido en Edge, se cae entre 570 y 650 niveles de evaluación anidados (unas 140 llamadas
// de una función simple, unas 47 si cada llamada anida bucles e if). En otro navegador basado en Chromium
// se cayó con unos 370 niveles y funcionó con 209, así que los topes quedan por debajo de eso.
public static class BrowserLimits
{
    // Tope de nodos evaluados: en el navegador no hay un timeout que corte un programa muy largo.
    public const long MaxExecutionSteps = 2_000_000;

    // Corta la recursión común con el mensaje habitual de profundidad máxima.
    public const int MaxCallDepth = 40;

    // Corta las funciones "pesadas" (bucles e if dentro de cada llamada) antes de que agoten la pila.
    public const int MaxNestingDepth = 200;

    public static Interpreter Apply(Interpreter interpreter)
    {
        interpreter.MaxExecutionSteps = MaxExecutionSteps;
        interpreter.MaxCallDepth = MaxCallDepth;
        interpreter.MaxNestingDepth = MaxNestingDepth;
        return interpreter;
    }
}
