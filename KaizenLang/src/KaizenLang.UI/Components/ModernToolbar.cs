using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace KaizenLang.UI.Components;

public class ModernToolbar : Panel
{
    public Button? NewButton { get; private set; }
    public Button? OpenButton { get; private set; }
    public Button? SaveButton { get; private set; }
    public Button? CompileButton { get; private set; }
    public Button? ExecuteButton { get; private set; }
    public ComboBox? ThemeSelector { get; private set; }

    public ModernToolbar()
    {
        Height = 60;
        Dock = DockStyle.Top;
        Padding = new Padding(15, 10, 15, 10);

        InitializeControls();
    }

    private void InitializeControls()
    {
        var flowPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            AutoSize = true,
            Padding = new Padding(0)
        };

        // Botones de archivo
        NewButton = CreateToolbarButton("📄 Nuevo", "Ctrl+N");
        OpenButton = CreateToolbarButton("📂 Abrir", "Ctrl+O");
        SaveButton = CreateToolbarButton("💾 Guardar", "Ctrl+S");

        // Separador
        var separator1 = new Panel { Width = 2, Height = 40, Margin = new Padding(10, 0, 10, 0) };

        // Botones de ejecución
        CompileButton = CreateToolbarButton("🔨 Compilar", "F6", true);
        ExecuteButton = CreateToolbarButton("▶️ Ejecutar", "F5", true);

        // Separador
        var separator2 = new Panel { Width = 2, Height = 40, Margin = new Padding(10, 0, 10, 0) };

        // Selector de tema
        var themeLabel = new Label
        {
            Text = "Tema:",
            AutoSize = true,
            Margin = new Padding(10, 12, 5, 0),
            Font = new Font("Segoe UI", 9F)
        };

        ThemeSelector = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 120,
            Margin = new Padding(0, 8, 0, 0),
            Font = new Font("Segoe UI", 9F)
        };
        ThemeSelector.Items.AddRange(new object[] { "Oscuro", "Claro", "Auto" });
        ThemeSelector.SelectedIndex = 0;

        // Agregar controles
        flowPanel.Controls.Add(NewButton);
        flowPanel.Controls.Add(OpenButton);
        flowPanel.Controls.Add(SaveButton);
        flowPanel.Controls.Add(separator1);
        flowPanel.Controls.Add(CompileButton);
        flowPanel.Controls.Add(ExecuteButton);
        flowPanel.Controls.Add(separator2);
        flowPanel.Controls.Add(themeLabel);
        flowPanel.Controls.Add(ThemeSelector);

        Controls.Add(flowPanel);
    }

    private Button CreateToolbarButton(string text, string? tooltip = null, bool emphasized = false)
    {
        var button = new Button
        {
            Text = text,
            AutoSize = false,
            Width = 100,
            Height = 40,
            Margin = new Padding(3),
            Font = new Font("Segoe UI", 9F, emphasized ? FontStyle.Bold : FontStyle.Regular),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            TextAlign = ContentAlignment.MiddleCenter
        };

        button.FlatAppearance.BorderSize = 1;

        if (!string.IsNullOrEmpty(tooltip))
        {
            var toolTip = new ToolTip();
            toolTip.SetToolTip(button, tooltip);
        }

        // Efecto hover
        button.MouseEnter += (s, e) =>
        {
            button.FlatAppearance.BorderSize = 2;
        };

        button.MouseLeave += (s, e) =>
        {
            button.FlatAppearance.BorderSize = 1;
        };

        return button;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        // Dibujar línea inferior sutil
        using (var pen = new Pen(ControlPaint.Dark(BackColor, 0.1f)))
        {
            e.Graphics.DrawLine(pen, 0, Height - 1, Width, Height - 1);
        }
    }
}
