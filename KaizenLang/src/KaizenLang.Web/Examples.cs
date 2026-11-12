using System.Reflection;

namespace KaizenLang.Web;

public sealed record Example(string Id, string Title, string Code, string Input);

// Ejemplos embebidos desde KaizenLang/examples (ver KaizenLang.Web.csproj).
public static class Examples
{
    private static readonly (string File, string Title)[] Catalog =
    {
        ("test-01-variables-basicas", "Variables y tipos"),
        ("test-02-operaciones-aritmeticas", "Operaciones aritméticas"),
        ("test-03-operaciones-logicas", "Operaciones lógicas"),
        ("test-04-estructuras-if", "Condicionales if / else"),
        ("test-05-estructuras-while", "Bucles while"),
        ("test-06-estructuras-for", "Bucles for"),
        ("test-07-arrays-basicos", "Chainsaw (arreglos)"),
        ("test-08-matrices", "Hogyoku (matrices)"),
        ("test-09-funciones-basicas", "Funciones básicas"),
        ("test-10-funciones-avanzadas", "Funciones avanzadas"),
        ("test-11-input-output", "Entrada y salida"),
        ("test-12-programa-completo", "Programa completo"),
        ("test-15-casos-limite", "Casos límite"),
        ("test-16-stress-integral", "Prueba de estrés"),
        ("test-infinite-recursion", "Recursión infinita (error esperado)"),
        ("test-syntax-errors", "Errores de sintaxis (error esperado)"),
    };

    public static IReadOnlyList<Example> All { get; } = Load();

    private static IReadOnlyList<Example> Load()
    {
        var assembly = typeof(Examples).Assembly;
        var list = new List<Example>();
        foreach (var (file, title) in Catalog)
        {
            var code = Read(assembly, $"examples/{file}.txt");
            if (code == null)
                continue;
            var inputName = file.Replace("-input-output", "-input");
            var input = Read(assembly, $"inputs/{inputName}.txt") ?? string.Empty;
            list.Add(new Example(file, title, code, input));
        }
        return list;
    }

    private static string? Read(Assembly assembly, string name)
    {
        using var stream = assembly.GetManifestResourceStream(name);
        if (stream == null)
            return null;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd().Replace("\r\n", "\n");
    }
}
