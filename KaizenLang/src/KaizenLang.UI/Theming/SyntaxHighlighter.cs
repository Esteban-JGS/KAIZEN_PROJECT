using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using KaizenLang.Core;

namespace KaizenLang.UI.Theming;

/// <summary>
/// Proporciona syntax highlighting (resaltado de sintaxis) para el lenguaje KaizenLang.
/// Aplica colores específicos a diferentes elementos del código para mejorar la legibilidad.
/// Utiliza procesamiento asíncrono con debouncing para evitar interferencias con la escritura.
/// </summary>
public static class SyntaxHighlighter
{
    // Cache de expresiones regulares para mejor rendimiento
    private static readonly Regex _commentRegex = new Regex(@"//.*", RegexOptions.Multiline | RegexOptions.Compiled);
    private static readonly Regex _stringRegex = new Regex(@"""(?:\\""|[^""])*""", RegexOptions.Multiline | RegexOptions.Compiled);
    private static readonly Regex _numberRegex = new Regex(@"\b\d+\.?\d*\b", RegexOptions.Multiline | RegexOptions.Compiled);
    private static readonly Regex _genericTypeRegex = new Regex(@"\b(chainsaw|hogyoku)\s*<\s*(\w+)\s*>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex _functionRegex = new Regex(@"\b[a-zA-Z_][a-zA-Z0-9_]*\s*(?=\()", RegexOptions.Multiline | RegexOptions.Compiled);

    // Control de highlighting asíncrono
    private static readonly Dictionary<RichTextBox, CancellationTokenSource> _pendingHighlights = new Dictionary<RichTextBox, CancellationTokenSource>();
    private static readonly Dictionary<RichTextBox, bool> _isHighlighting = new Dictionary<RichTextBox, bool>();
    private static readonly object _lockObject = new object();

    // Lista de operadores como regex compilado
    private static readonly Regex _operatorRegex = new Regex(
        @"(?<operator>\+\+|--|==|!=|<=|>=|&&|\|\||[+\-*/%=<>!(){}\[\];,.:])",
        RegexOptions.Multiline | RegexOptions.Compiled);

    /// <summary>
    /// Aplica syntax highlighting a un RichTextBox basado en los tokens del lenguaje KaizenLang.
    /// </summary>
    public static void ApplySyntaxHighlighting(RichTextBox richTextBox)
    {
        ApplySyntaxHighlighting(richTextBox, false);
    }

    /// <summary>
    /// Aplica syntax highlighting a un RichTextBox de forma asíncrona con debouncing.
    /// </summary>
    /// <param name="richTextBox">El RichTextBox al que aplicar highlighting</param>
    /// <param name="forceHighlight">Si es true, aplica highlighting aunque el control tenga foco</param>
    public static void ApplySyntaxHighlighting(RichTextBox richTextBox, bool forceHighlight)
    {
        if (richTextBox == null || richTextBox.IsDisposed || !richTextBox.Visible)
            return;

        if (string.IsNullOrEmpty(richTextBox.Text))
            return;

        // No aplicar si el usuario está seleccionando texto o copiando
        if (!forceHighlight && richTextBox.SelectionLength > 0)
            return;

        // Cancelar highlighting previo pendiente
        lock (_lockObject)
        {
            if (_pendingHighlights.TryGetValue(richTextBox, out var existingCts))
            {
                existingCts.Cancel();
                existingCts.Dispose();
                _pendingHighlights.Remove(richTextBox);
            }

            // Si ya hay highlighting en progreso para este control, saltar
            if (_isHighlighting.TryGetValue(richTextBox, out var isActive) && isActive)
                return;
        }

        // Crear nuevo token de cancelación
        var cts = new CancellationTokenSource();
        lock (_lockObject)
        {
            _pendingHighlights[richTextBox] = cts;
        }

        // Ejecutar highlighting en segundo plano
        Task.Run(async () =>
        {
            try
            {
                // Pequeño delay para debouncing (solo si no es forzado)
                if (!forceHighlight)
                {
                    await Task.Delay(300, cts.Token);
                }

                if (cts.Token.IsCancellationRequested)
                    return;

                // Marcar como activo
                lock (_lockObject)
                {
                    _isHighlighting[richTextBox] = true;
                }

                // Invocar en el hilo de la UI
                richTextBox.Invoke((Action)(() =>
                {
                    if (richTextBox.IsDisposed || cts.Token.IsCancellationRequested)
                        return;

                    ApplySyntaxHighlightingInternal(richTextBox);
                }));
            }
            catch (OperationCanceledException)
            {
                // Cancelación esperada, no hacer nada
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en syntax highlighting: {ex.Message}");
            }
            finally
            {
                lock (_lockObject)
                {
                    _isHighlighting[richTextBox] = false;
                    _pendingHighlights.Remove(richTextBox);
                }
                cts.Dispose();
            }
        }, cts.Token);
    }

    /// <summary>
    /// Implementación interna del highlighting que se ejecuta en el hilo de la UI.
    /// </summary>
    private static void ApplySyntaxHighlightingInternal(RichTextBox richTextBox)
    {
        if (richTextBox == null || richTextBox.IsDisposed || !richTextBox.Visible)
            return;

        if (string.IsNullOrEmpty(richTextBox.Text))
            return;

        // Guardar posición del cursor y selección
        var selectionStart = richTextBox.SelectionStart;
        var selectionLength = richTextBox.SelectionLength;
        var scrollPosition = GetScrollPosition(richTextBox);

        // No aplicar si hay selección activa (usuario está copiando o seleccionando)
        if (selectionLength > 0)
            return;

        try
        {
            // Deshabilitar redibujado para evitar parpadeo
            NativeMethods.SendMessage(richTextBox.Handle, NativeMethods.WM_SETREDRAW, IntPtr.Zero, IntPtr.Zero);

            // Resetear formato a color base
            richTextBox.SelectAll();
            richTextBox.SelectionColor = ThemeManager.CurrentTheme.Foreground;
            richTextBox.SelectionBackColor = ThemeManager.CurrentTheme.Background;
            richTextBox.SelectionFont = new Font("Consolas", 12F, FontStyle.Regular);

            // Obtener rangos de comentarios una sola vez para reutilizar
            var commentRanges = GetCommentRanges(richTextBox.Text);

            // Aplicar highlighting por categorías en orden de prioridad
            ApplyStringHighlighting(richTextBox, commentRanges);
            ApplyNumberHighlighting(richTextBox, commentRanges);
            ApplyOperatorHighlighting(richTextBox, commentRanges);
            ApplyKeywordHighlighting(richTextBox, commentRanges);
            ApplyTypeHighlighting(richTextBox, commentRanges);
            ApplyFunctionHighlighting(richTextBox, commentRanges);
            // Los comentarios se aplican al final para máxima prioridad
            ApplyCommentHighlighting(richTextBox);

            // Restaurar posición del cursor sin selección
            richTextBox.SelectionStart = selectionStart;
            richTextBox.SelectionLength = 0;
            SetScrollPosition(richTextBox, scrollPosition);
        }
        catch (Exception ex)
        {
            // Log del error
            System.Diagnostics.Debug.WriteLine($"Error en syntax highlighting interno: {ex.Message}");
        }
        finally
        {
            // Reactivar redibujado
            try
            {
                NativeMethods.SendMessage(richTextBox.Handle, NativeMethods.WM_SETREDRAW, new IntPtr(1), IntPtr.Zero);
                richTextBox.Invalidate();
            }
            catch
            {
                // Ignorar errores al restaurar
            }
        }
    }

    #region Highlighting Methods

    /// <summary>
    /// Obtiene los rangos de todos los comentarios en el texto para excluirlos del procesamiento de otros elementos
    /// </summary>
    private static List<(int start, int end)> GetCommentRanges(string text)
    {
        var ranges = new List<(int start, int end)>();

        foreach (Match match in _commentRegex.Matches(text))
        {
            ranges.Add((match.Index, match.Index + match.Length));
        }

        return ranges;
    }

    /// <summary>
    /// Verifica si una posición está dentro de un comentario
    /// </summary>
    private static bool IsInComment(int position, List<(int start, int end)> commentRanges)
    {
        foreach (var (start, end) in commentRanges)
        {
            if (position >= start && position < end)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Verifica si un rango completo está dentro de un comentario
    /// </summary>
    private static bool IsRangeInComment(int start, int length, List<(int start, int end)> commentRanges)
    {
        int end = start + length;
        foreach (var (commentStart, commentEnd) in commentRanges)
        {
            if (start >= commentStart && end <= commentEnd)
                return true;
        }
        return false;
    }

    private static void ApplyCommentHighlighting(RichTextBox richTextBox)
    {
        var theme = ThemeManager.CurrentTheme;

        foreach (Match match in _commentRegex.Matches(richTextBox.Text))
        {
            richTextBox.Select(match.Index, match.Length);
            richTextBox.SelectionColor = theme.Comments;
            richTextBox.SelectionFont = new Font("Consolas", 12F, FontStyle.Italic);
        }
    }

    private static void ApplyStringHighlighting(RichTextBox richTextBox, List<(int start, int end)> commentRanges)
    {
        var theme = ThemeManager.CurrentTheme;

        foreach (Match match in _stringRegex.Matches(richTextBox.Text))
        {
            if (!IsRangeInComment(match.Index, match.Length, commentRanges))
            {
                richTextBox.Select(match.Index, match.Length);
                richTextBox.SelectionColor = theme.Strings;
                richTextBox.SelectionFont = new Font("Consolas", 12F, FontStyle.Regular);
            }
        }
    }

    private static void ApplyNumberHighlighting(RichTextBox richTextBox, List<(int start, int end)> commentRanges)
    {
        var theme = ThemeManager.CurrentTheme;

        foreach (Match match in _numberRegex.Matches(richTextBox.Text))
        {
            if (!IsRangeInComment(match.Index, match.Length, commentRanges))
            {
                richTextBox.Select(match.Index, match.Length);
                richTextBox.SelectionColor = theme.Numbers;
                richTextBox.SelectionFont = new Font("Consolas", 12F, FontStyle.Regular);
            }
        }
    }

    private static void ApplyKeywordHighlighting(RichTextBox richTextBox, List<(int start, int end)> commentRanges)
    {
        var theme = ThemeManager.CurrentTheme;
        var keywords = new[]
        {
            // Palabras clave de control
            ReservedWords.IF, ReservedWords.ELSE, ReservedWords.WHILE, ReservedWords.FOR, ReservedWords.DO, ReservedWords.RETURN,
            // Valores literales
            LiteralWords.TRUE, LiteralWords.FALSE,
            // Funciones builtin
            ReservedWords.INPUT, ReservedWords.OUTPUT, "length",
            // Bloques KaizenLang específicos
            DelimiterWords.BLOCK_START, DelimiterWords.BLOCK_END
        };

        foreach (var keyword in keywords)
        {
            var keywordRegex = new Regex($@"\b{Regex.Escape(keyword)}\b", RegexOptions.IgnoreCase);
            foreach (Match match in keywordRegex.Matches(richTextBox.Text))
            {
                if (!IsRangeInComment(match.Index, match.Length, commentRanges))
                {
                    richTextBox.Select(match.Index, match.Length);
                    richTextBox.SelectionColor = theme.Keywords;
                    richTextBox.SelectionFont = new Font("Consolas", 12F, FontStyle.Bold);
                }
            }
        }
    }

    private static void ApplyTypeHighlighting(RichTextBox richTextBox, List<(int start, int end)> commentRanges)
    {
        var theme = ThemeManager.CurrentTheme;
        var types = new[]
        {
            // Tipos básicos de KaizenLang
            TypeWords.GEAR, TypeWords.GRIMOIRE, TypeWords.SHIN, TypeWords.SHIKAI, TypeWords.BANKAI, "char", ReservedWords.VOID,
            // Tipos compuestos
            TypeWords.CHAINSAW, TypeWords.HOGYOKU
        };

        foreach (var type in types)
        {
            var typeRegex = new Regex($@"\b{Regex.Escape(type)}\b", RegexOptions.IgnoreCase);
            foreach (Match match in typeRegex.Matches(richTextBox.Text))
            {
                if (!IsRangeInComment(match.Index, match.Length, commentRanges))
                {
                    richTextBox.Select(match.Index, match.Length);
                    richTextBox.SelectionColor = theme.Functions;
                    richTextBox.SelectionFont = new Font("Consolas", 12F, FontStyle.Bold);
                }
            }
        }

        // Tipos genéricos
        foreach (Match match in _genericTypeRegex.Matches(richTextBox.Text))
        {
            if (!IsRangeInComment(match.Index, match.Length, commentRanges))
            {
                richTextBox.Select(match.Index, match.Length);
                richTextBox.SelectionColor = theme.Functions;
                richTextBox.SelectionFont = new Font("Consolas", 12F, FontStyle.Bold);
            }
        }
    }

    private static void ApplyOperatorHighlighting(RichTextBox richTextBox, List<(int start, int end)> commentRanges)
    {
        var theme = ThemeManager.CurrentTheme;

        foreach (Match match in _operatorRegex.Matches(richTextBox.Text))
        {
            var operatorGroup = match.Groups["operator"];
            if (operatorGroup.Success && !IsRangeInComment(operatorGroup.Index, operatorGroup.Length, commentRanges))
            {
                richTextBox.Select(operatorGroup.Index, operatorGroup.Length);
                richTextBox.SelectionColor = theme.Operators;
                richTextBox.SelectionFont = new Font("Consolas", 12F, FontStyle.Regular);
            }
        }
    }

    private static void ApplyFunctionHighlighting(RichTextBox richTextBox, List<(int start, int end)> commentRanges)
    {
        var theme = ThemeManager.CurrentTheme;

        foreach (Match match in _functionRegex.Matches(richTextBox.Text))
        {
            var funcName = match.Value.Trim();
            if (!IsRangeInComment(match.Index, funcName.Length, commentRanges))
            {
                // Verificar que no sea una palabra clave
                var keywords = new HashSet<string>
                {
                    ReservedWords.IF,
                    ReservedWords.ELSE,
                    ReservedWords.WHILE,
                    ReservedWords.FOR,
                    ReservedWords.DO,
                    ReservedWords.RETURN,
                    LiteralWords.TRUE,
                    LiteralWords.FALSE,
                    ReservedWords.INPUT,
                    ReservedWords.OUTPUT,
                    "length",
                    DelimiterWords.BLOCK_START,
                    DelimiterWords.BLOCK_END
                };
                if (!keywords.Contains(funcName.ToLower()))
                {
                    richTextBox.Select(match.Index, funcName.Length);
                    richTextBox.SelectionColor = theme.Functions;
                    richTextBox.SelectionFont = new Font("Consolas", 12F, FontStyle.Regular);
                }
            }
        }
    }

    #endregion

    #region Scroll Position Helpers

    private static Point GetScrollPosition(RichTextBox richTextBox)
    {
        try
        {
            return new Point(
                NativeMethods.GetScrollPos(richTextBox.Handle, 0), // SB_HORZ
                NativeMethods.GetScrollPos(richTextBox.Handle, 1)  // SB_VERT
            );
        }
        catch
        {
            return Point.Empty;
        }
    }

    private static void SetScrollPosition(RichTextBox richTextBox, Point position)
    {
        try
        {
            NativeMethods.SetScrollPos(richTextBox.Handle, 0, position.X, true); // SB_HORZ
            NativeMethods.SetScrollPos(richTextBox.Handle, 1, position.Y, true); // SB_VERT
            NativeMethods.SendMessage(richTextBox.Handle, NativeMethods.WM_HSCROLL, new IntPtr(NativeMethods.SB_THUMBPOSITION + 0x10000 * position.X), IntPtr.Zero);
            NativeMethods.SendMessage(richTextBox.Handle, NativeMethods.WM_VSCROLL, new IntPtr(NativeMethods.SB_THUMBPOSITION + 0x10000 * position.Y), IntPtr.Zero);
        }
        catch
        {
            // Ignorar errores de scroll
        }
    }

    #endregion
}

/// <summary>
/// Métodos nativos para manejo del scroll y redibujado
/// </summary>
internal static class NativeMethods
{
    public const int SB_HORZ = 0;
    public const int SB_VERT = 1;
    public const int WM_HSCROLL = 0x0114;
    public const int WM_VSCROLL = 0x0115;
    public const int SB_THUMBPOSITION = 4;
    public const int WM_SETREDRAW = 0x000B;

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    public static extern int GetScrollPos(IntPtr hWnd, int nBar);

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    public static extern int SetScrollPos(IntPtr hWnd, int nBar, int nPos, bool bRedraw);

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    public static extern IntPtr SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);
}
