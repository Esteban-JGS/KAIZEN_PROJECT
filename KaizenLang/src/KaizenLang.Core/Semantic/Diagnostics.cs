using System;
using System.Collections.Generic;
using System.Linq;

namespace KaizenLang.Core;

// Lightweight diagnostics collector used by the semantic analysis modules.
// Keeps messages in insertion order and provides helpers to report with node positions.
public class Diagnostics
{
    private readonly List<string> _errors = new();

    public void Clear() => _errors.Clear();

    // Report a message associated with a node; if node is null, message is stored as-is.
    public void Report(Node? node, string message)
    {
        if (node == null)
        {
            _errors.Add(message);
            return;
        }
        _errors.Add(Format(node, message));
    }

    // Report a pre-formatted message (already contains location or context)
    public void ReportMessage(string formattedMessage)
    {
        _errors.Add(formattedMessage);
    }

    public IReadOnlyList<string> GetAll() => _errors;

    public List<string> GetUnique()
    {
        return _errors.Distinct().ToList();
    }

    private static string Format(Node node, string message)
    {
        if (node == null)
            return message;
        return $"{message} (l{node.Line}:c{node.Column})";
    }
}
