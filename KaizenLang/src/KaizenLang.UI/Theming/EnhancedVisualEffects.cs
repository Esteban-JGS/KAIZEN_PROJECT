using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace KaizenLang.UI.Theming;

/// <summary>
/// Efectos visuales avanzados con owner-draw para dar verdadera profundidad a la interfaz.
/// </summary>
public static class EnhancedVisualEffects
{
    /// <summary>
    /// Convierte un botón en un botón moderno con efectos 3D reales y esquinas verdaderamente redondeadas.
    /// </summary>
    public static void MakeButtonModern(Button button, Color baseColor, bool isPrimary = true)
    {
        // Configuración básica
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.UseVisualStyleBackColor = false;
        button.BackColor = Color.Transparent; // Importante para evitar fondo cuadrado
        button.Cursor = Cursors.Hand;
        button.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        button.ForeColor = Color.White;

        // Configurar padding para mejor apariencia
        button.Padding = new Padding(20, 10, 20, 10);

        // Crear región redondeada para el botón
        CreateRoundedButtonRegion(button);

        // Variables para controlar el estado
        bool isPressed = false;
        bool isHovered = false;

        // Evento Paint personalizado
        button.Paint += (s, e) =>
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            var rect = button.ClientRectangle;

            // Crear colores dinámicos basados en el estado
            Color topColor, bottomColor, borderColor;

            if (isPressed)
            {
                topColor = DarkenColor(baseColor, 20);
                bottomColor = DarkenColor(baseColor, 10);
                borderColor = DarkenColor(baseColor, 30);
                rect = new Rectangle(rect.X + 1, rect.Y + 1, rect.Width - 2, rect.Height - 2);
            }
            else if (isHovered)
            {
                topColor = LightenColor(baseColor, 15);
                bottomColor = baseColor;
                borderColor = DarkenColor(baseColor, 20);
            }
            else
            {
                topColor = LightenColor(baseColor, 10);
                bottomColor = DarkenColor(baseColor, 10);
                borderColor = DarkenColor(baseColor, 25);
            }

            // Limpiar el fondo completamente
            g.Clear(Color.Transparent);

            // Dibujar sombra externa más sutil
            DrawButtonShadowRounded(g, rect);

            // Crear gradiente principal con mejor definición
            var gradientRect = new Rectangle(rect.X, rect.Y, rect.Width, rect.Height);
            using (var brush = new LinearGradientBrush(gradientRect, topColor, bottomColor, LinearGradientMode.Vertical))
            {
                // Puntos de color para un gradiente más elegante
                var blend = new ColorBlend();
                blend.Colors = new[] {
                    LightenColor(topColor, 8),
                    topColor,
                    baseColor,
                    bottomColor,
                    DarkenColor(bottomColor, 5)
                };
                blend.Positions = new[] { 0.0f, 0.2f, 0.5f, 0.8f, 1.0f };
                brush.InterpolationColors = blend;

                // Dibujar fondo con bordes redondeados perfectos
                using (var path = CreateRoundedRectanglePath(gradientRect, 8))
                {
                    g.FillPath(brush, path);
                }
            }                // Dibujar borde exterior elegante con bisel
            using (var borderPen = new Pen(borderColor, 1f))
            using (var path = CreateRoundedRectanglePath(rect, 8))
            {
                g.DrawPath(borderPen, path);
            }
            // Borde interno más claro para efecto bisel
            var innerBorderRect = new Rectangle(rect.X + 1, rect.Y + 1, rect.Width - 2, rect.Height - 2);
            using (var innerBorderPen = new Pen(Color.FromArgb(30, Color.White), 1f))
            using (var path = CreateRoundedRectanglePath(innerBorderRect, 7))
            {
                g.DrawPath(innerBorderPen, path);
            }

            // Dibujar highlight interno más sutil
            var innerRect = new Rectangle(rect.X + 1, rect.Y + 1, rect.Width - 2, rect.Height / 3);
            using (var highlightBrush = new LinearGradientBrush(innerRect,
                Color.FromArgb(40, Color.White), Color.FromArgb(0, Color.White), LinearGradientMode.Vertical))
            using (var path = CreateRoundedRectanglePath(innerRect, 7))
            {
                g.FillPath(highlightBrush, path);
            }

            // Dibujar el contenido del botón (icono + texto)
            DrawButtonContent(g, button, rect, isPressed);
        };

        // Eventos de interacción con efectos suaves
        button.MouseEnter += (s, e) =>
        {
            isHovered = true;
            button.Invalidate();
            button.Cursor = Cursors.Hand;
        };
        button.MouseLeave += (s, e) =>
        {
            isHovered = false;
            isPressed = false;
            button.Invalidate();
            button.Cursor = Cursors.Default;
        };
        button.MouseDown += (s, e) =>
        {
            isPressed = true;
            button.Invalidate();
        };
        button.MouseUp += (s, e) =>
        {
            isPressed = false;
            button.Invalidate();
        };
    }

    /// <summary>
    /// Aplica efectos de profundidad a un panel.
    /// </summary>
    public static void MakePanelModern(Panel panel)
    {
        panel.BackColor = Color.Transparent;

        panel.Paint += (s, e) =>
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = panel.ClientRectangle;
            var theme = ThemeManager.CurrentTheme;

            // Sombra externa sutil
            DrawPanelShadow(g, rect);

            // Fondo con gradiente sutil
            using (var brush = new LinearGradientBrush(rect,
                Color.FromArgb(255, theme.PanelBackground),
                Color.FromArgb(255, DarkenColor(theme.PanelBackground, 5)),
                LinearGradientMode.Vertical))
            {
                g.FillRectangle(brush, rect);
            }

            // Borde elegante
            using (var pen = new Pen(Color.FromArgb(100, theme.Border), 1))
            {
                g.DrawRectangle(pen, 0, 0, rect.Width - 1, rect.Height - 1);
            }

            // Highlight superior
            using (var pen = new Pen(Color.FromArgb(40, Color.White), 1))
            {
                g.DrawLine(pen, 1, 1, rect.Width - 2, 1);
            }
        };
    }

    /// <summary>
    /// Mejora un RichTextBox con efectos visuales modernos y syntax highlighting.
    /// </summary>
    public static void MakeRichTextBoxModern(RichTextBox richTextBox, bool enableSyntaxHighlighting = false)
    {
        // Crear un panel contenedor personalizado
        var container = new Panel
        {
            BackColor = Color.Transparent,
            Padding = new Padding(2) // Padding más generoso para el borde
        };

        // Configurar el RichTextBox
        var originalMargin = richTextBox.Margin;
        richTextBox.BorderStyle = BorderStyle.None;
        richTextBox.Margin = Padding.Empty;
        richTextBox.Font = new Font("Consolas", 12F, FontStyle.Regular);
        richTextBox.WordWrap = false;
        richTextBox.AcceptsTab = true;

        // Aplicar syntax highlighting si se solicita
        if (enableSyntaxHighlighting)
        {
            SetupSyntaxHighlighting(richTextBox);
        }

        // Eventos de dibujo para el contenedor
        container.Paint += (s, e) =>
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = container.ClientRectangle;
            var theme = ThemeManager.CurrentTheme;

            // Sombra interna
            using (var shadowBrush = new SolidBrush(Color.FromArgb(30, Color.Black)))
            {
                g.FillRectangle(shadowBrush, 0, 0, rect.Width, 2);
                g.FillRectangle(shadowBrush, 0, 0, 2, rect.Height);
            }

            // Borde principal
            var borderColor = richTextBox.Focused ? theme.FocusBorder : theme.Border;
            using (var pen = new Pen(borderColor, 2))
            {
                g.DrawRectangle(pen, 1, 1, rect.Width - 3, rect.Height - 3);
            }

            // Highlight cuando tiene foco
            if (richTextBox.Focused)
            {
                using (var pen = new Pen(Color.FromArgb(80, theme.FocusBorder), 1))
                {
                    g.DrawRectangle(pen, 0, 0, rect.Width - 1, rect.Height - 1);
                }
            }
        };

        // Reemplazar el RichTextBox con el contenedor
        var parent = richTextBox.Parent;
        var location = richTextBox.Location;
        var size = richTextBox.Size;
        var dock = richTextBox.Dock;
        var anchor = richTextBox.Anchor;
        var tableParent = parent as TableLayoutPanel;
        int tableRow = 0;
        int tableColumn = 0;
        int rowSpan = 1;
        int columnSpan = 1;

        if (tableParent != null)
        {
            tableRow = tableParent.GetRow(richTextBox);
            tableColumn = tableParent.GetColumn(richTextBox);
            rowSpan = tableParent.GetRowSpan(richTextBox);
            columnSpan = tableParent.GetColumnSpan(richTextBox);
        }

        parent?.Controls.Remove(richTextBox);
        container.Controls.Add(richTextBox);
        richTextBox.Dock = DockStyle.Fill;

        container.Location = location;
        container.Size = size;
        container.Dock = dock;
        container.Anchor = anchor;
        container.Margin = originalMargin;

        if (tableParent != null)
        {
            tableParent.Controls.Add(container, tableColumn, tableRow);
            tableParent.SetRowSpan(container, rowSpan);
            tableParent.SetColumnSpan(container, columnSpan);
        }
        else
        {
            parent?.Controls.Add(container);
        }

        // Eventos para redibujado
        richTextBox.Enter += (s, e) => container.Invalidate();
        richTextBox.Leave += (s, e) => container.Invalidate();
    }

    #region Métodos auxiliares

    private static void SetupSyntaxHighlighting(RichTextBox richTextBox)
    {
        // Variable para evitar highlighting recursivo
        bool isUserInteracting = false;
        System.Windows.Forms.Timer? delayTimer = null;

        // Aplicar highlighting inicial si ya hay texto
        if (!string.IsNullOrEmpty(richTextBox.Text))
        {
            SyntaxHighlighter.ApplySyntaxHighlighting(richTextBox);
        }

        // Aplicar highlighting cuando el control esté listo
        richTextBox.HandleCreated += (s, e) =>
        {
            if (s is RichTextBox rtb && !string.IsNullOrEmpty(rtb.Text))
            {
                SyntaxHighlighter.ApplySyntaxHighlighting(rtb);
            }
        };

        // Aplicar highlighting cuando el usuario termine de escribir (pierde el foco)
        richTextBox.Leave += (s, e) =>
        {
            if (!isUserInteracting && s is RichTextBox rtb && !string.IsNullOrEmpty(rtb.Text))
            {
                SyntaxHighlighter.ApplySyntaxHighlighting(rtb, true);
            }
        };

        // Detectar cuando el usuario está seleccionando texto
        richTextBox.SelectionChanged += (s, e) =>
        {
            if (s is RichTextBox rtb && rtb.SelectionLength > 0)
            {
                isUserInteracting = true;
                delayTimer?.Stop();
                delayTimer?.Dispose();
                delayTimer = null;
            }
            else
            {
                isUserInteracting = false;
            }
        };

        // Aplicar highlighting con delay cuando el usuario escribe
        richTextBox.TextChanged += (s, e) =>
        {
            if (isUserInteracting || s is not RichTextBox rtb || string.IsNullOrEmpty(rtb.Text))
                return;

            // Cancelar timer anterior si existe
            delayTimer?.Stop();
            delayTimer?.Dispose();

            // Crear nuevo timer con delay más largo para evitar parpadeo
            delayTimer = new System.Windows.Forms.Timer { Interval = 1200 }; // 1.2s para mejor experiencia
            delayTimer.Tick += (ts, te) =>
            {
                delayTimer.Stop();
                delayTimer.Dispose();
                delayTimer = null;

                if (!isUserInteracting)
                {
                    SyntaxHighlighter.ApplySyntaxHighlighting(rtb, false);
                }
            };
            delayTimer.Start();
        };

        // Mejorar la configuración del editor
        richTextBox.DetectUrls = false;
        richTextBox.EnableAutoDragDrop = true;
        richTextBox.HideSelection = false;
        richTextBox.ShowSelectionMargin = true;
    }

    private static void CreateRoundedButtonRegion(Button button)
    {
        // Crear región redondeada que se adapte al tamaño del botón
        button.Resize += (s, e) => UpdateButtonRegion(button);
        button.HandleCreated += (s, e) => UpdateButtonRegion(button);

        // Aplicar región inicial
        UpdateButtonRegion(button);
    }

    private static void UpdateButtonRegion(Button button)
    {
        if (button.IsDisposed)
            return;

        try
        {
            using (var path = CreateRoundedRectanglePath(button.ClientRectangle, 8))
            {
                button.Region = new Region(path);
            }
        }
        catch
        {
            // Si hay error, usar región rectangular por defecto
            button.Region = new Region(button.ClientRectangle);
        }
    }

    private static void DrawButtonContent(Graphics g, Button button, Rectangle rect, bool isPressed)
    {
        // Ajustar posición si está presionado
        var contentRect = isPressed ?
            new Rectangle(rect.X + 1, rect.Y + 1, rect.Width - 2, rect.Height - 2) :
            rect;

        var padding = button.Padding;
        var availableRect = new Rectangle(
            contentRect.X + padding.Left,
            contentRect.Y + padding.Top,
            contentRect.Width - padding.Horizontal,
            contentRect.Height - padding.Vertical
        );

        // Configurar el texto para máxima calidad
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
        g.CompositingQuality = CompositingQuality.HighQuality;

        // Si el botón tiene imagen
        if (button.Image != null)
        {
            DrawButtonWithImageAndText(g, button, availableRect);
        }
        else
        {
            // Solo texto
            DrawButtonText(g, button, availableRect);
        }
    }

    private static void DrawButtonWithImageAndText(Graphics g, Button button, Rectangle availableRect)
    {
        const int imageSize = 18;
        const int spacing = 8;

        // Calcular posiciones para centrar todo el contenido
        var textSize = g.MeasureString(button.Text.Trim(), button.Font);
        var totalWidth = imageSize + spacing + (int)textSize.Width;

        var startX = availableRect.X + (availableRect.Width - totalWidth) / 2;
        var centerY = availableRect.Y + availableRect.Height / 2;

        // Dibujar icono
        var imageRect = new Rectangle(startX, centerY - imageSize / 2, imageSize, imageSize);
        if (button.Image != null)
        {
            g.DrawImage(button.Image, imageRect);
        }

        // Dibujar texto
        var textX = startX + imageSize + spacing;
        var textRect = new Rectangle(textX, availableRect.Y,
            (int)textSize.Width, availableRect.Height);

        using (var brush = new SolidBrush(button.ForeColor))
        {
            var format = new StringFormat
            {
                Alignment = StringAlignment.Near,
                LineAlignment = StringAlignment.Center,
                FormatFlags = StringFormatFlags.NoWrap
            };

            g.DrawString(button.Text.Trim(), button.Font, brush, textRect, format);
        }
    }

    private static void DrawButtonText(Graphics g, Button button, Rectangle availableRect)
    {
        using (var brush = new SolidBrush(button.ForeColor))
        {
            var format = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center,
                FormatFlags = StringFormatFlags.NoWrap
            };

            g.DrawString(button.Text.Trim(), button.Font, brush, availableRect, format);
        }
    }

    private static void DrawButtonShadowRounded(Graphics g, Rectangle rect)
    {
        // Sombra externa redondeada más elegante
        for (int i = 0; i < 4; i++)
        {
            var shadowRect = new Rectangle(
                rect.X + i,
                rect.Y + i + 1,
                rect.Width,
                rect.Height);

            var alpha = 15 - (i * 3); // Degradado de sombra
            using (var shadowBrush = new SolidBrush(Color.FromArgb(alpha, Color.Black)))
            using (var path = CreateRoundedRectanglePath(shadowRect, 8))
            {
                g.FillPath(shadowBrush, path);
            }
        }
    }

    private static void DrawButtonShadow(Graphics g, Rectangle rect)
    {
        // Sombra externa suave
        for (int i = 0; i < 3; i++)
        {
            var shadowRect = new Rectangle(rect.X + i + 2, rect.Y + i + 2, rect.Width, rect.Height);
            using (var shadowBrush = new SolidBrush(Color.FromArgb(20 - i * 5, Color.Black)))
            using (var path = CreateRoundedRectanglePath(shadowRect, 8))
            {
                g.FillPath(shadowBrush, path);
            }
        }
    }

    private static void DrawPanelShadow(Graphics g, Rectangle rect)
    {
        // Sombra sutil para paneles
        using (var shadowBrush = new SolidBrush(Color.FromArgb(20, Color.Black)))
        {
            g.FillRectangle(shadowBrush, 2, 2, rect.Width - 2, rect.Height - 2);
        }
    }

    private static GraphicsPath CreateRoundedRectanglePath(Rectangle rect, int radius)
    {
        var path = new GraphicsPath();
        var diameter = radius * 2;

        // Ajustar rectángulo para evitar overflow
        rect.Width -= 1;
        rect.Height -= 1;

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

    public static Color LightenColor(Color color, int percentage)
    {
        var factor = percentage / 100.0;
        var r = Math.Min(255, (int)(color.R + (255 - color.R) * factor));
        var g = Math.Min(255, (int)(color.G + (255 - color.G) * factor));
        var b = Math.Min(255, (int)(color.B + (255 - color.B) * factor));
        return Color.FromArgb(color.A, r, g, b);
    }

    public static Color DarkenColor(Color color, int percentage)
    {
        var factor = percentage / 100.0;
        var r = Math.Max(0, (int)(color.R * (1 - factor)));
        var g = Math.Max(0, (int)(color.G * (1 - factor)));
        var b = Math.Max(0, (int)(color.B * (1 - factor)));
        return Color.FromArgb(color.A, r, g, b);
    }

    #endregion
}
