namespace KaizenLang.Core;

public class Node
{
    public string Type { get; set; }
    public List<Node> Children { get; set; }
    public object? Value { get; set; }
    public int Line { get; set; }
    public int Column { get; set; }

    public Node()
    {
        Type = string.Empty;
        Children = new List<Node>();
        Line = 0;
        Column = 0;
    }

    public Node(string type) : this()
    {
        Type = type;
    }
    public Node(string type, object? value) : this(type)
    {
        Value = value;
    }

    public Node(string type, List<Node> children) : this(type)
    {
        if (children != null)
            this.Children = children;
    }

    public static Node CreateFunctionCall(string functionName, List<Node>? arguments = null, int line = 0, int column = 0)
    {
        var fn = new Node("FunctionName", new List<Node> { new Node("Identifier", new List<Node> { new Node(functionName) }) });
        fn.Line = line;
        fn.Column = column;

        var args = new Node("Arguments", arguments ?? new List<Node>());

        var call = new Node("FunctionCall", new List<Node> { fn, args });
        call.Line = line;
        call.Column = column;
        return call;
    }

    // Método para agregar un nodo hijo
    public void AddChild(Node child)
    {
        Children.Add(child);
    }

    // Método para buscar un hijo por tipo
    public Node? FindChild(string type)
    {
        return Children.FirstOrDefault(c => c.Type == type);
    }

    // Método para obtener todos los hijos de un tipo específico
    public List<Node> FindChildren(string type)
    {
        return Children.Where(c => c.Type == type).ToList();
    }

    // Método para imprimir el árbol de manera legible
    public string ToTreeString(int indent = 0)
    {
        string result = new string(' ', indent * 2) + Type;
        if (Value != null)
            result += $": {Value}";
        result += "\n";

        foreach (var child in Children)
            result += child.ToTreeString(indent + 1);

        return result;
    }

    // Tipo de nodo con el que el parser marca un error de sintaxis; su primer hijo guarda el mensaje.
    public const string ErrorType = "Error";

    // Se compara el tipo exacto: los literales e identificadores también guardan su texto en Type,
    // así que buscar "Error" como subcadena marcaba como error un output("Error") o una variable numErrores.
    public bool HasErrors()
    {
        return DescendantsAndSelf().Any(n => n.Type == ErrorType);
    }

    // Mensajes de los nodos de error, en orden de aparición. No entra a los hijos de un nodo de error,
    // que solo guardan su mensaje.
    public List<string> GetAllErrors()
    {
        var collected = new List<string>();
        foreach (var node in DescendantsAndSelf(stopAt: n => n.Type == ErrorType))
        {
            if (node.Type == ErrorType)
            {
                collected.Add(node.Children.Count > 0 && !string.IsNullOrEmpty(node.Children[0].Type)
                    ? node.Children[0].Type
                    : "Error de sintaxis");
            }
        }
        return collected;
    }

    // Recorrido en preorden con una pila explícita, sin recursión: una expresión larga como 1+1+1+...
    // genera un árbol muy profundo y recorrerlo recursivamente podía agotar la pila de WebAssembly.
    // 'stopAt' devuelve el nodo pero no recorre sus hijos.
    public IEnumerable<Node> DescendantsAndSelf(Func<Node, bool>? stopAt = null)
    {
        var pending = new Stack<Node>();
        pending.Push(this);
        while (pending.Count > 0)
        {
            var node = pending.Pop();
            yield return node;
            if (stopAt != null && stopAt(node))
                continue;
            for (int i = node.Children.Count - 1; i >= 0; i--)
                pending.Push(node.Children[i]);
        }
    }

    // Profundidad máxima del árbol (1 = solo este nodo), calculada sin recursión.
    public int Depth()
    {
        int max = 0;
        var pending = new Stack<(Node Node, int Level)>();
        pending.Push((this, 1));
        while (pending.Count > 0)
        {
            var (node, level) = pending.Pop();
            if (level > max)
                max = level;
            foreach (var child in node.Children)
                pending.Push((child, level + 1));
        }
        return max;
    }
}
