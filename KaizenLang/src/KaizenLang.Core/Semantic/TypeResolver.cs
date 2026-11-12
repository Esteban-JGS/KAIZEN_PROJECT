using System;
using System.Collections.Generic;

namespace KaizenLang.Core;

// Responsible for resolving static types of expression AST nodes.
public class TypeResolver
{
    private readonly IEnumerable<SymbolTable> _scopes;
    private readonly IDictionary<string, FunctionSignature> _functions;
    private readonly IDictionary<string, int> _builtins;
    private readonly Diagnostics _diagnostics;

    public TypeResolver(IEnumerable<SymbolTable> scopes, IDictionary<string, FunctionSignature> functions, IDictionary<string, int> builtins, Diagnostics diagnostics)
    {
        _scopes = scopes;
        _functions = functions;
        _builtins = builtins;
        _diagnostics = diagnostics;
    }

    public string? Resolve(Node? expr)
    {
        if (expr == null)
            return null;

        switch (expr.Type)
        {
            case "INT":
                return TypeWords.GEAR;
            case "FLOAT":
                return TypeWords.SHIKAI;
            case "STRING":
                return TypeWords.GRIMOIRE;
            case "LITERAL":
                if (expr.Children.Count > 0)
                {
                    var lit = expr.Children[0].Type;
                    if (lit == LiteralWords.TRUE || lit == LiteralWords.FALSE)
                        return TypeWords.SHIN;
                    if (lit == LiteralWords.NULL)
                        return LiteralWords.NULL;
                }
                return TypeWords.GRIMOIRE;
            case "IDENTIFIER":
            case "Identifier":
                var name = ExtractIdentifierName(expr);
                if (string.IsNullOrEmpty(name))
                    return null;
                foreach (var scope in _scopes)
                {
                    // Declarada pero con tipo aún desconocido: no es un error, solo no se puede inferir.
                    var si = scope.LookupVariable(name);
                    if (si != null)
                        return string.IsNullOrEmpty(si.Type) ? null : si.Type;
                }
                _diagnostics.Report(expr, $"Variable '{name}' no declarada");
                return null;
            case "FunctionCall":
                var fnameNode = expr.FindChild("FunctionName");
                var fname = ExtractIdentifierName(fnameNode);
                if (string.IsNullOrEmpty(fname))
                    return null;
                if (_functions.TryGetValue(fname, out var sig))
                    return string.IsNullOrEmpty(sig.ReturnType) ? null : sig.ReturnType;
                if (_builtins.ContainsKey(fname))
                {
                    if (string.Equals(fname, ReservedWords.INPUT, StringComparison.OrdinalIgnoreCase))
                        return TypeWords.GRIMOIRE;
                    return null;
                }
                _diagnostics.Report(expr, $"Función '{fname}' no definida");
                return null;
            case "UnaryExpression":
                if (expr.Children.Count < 2)
                    return null;
                var unaryOp = GetOperatorSymbol(expr.Children[0]);
                var operandType = Resolve(expr.Children[1]);
                if (unaryOp == OperatorWords.NOT)
                    return TypeWords.SHIN;
                return SemanticUtils.NumericRank(operandType) > 0 ? operandType : null;
            case "Parentheses":
            case "ExpressionStatement":
                if (expr.Children.Count > 0)
                    return Resolve(expr.Children[0]);
                return null;
            case "ArrayLiteral":
                var elements = expr.FindChild("Elements");
                if (elements == null || elements.Children.Count == 0)
                    return TypeWords.CHAINSAW;
                string? eltType = null;
                var sawNullLiteral = false;
                foreach (var el in elements.Children)
                {
                    var et = Resolve(el);
                    if (et == null)
                        return null;
                    if (SemanticUtils.IsNullLiteralType(et))
                    {
                        sawNullLiteral = true;
                        continue;
                    }
                    if (eltType == null)
                        eltType = et;
                    else if (!string.Equals(eltType, et, StringComparison.OrdinalIgnoreCase))
                        return null;
                }
                if (eltType == null)
                    eltType = sawNullLiteral ? LiteralWords.NULL : "?";
                return $"{TypeWords.CHAINSAW}<{eltType}>";
            case "IndexAccess":
                if (expr.Children != null && expr.Children.Count >= 1)
                {
                    if (expr.Children.Count >= 2)
                    {
                        var indexType = Resolve(expr.Children[1]);
                        if (!string.IsNullOrEmpty(indexType) && !string.Equals(indexType, TypeWords.GEAR, StringComparison.OrdinalIgnoreCase))
                            _diagnostics.Report(expr, $"El índice debe ser de tipo '{TypeWords.GEAR}', se encontró '{indexType}'");
                    }

                    var target = expr.Children[0];
                    var ttype = Resolve(target);
                    if (string.IsNullOrEmpty(ttype))
                        return null;
                    if (ttype.StartsWith($"{TypeWords.CHAINSAW}<", StringComparison.OrdinalIgnoreCase))
                    {
                        var inner = ExtractInnerType(ttype);
                        if (!string.IsNullOrEmpty(inner))
                            return inner;
                        return null;
                    }
                    if (ttype.StartsWith($"{TypeWords.CHAINSAW}<{TypeWords.CHAINSAW}<", StringComparison.OrdinalIgnoreCase))
                    {
                        var inner = ExtractInnerType(ttype);
                        if (!string.IsNullOrEmpty(inner))
                            return inner;
                    }
                }
                return null;
            default:
                if (expr.Type == "Expression")
                {
                    if (expr.Children != null && expr.Children.Count == 1)
                        return Resolve(expr.Children[0]);
                }
                if (expr.Children != null && expr.Children.Count == 3)
                {
                    var left = Resolve(expr.Children[0]);
                    var op = expr.Children[1];
                    var right = Resolve(expr.Children[2]);
                    var opSymbol = GetOperatorSymbol(op);

                    // '+' con un texto concatena, sea cual sea el otro operando.
                    if (opSymbol == OperatorWords.ADD &&
                        (string.Equals(left, TypeWords.GRIMOIRE, StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(right, TypeWords.GRIMOIRE, StringComparison.OrdinalIgnoreCase)))
                        return TypeWords.GRIMOIRE;

                    return ResolveBinary(expr, left, opSymbol, right);
                }
                return null;
        }
    }

    // Tipo de "izq op der". Si un lado es desconocido (null) no se reporta nada: solo se valida lo que se puede inferir.
    private string? ResolveBinary(Node expr, string? left, string opSymbol, string? right)
    {
        bool known = !string.IsNullOrEmpty(left) && !string.IsNullOrEmpty(right);
        int leftRank = SemanticUtils.NumericRank(left);
        int rightRank = SemanticUtils.NumericRank(right);

        switch (opSymbol)
        {
            case OperatorWords.ADD:
            case OperatorWords.SUBTRACT:
            case OperatorWords.MULTIPLY:
            case OperatorWords.DIVIDE:
            case OperatorWords.MODULO:
                if (!known)
                    return null;
                if (leftRank == 0 || rightRank == 0)
                {
                    _diagnostics.Report(expr, $"Operación '{opSymbol}' no válida entre '{left}' y '{right}'");
                    return null;
                }
                // El resultado toma el tipo numérico más amplio: gear < shikai < bankai.
                return leftRank >= rightRank ? left : right;

            case OperatorWords.EQUAL:
            case OperatorWords.NOT_EQUAL:
                if (known && !AreComparable(left!, right!))
                    _diagnostics.Report(expr, $"Comparación entre tipos incompatibles: '{left}' y '{right}'");
                return TypeWords.SHIN;

            case OperatorWords.LESS:
            case OperatorWords.GREATER:
            case OperatorWords.LESS_EQUAL:
            case OperatorWords.GREATER_EQUAL:
                if (known && (leftRank == 0 || rightRank == 0))
                    _diagnostics.Report(expr, $"El operador '{opSymbol}' requiere números, se encontró '{left}' y '{right}'");
                return TypeWords.SHIN;

            case OperatorWords.AND:
            case OperatorWords.OR:
                foreach (var side in new[] { left, right })
                {
                    if (!string.IsNullOrEmpty(side) && !string.Equals(side, TypeWords.SHIN, StringComparison.OrdinalIgnoreCase))
                        _diagnostics.Report(expr, $"El operador '{opSymbol}' requiere valores shin, se encontró '{side}'");
                }
                return TypeWords.SHIN;

            default:
                return null;
        }
    }

    // == y != aceptan números entre sí, tipos iguales, o null contra un tipo que admite null.
    private static bool AreComparable(string left, string right)
    {
        if (string.Equals(left, right, StringComparison.OrdinalIgnoreCase))
            return true;
        if (SemanticUtils.NumericRank(left) > 0 && SemanticUtils.NumericRank(right) > 0)
            return true;
        if (SemanticUtils.IsNullLiteralType(left))
            return SemanticUtils.SupportsNullAssignment(right);
        if (SemanticUtils.IsNullLiteralType(right))
            return SemanticUtils.SupportsNullAssignment(left);
        return false;
    }

    public static string ExtractInnerType(string typeStr)
    {
        if (string.IsNullOrEmpty(typeStr))
            return string.Empty;
        var open = typeStr.IndexOf('<');
        var close = typeStr.LastIndexOf('>');
        if (open >= 0 && close > open)
        {
            return typeStr.Substring(open + 1, close - open - 1);
        }
        return string.Empty;
    }

    private static string GetOperatorSymbol(Node? opNode)
    {
        if (opNode == null)
            return string.Empty;
        // El parser guarda el símbolo como hijo del nodo "Operator"; antes se devolvía "Operator"
        // y ninguna expresión binaria llegaba a tener tipo.
        if (opNode.Type == "Operator" && opNode.Children.Count > 0)
            return opNode.Children[0].Type;
        if (opNode.Value != null)
            return opNode.Value.ToString() ?? opNode.Type;
        return opNode.Type ?? string.Empty;
    }

    // Local helper to extract identifier text from various node shapes
    private static string? ExtractIdentifierName(Node? idNode)
    {
        if (idNode == null)
            return null;
        if (idNode.Type == "Identifier" || idNode.Type == "IDENTIFIER")
        {
            if (idNode.Value != null)
                return idNode.Value.ToString();
            if (idNode.Children.Count > 0)
                return idNode.Children[0].Value?.ToString() ?? idNode.Children[0].Type;
        }
        if (idNode.Type == "FunctionName")
        {
            if (idNode.Children.Count > 0)
            {
                var c = idNode.Children[0];
                if (c.Value != null)
                    return c.Value.ToString();
                if (c.Children.Count > 0)
                    return c.Children[0].Value?.ToString() ?? c.Children[0].Type;
                return c.Type;
            }
        }
        if (idNode.Value != null)
            return idNode.Value.ToString();
        return idNode.Children.FirstOrDefault()?.Value?.ToString() ?? idNode.Children.FirstOrDefault()?.Type;
    }
}
