using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace KaizenLang.UI.Theming;

/// <summary>
/// Renderer personalizado para crear menús con efectos visuales modernos.
/// </summary>
public class ModernMenuRenderer : ToolStripProfessionalRenderer
{
    private readonly Theme theme;

    public ModernMenuRenderer(Theme theme) : base(new ModernMenuColorTable(theme))
    {
        this.theme = theme;
    }

    protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var rect = new Rectangle(1, 1, e.Item.Width - 2, e.Item.Height - 2);

        if (e.Item.Selected)
        {
            // Efecto hover elegante
            using (var brush = new LinearGradientBrush(rect,
                EnhancedVisualEffects.LightenColor(theme.ButtonBackground, 20),
                theme.ButtonBackground,
                LinearGradientMode.Vertical))
            {
                g.FillRoundedRect(brush, rect, 4);
            }

            // Borde sutil
            using (var pen = new Pen(EnhancedVisualEffects.LightenColor(theme.ButtonBackground, 30), 1))
            {
                g.DrawRoundedRect(pen, rect, 4);
            }
        }
        else if (e.Item.Pressed)
        {
            // Efecto pressed
            using (var brush = new LinearGradientBrush(rect,
                theme.ButtonBackground,
                EnhancedVisualEffects.DarkenColor(theme.ButtonBackground, 10),
                LinearGradientMode.Vertical))
            {
                g.FillRoundedRect(brush, rect, 4);
            }
        }
    }

    protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var rect = e.AffectedBounds;

        // Fondo con gradiente más elegante
        using (var brush = new LinearGradientBrush(rect,
            EnhancedVisualEffects.LightenColor(theme.MenuBackground, 2),
            EnhancedVisualEffects.DarkenColor(theme.MenuBackground, 5),
            LinearGradientMode.Vertical))
        {
            g.FillRectangle(brush, rect);
        }

        // Línea superior de highlight
        using (var pen = new Pen(Color.FromArgb(40, Color.White), 1))
        {
            g.DrawLine(pen, rect.Left, rect.Top, rect.Right, rect.Top);
        }

        // Línea inferior elegante con sombra
        using (var pen = new Pen(Color.FromArgb(120, theme.Border), 1))
        {
            g.DrawLine(pen, rect.Left, rect.Bottom - 1, rect.Right, rect.Bottom - 1);
        }

        // Sombra sutil debajo del menú
        using (var shadowBrush = new SolidBrush(Color.FromArgb(20, Color.Black)))
        {
            g.FillRectangle(shadowBrush, rect.Left, rect.Bottom - 1, rect.Width, 2);
        }
    }
    protected override void OnRenderImageMargin(ToolStripRenderEventArgs e)
    {
        // Margen de iconos con efecto sutil
        var g = e.Graphics;
        var rect = e.AffectedBounds;

        using (var brush = new SolidBrush(Color.FromArgb(30, theme.SecondaryBackground)))
        {
            g.FillRectangle(brush, rect);
        }
    }
}

/// <summary>
/// Tabla de colores personalizada para el renderer del menú.
/// </summary>
public class ModernMenuColorTable : ProfessionalColorTable
{
    private readonly Theme theme;

    public ModernMenuColorTable(Theme theme)
    {
        this.theme = theme;
    }

    public override Color MenuItemSelected => Color.Transparent;
    public override Color MenuItemBorder => Color.Transparent;
    public override Color MenuBorder => theme.Border;
    public override Color ToolStripDropDownBackground => theme.MenuBackground;
    public override Color ImageMarginGradientBegin => Color.Transparent;
    public override Color ImageMarginGradientMiddle => Color.Transparent;
    public override Color ImageMarginGradientEnd => Color.Transparent;
    public override Color MenuItemSelectedGradientBegin => Color.Transparent;
    public override Color MenuItemSelectedGradientEnd => Color.Transparent;
    public override Color MenuItemPressedGradientBegin => Color.Transparent;
    public override Color MenuItemPressedGradientEnd => Color.Transparent;
}

/// <summary>
/// Extensiones para dibujar formas redondeadas en Graphics para menús.
/// </summary>
public static class MenuGraphicsExtensions
{
    public static void FillRoundedRect(this Graphics g, Brush brush, Rectangle rect, int radius)
    {
        using (var path = GetRoundedRectPath(rect, radius))
        {
            g.FillPath(brush, path);
        }
    }

    public static void DrawRoundedRect(this Graphics g, Pen pen, Rectangle rect, int radius)
    {
        using (var path = GetRoundedRectPath(rect, radius))
        {
            g.DrawPath(pen, path);
        }
    }

    private static GraphicsPath GetRoundedRectPath(Rectangle rect, int radius)
    {
        var path = new GraphicsPath();
        var diameter = radius * 2;

        if (radius <= 0)
        {
            path.AddRectangle(rect);
            return path;
        }

        // Crear path con esquinas redondeadas
        path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
        path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
        path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);

        path.CloseFigure();
        return path;
    }
}
