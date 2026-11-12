using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace KaizenLang.UI.Theming;

/// <summary>
/// Proporciona efectos visuales avanzados para mejorar la apariencia de los controles.
/// Incluye efectos de sombra, gradientes, bordes redondeados y animaciones hover.
/// </summary>
public static class VisualEffects
{
    /// <summary>
    /// Aplica efectos visuales modernos a un botón (gradientes, sombras, bordes redondeados).
    /// </summary>
    public static void ApplyModernButtonStyle(Button button, bool isPrimary = true)
    {
        var theme = ThemeManager.CurrentTheme;

        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.Cursor = Cursors.Hand;

        // Configurar padding interno para mayor elegancia
        button.Padding = new Padding(15, 8, 15, 8);

        // Configurar fuente más moderna
        button.Font = new Font("Segoe UI", 10F, FontStyle.Bold);

        if (isPrimary)
        {
            button.BackColor = theme.ButtonBackground;
            button.ForeColor = theme.ButtonForeground;
            button.FlatAppearance.MouseOverBackColor = theme.ButtonMouseOver;
            button.FlatAppearance.MouseDownBackColor = theme.ButtonMouseDown;
        }
        else
        {
            button.BackColor = theme.SecondaryBackground;
            button.ForeColor = theme.SecondaryForeground;
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(40, theme.ButtonMouseOver);
            button.FlatAppearance.MouseDownBackColor = Color.FromArgb(60, theme.ButtonMouseDown);
        }

        // Eventos para efectos hover suaves
        button.MouseEnter += (s, e) => ButtonHoverEffect((Button)s!, true);
        button.MouseLeave += (s, e) => ButtonHoverEffect((Button)s!, false);

        // Evento para dibujo personalizado con efectos
        button.Paint += (s, e) => DrawModernButton((Button)s!, e.Graphics, isPrimary);
    }

    /// <summary>
    /// Efecto hover suave para botones.
    /// </summary>
    private static void ButtonHoverEffect(Button button, bool isHover)
    {
        var theme = ThemeManager.CurrentTheme;

        if (isHover)
        {
            button.BackColor = theme.ButtonMouseOver;
            // Efecto de elevación sutil
            button.Location = new Point(button.Location.X, button.Location.Y - 1);
        }
        else
        {
            button.BackColor = theme.ButtonBackground;
            // Restaurar posición original
            button.Location = new Point(button.Location.X, button.Location.Y + 1);
        }
    }

    /// <summary>
    /// Dibuja un botón moderno con gradientes y sombras.
    /// </summary>
    private static void DrawModernButton(Button button, Graphics g, bool isPrimary)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var rect = new Rectangle(0, 0, button.Width, button.Height);
        var theme = ThemeManager.CurrentTheme;

        // Crear gradiente sutil
        using (var brush = new LinearGradientBrush(rect,
            Color.FromArgb(20, Color.White),
            Color.FromArgb(10, Color.Black),
            LinearGradientMode.Vertical))
        {
            // Dibujar sombra sutil
            var shadowRect = new Rectangle(rect.X + 2, rect.Y + 2, rect.Width - 2, rect.Height - 2);
            using (var shadowBrush = new SolidBrush(Color.FromArgb(30, Color.Black)))
            {
                g.FillRoundedRectangle(shadowBrush, shadowRect, 6);
            }

            // Dibujar fondo con gradiente
            g.FillRoundedRectangle(brush, rect, 6);
        }

        // Borde sutil
        using (var pen = new Pen(Color.FromArgb(100, theme.ButtonBorder), 1))
        {
            g.DrawRoundedRectangle(pen, rect, 6);
        }
    }

    /// <summary>
    /// Aplica efectos de profundidad a un panel.
    /// </summary>
    public static void ApplyPanelDepthEffect(Panel panel)
    {
        var theme = ThemeManager.CurrentTheme;

        panel.BackColor = theme.PanelBackground;
        panel.Padding = new Padding(10);

        // Evento para dibujo de sombra
        panel.Paint += (s, e) => DrawPanelWithShadow((Panel)s!, e.Graphics);
    }

    /// <summary>
    /// Dibuja un panel con efecto de sombra y borde elegante.
    /// </summary>
    private static void DrawPanelWithShadow(Panel panel, Graphics g)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var rect = panel.ClientRectangle;
        var theme = ThemeManager.CurrentTheme;

        // Sombra interior sutil
        using (var shadowBrush = new SolidBrush(Color.FromArgb(15, Color.Black)))
        {
            var shadowRect = new Rectangle(rect.X + 1, rect.Y + 1, rect.Width - 2, rect.Height - 2);
            g.FillRectangle(shadowBrush, shadowRect);
        }

        // Borde elegante
        using (var pen = new Pen(Color.FromArgb(80, theme.Border), 1))
        {
            g.DrawRectangle(pen, rect.X, rect.Y, rect.Width - 1, rect.Height - 1);
        }
    }

    /// <summary>
    /// Aplica separadores visuales elegantes entre secciones.
    /// </summary>
    public static Panel CreateVisualSeparator(DockStyle dock = DockStyle.Top, int height = 1)
    {
        var theme = ThemeManager.CurrentTheme;

        var separator = new Panel
        {
            Height = height + 4, // Espacio adicional
            Dock = dock,
            BackColor = Color.Transparent
        };

        separator.Paint += (s, e) =>
        {
            var g = e.Graphics;
            var rect = separator.ClientRectangle;

            // Línea principal
            using (var pen = new Pen(Color.FromArgb(100, theme.Border), height))
            {
                var y = rect.Height / 2;
                g.DrawLine(pen, 20, y, rect.Width - 20, y);
            }

            // Línea de brillo sutil
            using (var pen = new Pen(Color.FromArgb(30, Color.White), 1))
            {
                var y = rect.Height / 2 + 1;
                g.DrawLine(pen, 20, y, rect.Width - 20, y);
            }
        };

        return separator;
    }

    /// <summary>
    /// Mejora la apariencia de RichTextBox con bordes elegantes.
    /// </summary>
    public static void ApplyModernTextBoxStyle(RichTextBox textBox)
    {
        var theme = ThemeManager.CurrentTheme;

        textBox.BorderStyle = BorderStyle.None;
        textBox.BackColor = theme.TextBoxBackground;
        textBox.ForeColor = theme.TextBoxForeground;

        // Crear un panel contenedor con borde elegante
        var container = new Panel
        {
            Padding = new Padding(2),
            BackColor = theme.Border
        };

        // Mover el textbox al contenedor
        var parent = textBox.Parent;
        var location = textBox.Location;
        var size = textBox.Size;
        var dock = textBox.Dock;
        var anchor = textBox.Anchor;

        parent?.Controls.Remove(textBox);
        container.Controls.Add(textBox);
        textBox.Dock = DockStyle.Fill;

        container.Location = location;
        container.Size = size;
        container.Dock = dock;
        container.Anchor = anchor;

        parent?.Controls.Add(container);

        // Efecto de focus
        textBox.Enter += (s, e) => container.BackColor = theme.FocusBorder;
        textBox.Leave += (s, e) => container.BackColor = theme.Border;
    }

    /// <summary>
    /// Crear un botón con ícono moderno y efectos visuales.
    /// </summary>
    public static Button CreateModernIconButton(string text, Image? icon, bool isPrimary = true)
    {
        var button = new Button
        {
            Text = $"  {text}",
            Image = icon,
            ImageAlign = ContentAlignment.MiddleLeft,
            TextAlign = ContentAlignment.MiddleRight,
            Height = 40,
            MinimumSize = new Size(120, 40)
        };

        ApplyModernButtonStyle(button, isPrimary);
        return button;
    }
}

/// <summary>
/// Extensiones para dibujo de formas redondeadas.
/// </summary>
public static class GraphicsExtensions
{
    public static void FillRoundedRectangle(this Graphics g, Brush brush, Rectangle rect, int radius)
    {
        using (var path = GetRoundedRectanglePath(rect, radius))
        {
            g.FillPath(brush, path);
        }
    }

    public static void DrawRoundedRectangle(this Graphics g, Pen pen, Rectangle rect, int radius)
    {
        using (var path = GetRoundedRectanglePath(rect, radius))
        {
            g.DrawPath(pen, path);
        }
    }

    private static GraphicsPath GetRoundedRectanglePath(Rectangle rect, int radius)
    {
        var path = new GraphicsPath();
        var diameter = radius * 2;

        // Esquinas redondeadas
        path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
        path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
        path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);

        path.CloseFigure();
        return path;
    }
}
