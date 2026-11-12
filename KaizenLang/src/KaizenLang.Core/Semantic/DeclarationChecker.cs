using System;
using System.Collections.Generic;
using System.Linq;

namespace KaizenLang.Core;

public class DeclarationChecker
{
    private readonly Stack<SymbolTable> scopes;
    private readonly IDictionary<string, FunctionSignature> functions;
    private readonly Dictionary<string, int> builtins;
    private readonly TypeResolver typeResolver;
    private readonly Diagnostics diagnostics;
    private readonly Action<Node> visitNodeCallback;

    public DeclarationChecker(Stack<SymbolTable> scopes,
                              IDictionary<string, FunctionSignature> functions,
                              Dictionary<string, int> builtins,
                              TypeResolver typeResolver,
                              Diagnostics diagnostics,
                              Action<Node> visitNodeCallback)
    {
        this.scopes = scopes;
        this.functions = functions;
        this.builtins = builtins;
        this.typeResolver = typeResolver;
        this.diagnostics = diagnostics;
        this.visitNodeCallback = visitNodeCallback;
    }

    public void RegisterVariable(Node node)
    {
        var nameNode = node.FindChild("Identifier") ?? node.FindChild("IDENTIFIER");
        if (nameNode == null)
        {
            diagnostics.Report(node, "Declaración de variable sin nombre");
            return;
        }

        var varName = SemanticUtils.ExtractIdentifierName(nameNode);
        if (string.IsNullOrEmpty(varName))
        {
            diagnostics.Report(node, "Declaración de variable sin nombre");
            return;
        }

        var current = scopes.Peek();
        if (!current.DeclareVariable(varName, string.Empty, nameNode.Line))
        {
            diagnostics.Report(node, $"Variable '{varName}' ya declarada en este alcance");
            return;
        }

        if (node.Children == null || node.Children.Count == 0)
        {
            diagnostics.Report(node, "Declaración de variable sin tipo");
            return;
        }

        var typeNode = node.Children[0];
        if (typeNode == null)
        {
            diagnostics.Report(node, "Declaración de variable con tipo mal formado");
            return;
        }

        var wrapperType = typeNode.Type ?? string.Empty;
        var declaredType = string.Empty;
        var declaredElem = string.Empty;

        if (typeNode.Children != null && typeNode.Children.Count > 0)
            declaredElem = typeNode.Children[0].Type ?? string.Empty;

        if (!string.IsNullOrEmpty(wrapperType)
            && !wrapperType.StartsWith(TypeWords.CHAINSAW, StringComparison.OrdinalIgnoreCase)
            && !wrapperType.StartsWith(TypeWords.HOGYOKU, StringComparison.OrdinalIgnoreCase))
        {
            declaredType = wrapperType;
        }

        bool initializerHasErrors = false;

        if ((wrapperType.StartsWith(TypeWords.CHAINSAW, StringComparison.OrdinalIgnoreCase) || wrapperType.StartsWith(TypeWords.HOGYOKU, StringComparison.OrdinalIgnoreCase))
                && string.IsNullOrEmpty(declaredElem))
        {
            diagnostics.Report(node, "Declaración de chainsaw/hogyoku requiere tipo de elemento explícito");
            return;
        }

        if (node.Children.Count > 2)
        {
            var initWrapper = node.Children[2];
            var initExpr = initWrapper.Children.Count > 0 ? initWrapper.Children[0] : initWrapper;

            if (initExpr != null && initExpr.Children != null && initExpr.Children.Count >= 2)
            {
                for (int i = 0; i < initExpr.Children.Count - 1; i++)
                {
                    var a = initExpr.Children[i];
                    var b = initExpr.Children[i + 1];
                    if ((a.Type == "INT" || a.Type == "FLOAT") && (b.Type == "IDENTIFIER" || b.Type == "Identifier"))
                    {
                        Node locationNode = a ?? initExpr;
                        if (a != null && a.Children != null && a.Children.Count > 0)
                        {
                            var firstChild = a.Children[0];
                            if (firstChild != null && firstChild.Line > 0)
                                locationNode = firstChild;
                        }

                        diagnostics.Report(locationNode, $"Inicializador inválido: literal seguido de identificador sin operador (posible token pegado)");
                        initializerHasErrors = true;
                        break;
                    }
                }
            }

            if (!initializerHasErrors)
            {
                ValidateIdentifiersInExpression(initExpr);
                // Recorre el inicializador con el visitante general para validar también la aridad de
                // las llamadas: antes "gear x = suma(10);" pasaba sin error si suma esperaba 2 argumentos.
                if (initExpr != null)
                    visitNodeCallback(initExpr);
            }

            var initType = initializerHasErrors ? null : typeResolver.Resolve(initExpr);
            if (initType != null)
            {
                if (wrapperType.StartsWith(TypeWords.CHAINSAW, StringComparison.OrdinalIgnoreCase) || wrapperType.StartsWith(TypeWords.HOGYOKU, StringComparison.OrdinalIgnoreCase))
                {
                    if (!string.IsNullOrEmpty(declaredElem))
                    {
                        if (wrapperType.StartsWith(TypeWords.HOGYOKU, StringComparison.OrdinalIgnoreCase))
                        {
                            var expected = $"{TypeWords.CHAINSAW}<{TypeWords.CHAINSAW}<{declaredElem}>>";
                            if (string.Equals(initType, expected, StringComparison.OrdinalIgnoreCase))
                            {
                            }
                            else if (initType.StartsWith($"{TypeWords.CHAINSAW}<", StringComparison.OrdinalIgnoreCase))
                            {
                                var inner = TypeResolver.ExtractInnerType(initType);
                                if (string.IsNullOrEmpty(inner) || !inner.StartsWith($"{TypeWords.CHAINSAW}<", StringComparison.OrdinalIgnoreCase))
                                {
                                    diagnostics.Report(node, $"Tipo incompatible en inicialización: se esperaba 'hogyoku<{declaredElem}>' pero se encontró '{initType}'");
                                }
                                else
                                {
                                    var innerElem = TypeResolver.ExtractInnerType(inner);
                                    if (!string.Equals(innerElem, declaredElem, StringComparison.OrdinalIgnoreCase) &&
                                        !(SemanticUtils.IsNullLiteralType(innerElem) && SemanticUtils.SupportsNullAssignment(declaredElem)))
                                        diagnostics.Report(node, $"Tipo incompatible en inicialización: se esperaba 'hogyoku<{declaredElem}>' pero se encontró '{initType}'");
                                }
                            }
                        }
                        else if (initType.StartsWith($"{TypeWords.CHAINSAW}<") && !string.IsNullOrEmpty(declaredElem))
                        {
                            var initElem = TypeResolver.ExtractInnerType(initType);
                            if (!string.IsNullOrEmpty(initElem) &&
                                !string.Equals(initElem, declaredElem, StringComparison.OrdinalIgnoreCase) &&
                                !(SemanticUtils.IsNullLiteralType(initElem) && SemanticUtils.SupportsNullAssignment(declaredElem)))
                                diagnostics.Report(node, $"Tipo incompatible en inicialización: se esperaba '{declaredElem}' pero se encontró '{initElem}'");
                        }
                    }

                }
                else
                {
                    if (!string.IsNullOrEmpty(declaredType) && !CanAssign(declaredType, initType))
                    {
                        diagnostics.Report(node, $"Tipo incompatible en inicialización: se esperaba '{declaredType}' pero se encontró '{initType}'");
                    }
                }
            }
        }

        if (!initializerHasErrors)
        {
            string effectiveType = declaredType;
            if (wrapperType.StartsWith(TypeWords.CHAINSAW, StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrEmpty(declaredElem))
                    effectiveType = $"{TypeWords.CHAINSAW}<{declaredElem}>";
                else
                    effectiveType = TypeWords.CHAINSAW;
            }
            else if (wrapperType.StartsWith(TypeWords.HOGYOKU, StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrEmpty(declaredElem))
                    effectiveType = $"{TypeWords.CHAINSAW}<{TypeWords.CHAINSAW}<{declaredElem}>>";
                else
                    effectiveType = $"{TypeWords.CHAINSAW}<{TypeWords.CHAINSAW}>";
            }

            var sym = current.LookupVariable(varName);
            if (sym != null)
            {
                sym.Type = effectiveType;
                if (node.Children.Count > 2)
                    current.SetVariableValue(varName, new object());
            }
        }

    }

    public void RegisterFunction(Node node)
    {
        var nameNode = node.FindChild("FunctionName") ?? node.FindChild("Identifier") ?? node.FindChild("IDENTIFIER");
        var fnName = SemanticUtils.ExtractIdentifierName(nameNode);
        if (string.IsNullOrEmpty(fnName))
        {
            diagnostics.Report(node, "Declaración de función sin nombre");
            return;
        }

        int arity = 0;
        var paramsNode = node.FindChild("Parameters") ?? node.FindChild("Arguments") ?? node.FindChild("PARAMETERS");
        if (paramsNode != null)
            arity = paramsNode.Children.Count;

        if (functions.ContainsKey(fnName))
        {
            diagnostics.Report(node, $"Función '{fnName}' ya declarada");
            return;
        }

        // El tipo de retorno se registra antes de recorrer el cuerpo para que las llamadas recursivas tengan tipo.
        var returnType = node.Children.Count > 0 ? node.Children[0].Type : string.Empty;
        functions[fnName] = new FunctionSignature { Name = fnName, Arity = arity, IsBuiltin = false, ReturnType = returnType };

        scopes.Push(new SymbolTable());
        if (paramsNode != null)
        {
            foreach (var p in paramsNode.Children)
            {
                var idNode = p.FindChild("Identifier") ?? p.FindChild("IDENTIFIER");
                var paramName = SemanticUtils.ExtractIdentifierName(idNode) ?? p.Value?.ToString();
                if (string.IsNullOrEmpty(paramName))
                    continue;
                var cur = scopes.Peek();
                if (cur.LookupVariable(paramName) != null)
                {
                    diagnostics.Report(p, $"Parámetro '{paramName}' duplicado");
                }
                else
                {
                    var typeNode = p.Children.Count > 0 ? p.Children[0] : null;
                    cur.DeclareVariable(paramName, ParamTypeName(typeNode), p.Line);
                }
            }
        }

        var body = node.FindChild("Body") ?? node.FindChild("Block");
        if (body != null)
        {
            var previous = (currentFunctionName, currentReturnType);
            (currentFunctionName, currentReturnType) = (fnName, returnType);
            try
            {
                visitNodeCallback(body);
            }
            finally
            {
                (currentFunctionName, currentReturnType) = previous;
            }

            var returns = new List<Node>();
            SemanticUtils.CollectNodes(body, "Return", returns);
            if (!IsVoid(returnType) && returns.Count == 0)
                diagnostics.Report(node, $"La función '{fnName}' debe devolver un valor de tipo '{returnType}'");
        }

        if (scopes.Count > 0)
            scopes.Pop();
    }

    private string? currentFunctionName;
    private string currentReturnType = string.Empty;

    public bool InsideFunction => currentFunctionName != null;

    private static bool IsVoid(string type) => string.Equals(type, ReservedWords.VOID, StringComparison.OrdinalIgnoreCase);

    // Valida un 'return' contra el tipo de la función que se está recorriendo. Se llama durante el
    // recorrido del cuerpo, cuando las variables locales del bloque todavía están en el alcance.
    public void CheckReturn(Node ret)
    {
        if (currentFunctionName == null)
            return;

        var value = ret.Children.Count > 0 ? ret.Children[0] : null;
        if (IsVoid(currentReturnType))
        {
            if (value != null)
                diagnostics.Report(ret, $"La función void '{currentFunctionName}' no puede devolver un valor");
            return;
        }

        if (value == null)
        {
            diagnostics.Report(ret, $"La función '{currentFunctionName}' debe devolver un valor de tipo '{currentReturnType}'");
            return;
        }

        var valueType = typeResolver.Resolve(value);
        if (!string.IsNullOrEmpty(valueType) && !CanAssign(currentReturnType, valueType))
            diagnostics.Report(ret, $"Tipo de retorno incompatible en '{currentFunctionName}': se esperaba '{currentReturnType}' pero se encontró '{valueType}'");
    }


    private bool IsSymbolDefined(string name) => SemanticUtils.IsSymbolDefined(scopes, name);

    private void ValidateIdentifiersInExpression(Node? expr)
    {
        if (expr == null)
            return;

        if (expr.Type == "IDENTIFIER" || expr.Type == "Identifier")
        {
            var name = SemanticUtils.ExtractIdentifierName(expr);
            if (!string.IsNullOrEmpty(name) && !IsSymbolDefined(name) && !functions.ContainsKey(name) && !builtins.ContainsKey(name))
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

    private static bool CanAssign(string targetType, string sourceType) => SemanticUtils.CanAssign(targetType, sourceType);

    // Tipo de un parámetro con la misma notación que usan las variables:
    // gear, chainsaw<gear>, y hogyoku<T> como chainsaw<chainsaw<T>>.
    private static string ParamTypeName(Node? typeNode)
    {
        if (typeNode == null || typeNode.Type == "Identifier")
            return string.Empty;
        if (typeNode.Children.Count == 0)
            return typeNode.Type;

        var element = typeNode.Children[0].Type;
        if (string.Equals(typeNode.Type, TypeWords.HOGYOKU, StringComparison.OrdinalIgnoreCase))
            return $"{TypeWords.CHAINSAW}<{TypeWords.CHAINSAW}<{element}>>";
        return $"{typeNode.Type}<{element}>";
    }
}
