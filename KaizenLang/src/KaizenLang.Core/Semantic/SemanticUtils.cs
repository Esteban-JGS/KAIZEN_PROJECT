using System;
using System.Collections.Generic;
using System.Linq;

namespace KaizenLang.Core;

public static class SemanticUtils
{
    public static string? ExtractIdentifierName(Node? idNode)
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

    // Agrega a 'found' todos los nodos del subárbol con el tipo indicado.
    public static void CollectNodes(Node? node, string type, List<Node> found)
    {
        if (node == null)
            return;
        if (node.Type == type)
            found.Add(node);
        foreach (var child in node.Children)
            CollectNodes(child, type, found);
    }

    public static bool IsSymbolDefined(IEnumerable<SymbolTable> scopes, string name)
    {
        foreach (var scope in scopes)
        {
            if (scope.LookupVariable(name) != null)
                return true;
        }
        return false;
    }

    public static bool IsNullLiteralType(string? typeName)
    {
        return string.Equals(typeName, LiteralWords.NULL, StringComparison.OrdinalIgnoreCase);
    }

    public static bool SupportsNullAssignment(string? targetType)
    {
        if (string.IsNullOrWhiteSpace(targetType))
            return false;

        var normalized = targetType.Trim();

        if (normalized.StartsWith(TypeWords.CHAINSAW, StringComparison.OrdinalIgnoreCase) ||
            normalized.StartsWith(TypeWords.HOGYOKU, StringComparison.OrdinalIgnoreCase))
            return true;

        return string.Equals(normalized, TypeWords.GRIMOIRE, StringComparison.OrdinalIgnoreCase);
    }

    // Reglas de asignación compartidas por declaraciones y asignaciones:
    // mismo tipo, null hacia tipos que lo admiten, y ensanchamiento numérico gear -> shikai -> bankai.
    public static bool CanAssign(string targetType, string sourceType)
    {
        if (IsNullLiteralType(sourceType))
            return IsNullLiteralType(targetType) || SupportsNullAssignment(targetType);

        if (string.Equals(targetType, sourceType, StringComparison.OrdinalIgnoreCase))
            return true;

        var targetRank = NumericRank(targetType);
        var sourceRank = NumericRank(sourceType);
        return sourceRank > 0 && targetRank >= sourceRank;
    }

    // 0 si no es numérico; si no, el orden de ensanchamiento gear (1) < shikai (2) < bankai (3).
    public static int NumericRank(string? typeName)
    {
        if (string.Equals(typeName, TypeWords.GEAR, StringComparison.OrdinalIgnoreCase))
            return 1;
        if (string.Equals(typeName, TypeWords.SHIKAI, StringComparison.OrdinalIgnoreCase))
            return 2;
        if (string.Equals(typeName, TypeWords.BANKAI, StringComparison.OrdinalIgnoreCase))
            return 3;
        return 0;
    }
}
