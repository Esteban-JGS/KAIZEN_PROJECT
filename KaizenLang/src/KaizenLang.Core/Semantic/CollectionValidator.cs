using System;
using System.Collections.Generic;

namespace KaizenLang.Core;

public class CollectionValidator
{
    private readonly Diagnostics diagnostics;
    private readonly TypeResolver typeResolver;

    public CollectionValidator(TypeResolver typeResolver, Diagnostics diagnostics)
    {
        this.typeResolver = typeResolver;
        this.diagnostics = diagnostics;
    }

    public void CheckCollectionInitializer(Node varDecl)
    {
        if (varDecl.Children.Count < 3)
            return;

        var typeNode = varDecl.Children[0];
        string declaredType = typeNode.Type ?? string.Empty;
        string elemType = string.Empty;
        if (typeNode.Children.Count > 0)
            elemType = typeNode.Children[0].Type ?? string.Empty;

        if (string.IsNullOrEmpty(declaredType))
            return;

        var initNode = varDecl.Children[2];
        Node? arrayLiteral = FindDescendant(initNode, "ArrayLiteral");
        if (arrayLiteral == null)
            return;

        if (declaredType.IndexOf(TypeWords.HOGYOKU, StringComparison.OrdinalIgnoreCase) >= 0)
        {
            var elements = arrayLiteral.FindChild("Elements");
            if (elements == null)
                return;
            int? expectedCols = null;
            int rowIndex = 0;
            foreach (var rowExpr in elements.Children)
            {
                var rowArr = rowExpr.FindChild("ArrayLiteral") ?? (rowExpr.Type == "ArrayLiteral" ? rowExpr : null);
                if (rowArr == null)
                {
                    diagnostics.Report(rowExpr, $"Hogyoku no rectangular: fila {rowIndex} no es una fila (se esperaba chainsaw)");
                    rowIndex++;
                    continue;
                }

                var rowEls = rowArr.FindChild("Elements");
                int cols = rowEls?.Children.Count ?? 0;
                if (expectedCols == null)
                    expectedCols = cols;
                else if (expectedCols != cols)
                    diagnostics.Report(rowArr, $"Hogyoku no rectangular: longitudes de fila inconsistentes (esperado {expectedCols}, fila {rowIndex} tiene {cols})");

                if (!string.IsNullOrEmpty(elemType) && rowEls != null)
                {
                    int col = 0;
                    foreach (var elExpr in rowEls.Children)
                    {
                        ValidateElementType(elExpr, elemType, $"fila {rowIndex}, col {col}", isMatrix: true);
                        col++;
                    }
                }

                rowIndex++;
            }
        }
        else if (declaredType.IndexOf(TypeWords.CHAINSAW, StringComparison.OrdinalIgnoreCase) >= 0)
        {
            var elements = arrayLiteral.FindChild("Elements");
            if (elements == null)
                return;
            if (!string.IsNullOrEmpty(elemType))
            {
                int idx = 0;
                foreach (var elExpr in elements.Children)
                {
                    ValidateElementType(elExpr, elemType, $"índice {idx}", isMatrix: false);
                    idx++;
                }
            }
        }
    }

    private Node? FindDescendant(Node root, string type)
    {
        if (root == null)
            return null;
        if (root.Type == type)
            return root;
        foreach (var c in root.Children)
        {
            var found = FindDescendant(c, type);
            if (found != null)
                return found;
        }
        return null;
    }

    private void ValidateElementType(Node elementExpr, string expectedType, string locationLabel, bool isMatrix = false)
    {
        var actualType = typeResolver.Resolve(elementExpr);
        if (string.IsNullOrEmpty(actualType))
            return;

        if (IsNullLiteral(actualType) || IsTypeCompatible(expectedType, actualType))
            return;

        var target = isMatrix ? "hogyoku" : "chainsaw";
        diagnostics.Report(elementExpr, $"Tipo incompatible en inicialización de {target}: se esperaba '{expectedType}' pero se encontró '{actualType}' ({locationLabel})");
    }

    private static bool IsNullLiteral(string? typeName)
    {
        return string.Equals(typeName, LiteralWords.NULL, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsTypeCompatible(string expected, string actual)
    {
        if (string.Equals(expected, actual, StringComparison.OrdinalIgnoreCase))
            return true;

        if (IsNumericType(expected) && IsNumericType(actual))
        {
            if (string.Equals(expected, TypeWords.SHIKAI, StringComparison.OrdinalIgnoreCase) && string.Equals(actual, TypeWords.GEAR, StringComparison.OrdinalIgnoreCase))
                return true;
            if (string.Equals(expected, TypeWords.BANKAI, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    private static bool IsNumericType(string typeName)
    {
        return string.Equals(typeName, TypeWords.GEAR, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(typeName, TypeWords.SHIKAI, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(typeName, TypeWords.BANKAI, StringComparison.OrdinalIgnoreCase);
    }
}
