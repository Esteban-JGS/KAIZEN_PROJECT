using System;
using System.Drawing;
using System.Windows.Forms;
using KaizenLang.UI.Theming;

namespace KaizenLang.UI.Components;

public class SidebarPanel : Panel
{
    private Label titleLabel = null!;
    private RichTextBox infoBox = null!;
    private Panel headerPanel = null!;

    public SidebarPanel()
    {
        Dock = DockStyle.Fill;
        Padding = new Padding(10);
        Width = 280;

        InitializeControls();
    }

    private void InitializeControls()
    {
        // Header
        headerPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 45,
            Padding = new Padding(0, 0, 0, 8)
        };

        titleLabel = new Label
        {
            Text = "Guía Rápida",
            Dock = DockStyle.Top,
            Font = new Font("Segoe UI Semibold", 10.5F, FontStyle.Bold),
            Height = 32,
            TextAlign = ContentAlignment.MiddleCenter,
            Padding = new Padding(0, 6, 0, 0)
        };

        headerPanel.Controls.Add(titleLabel);

        // Info box - usar la misma fuente que el editor de código
        infoBox = new RichTextBox
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            BorderStyle = BorderStyle.None,
            Font = new Font("Cascadia Code", 9F, FontStyle.Regular),
            Padding = new Padding(10),
            WordWrap = false,
            ScrollBars = RichTextBoxScrollBars.Vertical
        };

        // Intentar fuentes alternativas si Cascadia Code no está disponible
        if (infoBox.Font.Name != "Cascadia Code")
        {
            infoBox.Font = new Font("Consolas", 9F, FontStyle.Regular);
        }

        LoadQuickGuide();

        Controls.Add(infoBox);
        Controls.Add(headerPanel);
    }

    private void LoadQuickGuide()
    {
        // Limpiar el contenido
        infoBox.Clear();

        var theme = ThemeManager.CurrentTheme;

        // Colores de sintaxis
        var keywordColor = Color.FromArgb(86, 156, 214);  // Azul para keywords
        var typeColor = Color.FromArgb(78, 201, 176);      // Verde agua para tipos
        var stringColor = Color.FromArgb(206, 145, 120);   // Naranja para strings
        var commentColor = Color.FromArgb(87, 166, 74);    // Verde para comentarios
        var normalColor = theme.TextBoxForeground;
        var headerColor = Color.FromArgb(220, 220, 170);   // Amarillo claro para headers

        // Helper para agregar texto con color
        void AddText(string text, Color color, bool bold = false)
        {
            infoBox.SelectionColor = color;
            infoBox.SelectionFont = bold
                ? new Font(infoBox.Font, FontStyle.Bold)
                : infoBox.Font;
            infoBox.AppendText(text);
        }

        // Título
        AddText("═══════════════════════════════\n", headerColor);
        AddText("   KAIZENLANG - GUÍA RÁPIDA\n", headerColor, true);
        AddText("═══════════════════════════════\n\n", headerColor);

        // VARIABLES
        AddText("▸ VARIABLES\n\n", headerColor, true);
        AddText("  ", normalColor);
        AddText("gear", typeColor, true);
        AddText(" numero = ", normalColor);
        AddText("10", Color.FromArgb(181, 206, 168));
        AddText(";\n", normalColor);

        AddText("  ", normalColor);
        AddText("grimoire", typeColor, true);
        AddText(" texto = ", normalColor);
        AddText("\"Hola\"", stringColor);
        AddText(";\n", normalColor);

        AddText("  ", normalColor);
        AddText("shin", typeColor, true);
        AddText(" estado = ", normalColor);
        AddText("true", keywordColor);
        AddText(";\n\n", normalColor);

        // CONDICIONALES
        AddText("▸ CONDICIONALES\n\n", headerColor, true);
        AddText("  ", normalColor);
        AddText("if", keywordColor, true);
        AddText(" (numero > ", normalColor);
        AddText("5", Color.FromArgb(181, 206, 168));
        AddText(") ", normalColor);
        AddText("ying", keywordColor, true);
        AddText("\n    ", normalColor);
        AddText("output", Color.FromArgb(220, 220, 170));
        AddText("(", normalColor);
        AddText("\"Mayor\"", stringColor);
        AddText(");\n  ", normalColor);
        AddText("yang", keywordColor, true);
        AddText("\n\n", normalColor);

        // BUCLES
        AddText("▸ BUCLES\n\n", headerColor, true);

        // While
        AddText("  ", normalColor);
        AddText("while", keywordColor, true);
        AddText(" (numero < ", normalColor);
        AddText("10", Color.FromArgb(181, 206, 168));
        AddText(") ", normalColor);
        AddText("ying", keywordColor, true);
        AddText("\n    numero = numero + ", normalColor);
        AddText("1", Color.FromArgb(181, 206, 168));
        AddText(";\n  ", normalColor);
        AddText("yang", keywordColor, true);
        AddText("\n\n", normalColor);

        // For
        AddText("  ", normalColor);
        AddText("for", keywordColor, true);
        AddText(" (", normalColor);
        AddText("gear", typeColor, true);
        AddText(" i=", normalColor);
        AddText("0", Color.FromArgb(181, 206, 168));
        AddText("; i<", normalColor);
        AddText("5", Color.FromArgb(181, 206, 168));
        AddText("; i++) ", normalColor);
        AddText("ying", keywordColor, true);
        AddText("\n    ", normalColor);
        AddText("output", Color.FromArgb(220, 220, 170));
        AddText("(i);\n  ", normalColor);
        AddText("yang", keywordColor, true);
        AddText("\n\n", normalColor);

        // FUNCIONES
        AddText("▸ FUNCIONES\n\n", headerColor, true);
        AddText("  ", normalColor);
        AddText("gear", typeColor, true);
        AddText(" suma(", normalColor);
        AddText("gear", typeColor, true);
        AddText(" a, ", normalColor);
        AddText("gear", typeColor, true);
        AddText(" b) ", normalColor);
        AddText("ying", keywordColor, true);
        AddText("\n    ", normalColor);
        AddText("return", keywordColor, true);
        AddText(" a + b;\n  ", normalColor);
        AddText("yang", keywordColor, true);
        AddText("\n\n", normalColor);

        // ARRAYS
        AddText("▸ ARRAYS\n\n", headerColor, true);
        AddText("  ", normalColor);
        AddText("chainsaw", typeColor, true);
        AddText("<", normalColor);
        AddText("gear", typeColor, true);
        AddText("> lista = [", normalColor);
        AddText("1", Color.FromArgb(181, 206, 168));
        AddText(", ", normalColor);
        AddText("2", Color.FromArgb(181, 206, 168));
        AddText(", ", normalColor);
        AddText("3", Color.FromArgb(181, 206, 168));
        AddText("];\n\n", normalColor);

        // ATAJOS
        AddText("═══════════════════════════════\n", headerColor);
        AddText("⌨  ATAJOS DE TECLADO\n", headerColor, true);
        AddText("═══════════════════════════════\n\n", headerColor);

        AddText("  F5        ", Color.FromArgb(156, 220, 254), true);
        AddText("→ Ejecutar código\n", normalColor);
        AddText("  F6        ", Color.FromArgb(156, 220, 254), true);
        AddText("→ Compilar código\n", normalColor);
        AddText("  Ctrl+S    ", Color.FromArgb(156, 220, 254), true);
        AddText("→ Guardar archivo\n", normalColor);
        AddText("  Ctrl+O    ", Color.FromArgb(156, 220, 254), true);
        AddText("→ Abrir archivo\n", normalColor);
        AddText("  Ctrl+N    ", Color.FromArgb(156, 220, 254), true);
        AddText("→ Nuevo archivo\n\n", normalColor);

        // Footer
        AddText("═══════════════════════════════\n", headerColor);

        // Resetear al inicio
        infoBox.SelectionStart = 0;
        infoBox.SelectionLength = 0;
        infoBox.SelectionColor = normalColor;
    }
    public void UpdateTheme(Theme theme)
    {
        BackColor = theme.PanelBackground;
        titleLabel.ForeColor = theme.Foreground;

        // Usar los mismos colores que el editor de código
        infoBox.BackColor = theme.TextBoxBackground;
        infoBox.ForeColor = theme.TextBoxForeground;
        headerPanel.BackColor = theme.PanelBackground;

        // Recargar la guía con los nuevos colores
        LoadQuickGuide();
    }
}
