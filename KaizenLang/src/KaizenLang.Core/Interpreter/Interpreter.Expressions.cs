using System.Globalization;

namespace KaizenLang.Core;

public partial class Interpreter
{
    // El parser genera expresiones binarias como [izquierdo, Operator, derecho]. Las llamadas a funciones
    // (incluidas output, input y length) son operandos normales y se evalúan con ExecuteNode; antes había
    // atajos para builtins que devolvían la llamada e ignoraban el resto, p. ej. length(a) - 1 daba length(a).
    private object? ExecuteExpression(Node node)
    {
        if (node.Children.Count == 0)
            return null;

        if (node.Children.Count == 1)
            return ExecuteNode(node.Children[0]);

        if (node.Children.Count % 2 == 0)
            throw new Exception("Expresión mal formada: falta un operando");

        var left = ExecuteNode(node.Children[0]);
        for (int i = 1; i < node.Children.Count; i += 2)
        {
            var operatorNode = node.Children[i];
            if (operatorNode.Type != "Operator" || operatorNode.Children.Count == 0)
                throw new Exception("Expresión mal formada: se esperaba un operador");

            var op = operatorNode.Children[0].Type;
            var rightNode = node.Children[i + 1];

            // && y || solo evalúan el lado derecho cuando hace falta, como en C/Java:
            // así "i < length(a) && a[i] == 0" no indexa fuera de rango.
            if (op == OperatorWords.AND || op == OperatorWords.OR)
            {
                var leftBool = ToBoolean(left, op);
                bool decided = op == OperatorWords.AND ? !leftBool : leftBool;
                left = decided ? leftBool : ToBoolean(ExecuteNode(rightNode), op);
                continue;
            }

            left = EvaluateOperation(left, op, ExecuteNode(rightNode));
        }
        return left;
    }

    private static bool ToBoolean(object? value, string op)
    {
        if (value is bool b)
            return b;
        throw new Exception($"El operador '{op}' requiere valores shin, se recibió {DescribeType(value)}");
    }

    private object EvaluateOperation(object? left, string op, object? right)
    {
        if (op == OperatorWords.EQUAL || op == OperatorWords.NOT_EQUAL)
        {
            if (left == null || right == null)
                return op == OperatorWords.EQUAL ? Equals(left, right) : !Equals(left, right);
        }

        // Con '+' y al menos un texto, se concatena usando el mismo formato que output().
        if (op == OperatorWords.ADD && (left is string || right is string))
            return FormatValue(left) + FormatValue(right);

        if (left is string leftStr && right is string rightStr)
        {
            switch (op)
            {
                case OperatorWords.EQUAL:
                    return leftStr == rightStr;
                case OperatorWords.NOT_EQUAL:
                    return leftStr != rightStr;
                default:
                    throw new Exception($"Operación '{op}' no soportada para textos");
            }
        }

        // gear con gear se calcula en enteros: 7 / 2 == 3 y 7 % 2 == 1.
        if (IsInteger(left) && IsInteger(right))
            return EvaluateIntegerOperation(Convert.ToInt64(left), op, Convert.ToInt64(right));

        if (TryConvertToDouble(left, out double leftNum) && TryConvertToDouble(right, out double rightNum))
        {
            switch (op)
            {
                case OperatorWords.ADD:
                    return leftNum + rightNum;
                case OperatorWords.SUBTRACT:
                    return leftNum - rightNum;
                case OperatorWords.MULTIPLY:
                    return leftNum * rightNum;
                case OperatorWords.DIVIDE:
                    return rightNum != 0 ? leftNum / rightNum : throw new Exception("División por cero");
                case OperatorWords.MODULO:
                    return rightNum != 0 ? leftNum % rightNum : throw new Exception("Módulo por cero");
                case OperatorWords.EQUAL:
                    return leftNum == rightNum;
                case OperatorWords.NOT_EQUAL:
                    return leftNum != rightNum;
                case OperatorWords.LESS:
                    return leftNum < rightNum;
                case OperatorWords.GREATER:
                    return leftNum > rightNum;
                case OperatorWords.LESS_EQUAL:
                    return leftNum <= rightNum;
                case OperatorWords.GREATER_EQUAL:
                    return leftNum >= rightNum;
                default:
                    throw new Exception($"Operación '{op}' no reconocida para números");
            }
        }

        if (left is bool leftBool && right is bool rightBool)
        {
            switch (op)
            {
                case OperatorWords.EQUAL:
                    return leftBool == rightBool;
                case OperatorWords.NOT_EQUAL:
                    return leftBool != rightBool;
                default:
                    throw new Exception($"Operación '{op}' no soportada para booleanos");
            }
        }

        throw new Exception($"Tipos incompatibles para operación '{op}': {DescribeType(left)} y {DescribeType(right)}");
    }

    private static object EvaluateIntegerOperation(long left, string op, long right)
    {
        try
        {
            switch (op)
            {
                case OperatorWords.ADD:
                    return NarrowInteger(checked(left + right));
                case OperatorWords.SUBTRACT:
                    return NarrowInteger(checked(left - right));
                case OperatorWords.MULTIPLY:
                    return NarrowInteger(checked(left * right));
                case OperatorWords.DIVIDE:
                    return right != 0 ? NarrowInteger(left / right) : throw new Exception("División por cero");
                case OperatorWords.MODULO:
                    return right != 0 ? NarrowInteger(left % right) : throw new Exception("Módulo por cero");
                case OperatorWords.EQUAL:
                    return left == right;
                case OperatorWords.NOT_EQUAL:
                    return left != right;
                case OperatorWords.LESS:
                    return left < right;
                case OperatorWords.GREATER:
                    return left > right;
                case OperatorWords.LESS_EQUAL:
                    return left <= right;
                case OperatorWords.GREATER_EQUAL:
                    return left >= right;
                default:
                    throw new Exception($"Operación '{op}' no reconocida para números");
            }
        }
        catch (OverflowException)
        {
            throw new Exception($"Desbordamiento aritmético en la operación '{op}'");
        }
    }

    private static object NarrowInteger(long value)
    {
        return value is >= int.MinValue and <= int.MaxValue ? (int)value : value;
    }

    private static bool IsInteger(object? value) => value is int or long;

    private static bool TryConvertToDouble(object? value, out double result)
    {
        switch (value)
        {
            case int i:
                result = i;
                return true;
            case long l:
                result = l;
                return true;
            case float f:
                result = f;
                return true;
            case double d:
                result = d;
                return true;
            case string s:
                return double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out result);
            default:
                result = 0;
                return false;
        }
    }

    // Formato de los valores al mostrarlos o concatenarlos. Usa siempre punto decimal (InvariantCulture)
    // para que coincida con la sintaxis de los literales, sin importar la configuración regional del equipo,
    // y muestra los booleanos y las colecciones con la notación del lenguaje.
    internal static string FormatValue(object? value)
    {
        switch (value)
        {
            case null:
                return LiteralWords.NULL;
            case string s:
                return s;
            case bool b:
                return b ? LiteralWords.TRUE : LiteralWords.FALSE;
            case System.Collections.IList list:
                var parts = new List<string>(list.Count);
                foreach (var item in list)
                    parts.Add(FormatValue(item));
                return "[" + string.Join(", ", parts) + "]";
            case IFormattable formattable:
                return formattable.ToString(null, CultureInfo.InvariantCulture);
            default:
                return value.ToString() ?? string.Empty;
        }
    }

    private static string DescribeType(object? value)
    {
        return value switch
        {
            null => LiteralWords.NULL,
            int or long => TypeWords.GEAR,
            double or float => TypeWords.BANKAI,
            bool => TypeWords.SHIN,
            string => TypeWords.GRIMOIRE,
            System.Collections.IList => TypeWords.CHAINSAW,
            _ => value.GetType().Name
        };
    }

    private object? ExecuteUnaryExpression(Node node)
    {
        // UnaryExpression tiene 2 hijos: Operator y operando
        if (node.Children.Count < 2)
            throw new Exception("UnaryExpression mal formada");

        var operatorNode = node.Children[0];
        var operandNode = node.Children[1];

        if (operatorNode.Type != "Operator" || operatorNode.Children.Count == 0)
            throw new Exception("Operador unario mal formado");

        var op = operatorNode.Children[0].Type;
        var operand = ExecuteNode(operandNode);

        switch (op)
        {
            case OperatorWords.SUBTRACT:
                if (operand is int intVal)
                    return intVal == int.MinValue ? -(long)intVal : -intVal;
                if (operand is long longVal)
                    return -longVal;
                if (operand is float floatVal)
                    return -floatVal;
                if (operand is double doubleVal)
                    return -doubleVal;
                throw new Exception($"Operador unario '-' no soportado para tipo {DescribeType(operand)}");

            case OperatorWords.NOT:
                if (operand is bool boolVal)
                    return !boolVal;
                throw new Exception($"Operador unario '!' no soportado para tipo {DescribeType(operand)}");

            case OperatorWords.ADD:
                if (operand is int || operand is long || operand is float || operand is double)
                    return operand;
                throw new Exception($"Operador unario '+' no soportado para tipo {DescribeType(operand)}");

            default:
                throw new Exception($"Operador unario '{op}' no reconocido");
        }
    }

    private object? ExecuteLengthBuiltin(Node? argsNode)
    {
        if (argsNode == null || argsNode.Children.Count != 1)
            throw new Exception("Builtin 'length' espera exactamente 1 argumento");

        var value = ExecuteNode(argsNode.Children[0]);
        if (value == null)
            throw new Exception("No se puede calcular length de un valor null");

        if (value is string s)
            return s.Length;

        if (value is System.Collections.ICollection collection)
            return collection.Count;

        throw new Exception($"length no soportado para tipo {DescribeType(value)}");
    }
}
