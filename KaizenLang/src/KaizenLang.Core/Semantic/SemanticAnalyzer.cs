using System;
using System.Collections.Generic;
using System.Linq;

namespace KaizenLang.Core;

public class SemanticAnalyzer
{

    private readonly Stack<SymbolTable> scopes = new();
    private readonly Dictionary<string, FunctionSignature> functions = new(StringComparer.OrdinalIgnoreCase);
    private readonly Diagnostics diagnostics = new();
    private TypeResolver? typeResolver;
    private DeclarationChecker? declarationChecker;
    private CollectionValidator? collectionValidator;

    private readonly Dictionary<string, int> builtins = new(StringComparer.OrdinalIgnoreCase)
    {
    { ReservedWords.INPUT, 0 },
    { ReservedWords.OUTPUT, -1 },
    { "print", -1 },
    { "length", 1 },
    };

    public List<string> AnalyzeProgram(Node ast)
    {
        diagnostics.Clear();
        scopes.Clear();
        functions.Clear();
        PushScope();
        foreach (var kv in builtins)
        {
            functions[kv.Key] = new FunctionSignature { Name = kv.Key, Arity = kv.Value, IsBuiltin = true };
        }
        if (functions.ContainsKey(ReservedWords.INPUT))
            functions[ReservedWords.INPUT].ReturnType = TypeWords.GRIMOIRE;
        if (functions.ContainsKey("length"))
            functions["length"].ReturnType = TypeWords.GEAR;
        if (functions.ContainsKey(ReservedWords.OUTPUT))
            functions[ReservedWords.OUTPUT].ReturnType = ReservedWords.VOID;
        if (functions.ContainsKey("print"))
            functions["print"].ReturnType = "void";
        typeResolver = new TypeResolver(scopes, functions, builtins, diagnostics);
        declarationChecker = new DeclarationChecker(scopes, functions, builtins, typeResolver, diagnostics, VisitNode);
        collectionValidator = new CollectionValidator(typeResolver, diagnostics);
        foreach (var child in ast.Children)
        {
            VisitTopLevel(child);
        }
        ValidateAllIdentifiers(ast);
        return diagnostics.GetUnique();
    }

    private void ValidateAllIdentifiers(Node root)
    {
        if (root == null)
            return;
        foreach (var child in root.Children)
            ValidateNodeWithScope(child);
    }

    private void ValidateNodeWithScope(Node node)
    {
        if (node == null)
            return;

        if (string.Equals(node.Type, "Function", StringComparison.OrdinalIgnoreCase) || string.Equals(node.Type, "FunctionDeclaration", StringComparison.OrdinalIgnoreCase))
        {
            PushScope();
            var paramsNode = node.FindChild("Parameters");
            if (paramsNode != null)
            {
                foreach (var p in paramsNode.Children)
                {
                    var id = p.FindChild("Identifier") ?? p.FindChild("IDENTIFIER");
                    var pname = SemanticUtils.ExtractIdentifierName(id) ?? id?.Type;
                    var typeNode = p.Children.Count > 0 ? p.Children[0] : null;
                    var ptype = typeNode != null && typeNode.Children.Count > 0 ? typeNode.Children[0].Type : typeNode?.Type ?? string.Empty;
                    if (!string.IsNullOrEmpty(pname))
                        scopes.Peek().DeclareVariable(pname, ptype, p.Line);
                }
            }

            var body = node.FindChild("Body") ?? node.FindChild("Block");
            if (body != null)
                ValidateIdentifiersInExpression(body);

            PopScope();
            return;
        }

        ValidateIdentifiersInExpression(node);
        foreach (var c in node.Children)
            ValidateNodeWithScope(c);
    }

    private void VisitTopLevel(Node node)
    {
        switch (node.Type)
        {
            case "VariableDeclaration":
                declarationChecker?.RegisterVariable(node);
                collectionValidator?.CheckCollectionInitializer(node);
                break;
            case "Function":
            case "FunctionDeclaration":
                declarationChecker?.RegisterFunction(node);
                break;
            default:
                VisitNode(node);
                break;
        }
    }

    private void VisitNode(Node node)
    {
        if (node == null)
            return;

        switch (node.Type)
        {
            // Bloques y for abren su propio alcance, igual que en el intérprete: la misma variable
            // puede declararse en el if y en el else, o en dos for seguidos.
            case "Block":
            case "For":
                PushScope();
                try
                {
                    int start = 0;
                    if (node.Type == "For" && node.Children.Count > 1)
                    {
                        // La inicialización se visita primero para que la condición vea la variable del bucle.
                        CheckCondition(node.Children[1], ReservedWords.FOR, visitFirst: node.Children[0]);
                        start = 1;
                    }
                    for (int i = start; i < node.Children.Count; i++)
                        VisitNode(node.Children[i]);
                }
                finally
                {
                    PopScope();
                }
                break;
            case "If":
            case "While":
                if (node.Children.Count > 0)
                    CheckCondition(node.Children[0], node.Type == "If" ? ReservedWords.IF : ReservedWords.WHILE);
                foreach (var c in node.Children)
                    VisitNode(c);
                break;
            case "DoWhile":
                foreach (var c in node.Children)
                    VisitNode(c);
                if (node.Children.Count > 1)
                    CheckCondition(node.Children[1], $"{ReservedWords.DO}-{ReservedWords.WHILE}");
                break;
            case "Return":
                if (declarationChecker?.InsideFunction == true)
                    declarationChecker.CheckReturn(node);
                else
                    diagnostics.Report(node, "'return' solo puede usarse dentro de una función");
                foreach (var c in node.Children)
                    VisitNode(c);
                break;
            case "VariableDeclaration":
                declarationChecker?.RegisterVariable(node);
                collectionValidator?.CheckCollectionInitializer(node);
                break;
            case "Assignment":
                CheckAssignment(node);
                if (node.Children.Count > 1)
                    VisitNode(node.Children[1]);
                break;
            case "Identifier":
            case "IDENTIFIER":
                var sname = SemanticUtils.ExtractIdentifierName(node);
                if (!string.IsNullOrEmpty(sname) && !SemanticUtils.IsSymbolDefined(scopes, sname) && !functions.ContainsKey(sname))
                    diagnostics.Report(node, $"Variable '{sname}' no declarada");
                break;
            case "FunctionCall":
                ValidateFunctionCall(node);
                break;
            default:
                foreach (var c in node.Children)
                    VisitNode(c);
                break;
        }
    }

    // La condición de if/while/for/do-while debe ser shin cuando su tipo se puede inferir.
    // En el for, la condición usa la variable de la inicialización, que se registra primero.
    private void CheckCondition(Node condition, string statement, Node? visitFirst = null)
    {
        if (visitFirst != null)
            VisitNode(visitFirst);

        var type = typeResolver?.Resolve(condition);
        if (!string.IsNullOrEmpty(type) && !string.Equals(type, TypeWords.SHIN, StringComparison.OrdinalIgnoreCase))
            diagnostics.Report(condition, $"La condición del {statement} debe ser de tipo '{TypeWords.SHIN}', se encontró '{type}'");
    }

    // Asignación "destino = valor", donde el destino es una variable o un elemento (a[i], m[i][j]).
    // Se compara el tipo del destino con el de la expresión completa del lado derecho; antes solo se miraba
    // su primer operando, así que "x = x + 1.5" con x gear no se detectaba.
    private void CheckAssignment(Node node)
    {
        var target = node.Children.Count > 0 ? node.Children[0] : null;
        var value = node.Children.Count > 1 ? node.Children[1] : null;

        var baseIdentifier = target;
        while (baseIdentifier != null && baseIdentifier.Type == "IndexAccess" && baseIdentifier.Children.Count > 0)
            baseIdentifier = baseIdentifier.Children[0];

        var name = SemanticUtils.ExtractIdentifierName(baseIdentifier);
        if (string.IsNullOrEmpty(name))
            return;

        if (!SemanticUtils.IsSymbolDefined(scopes, name))
        {
            diagnostics.Report(node, $"Variable '{name}' no declarada");
            return;
        }

        var targetType = typeResolver?.Resolve(target);
        var valueType = typeResolver?.Resolve(value);
        if (!string.IsNullOrEmpty(targetType) && !string.IsNullOrEmpty(valueType) && !CanAssign(targetType, valueType))
            diagnostics.Report(node, $"Tipos incompatibles para asignación: {targetType} y {valueType}");
    }

    private void ValidateFunctionCall(Node fnCallNode)
    {
        var fnameNode = fnCallNode.FindChild("FunctionName");
        var fname = SemanticUtils.ExtractIdentifierName(fnameNode) ?? fnameNode?.Children.FirstOrDefault()?.Type;

        if (string.IsNullOrEmpty(fname))
        {
            diagnostics.Report(fnCallNode, "Llamada a función sin nombre");
            return;
        }

        var argsNode = fnCallNode.FindChild("Arguments");
        int argCount = argsNode?.Children.Count ?? 0;

        if (builtins.TryGetValue(fname, out var expected))
        {
            // Aridad 0 en un builtin acepta un argumento opcional: input() o input("mensaje").
            if (expected == 0 && argCount > 1)
                diagnostics.Report(fnCallNode, $"La función '{fname}' espera 0 o 1 argumentos, recibió {argCount}");
            else if (expected > 0 && argCount != expected)
                diagnostics.Report(fnCallNode, $"La función '{fname}' espera {expected} argumentos, recibió {argCount}");
        }
        else if (functions.TryGetValue(fname, out var sig))
        {
            if (sig.Arity >= 0 && sig.Arity != argCount)
                diagnostics.Report(fnCallNode, $"La función '{fname}' espera {sig.Arity} argumentos, recibió {argCount}");
        }
        else
        {
            diagnostics.Report(fnCallNode, $"Función '{fname}' no definida");
        }

        if (argsNode != null)
        {
            foreach (var a in argsNode.Children)
            {
                VisitNode(a);
                typeResolver?.Resolve(a); // valida tipos dentro del argumento (p. ej. comparaciones)
            }
        }
    }

    private void PushScope()
    {
        scopes.Push(new SymbolTable());
    }

    private void PopScope()
    {
        if (scopes.Count > 0)
            scopes.Pop();
    }

    private static bool CanAssign(string targetType, string sourceType) => SemanticUtils.CanAssign(targetType, sourceType);

    private void ValidateIdentifiersInExpression(Node? expr)
    {
        if (expr == null)
            return;

        // Una declaración local (p. ej. dentro de una función) introduce su nombre en el alcance actual.
        // Antes se recorría como si fuera un uso y daba "no declarada" en la misma línea de la declaración.
        if (expr.Type == "VariableDeclaration")
        {
            for (int i = 2; i < expr.Children.Count; i++)
                ValidateIdentifiersInExpression(expr.Children[i]);

            var declared = SemanticUtils.ExtractIdentifierName(expr.FindChild("Identifier"));
            if (!string.IsNullOrEmpty(declared) && scopes.Count > 0)
                scopes.Peek().DeclareVariable(declared, string.Empty, expr.Line);
            return;
        }

        if (expr.Type == "IDENTIFIER" || expr.Type == "Identifier")
        {
            var name = SemanticUtils.ExtractIdentifierName(expr);
            if (!string.IsNullOrEmpty(name) && !SemanticUtils.IsSymbolDefined(scopes, name) && !functions.ContainsKey(name) && !builtins.ContainsKey(name))
            {
                diagnostics.Report(expr, $"Variable '{name}' no declarada");
            }
            return;
        }

        if (expr.Type == "FunctionCall")
        {
            var fname = SemanticUtils.ExtractIdentifierName(expr.FindChild("FunctionName"));
            if (!string.IsNullOrEmpty(fname) && !functions.ContainsKey(fname) && !builtins.ContainsKey(fname))
                diagnostics.Report(expr, $"Función '{fname}' no definida");

            var args = expr.FindChild("Arguments");
            if (args != null)
            {
                foreach (var a in args.Children)
                    ValidateIdentifiersInExpression(a);
            }
            return;
        }

        if (expr.Children != null)
        {
            foreach (var c in expr.Children)
                ValidateIdentifiersInExpression(c);
        }
    }
}
