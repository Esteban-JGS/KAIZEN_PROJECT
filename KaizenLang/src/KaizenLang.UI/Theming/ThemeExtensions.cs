namespace KaizenLang.UI.Theming;

/// <summary>
/// Extensiones para facilitar la aplicación de temas a controles
/// </summary>
public static class ThemeExtensions
{
    /// <summary>
    /// Aplica el tema actual a un control específico
    /// </summary>
    public static void ApplyCurrentTheme(this Control control)
    {
        ThemeManager.ApplyThemeToControl(control);
    }

    /// <summary>
    /// Aplica el tema actual a un control y todos sus hijos
    /// </summary>
    public static void ApplyCurrentThemeRecursive(this Control control)
    {
        ThemeManager.ApplyThemeToAllControls(control);
    }

    /// <summary>
    /// Suscribe un control para recibir actualizaciones automáticas de tema
    /// </summary>
    public static void SubscribeToThemeUpdates(this Control control)
    {
        ThemeManager.SubscribeToThemeChanges(control);
    }

    /// <summary>
    /// Aplica un estilo específico de botón usando ButtonsTheme
    /// </summary>
    // public static void ApplyButtonStyle(this ButtonBase button, ButtonsTheme.ButtonStyle style)
    // {
    //     ButtonsTheme.ApplyButtonStyle(button, style);
    // }

    /// <summary>
    /// Aplica colores de estado (error, warning, info) a un control
    /// </summary>
    public static void ApplyStatusColor(this Control control, StatusType status)
    {
        var theme = ThemeManager.CurrentTheme;

        switch (status)
        {
            case StatusType.Error:
                control.ForeColor = theme.Error;
                break;
            case StatusType.Warning:
                control.ForeColor = theme.Warning;
                break;
            case StatusType.Info:
                control.ForeColor = theme.Info;
                break;
            case StatusType.Success:
                control.ForeColor = Color.FromArgb(40, 167, 69); // Verde éxito
                break;
            case StatusType.Normal:
            default:
                control.ForeColor = theme.Foreground;
                break;
        }
    }

    /// <summary>
    /// Crea un panel con el estilo actual del tema
    /// </summary>
    public static Panel CreateThemedPanel(bool useSecondaryColors = false)
    {
        var theme = ThemeManager.CurrentTheme;

        return new Panel
        {
            BackColor = useSecondaryColors ? theme.SecondaryBackground : theme.Background,
            ForeColor = useSecondaryColors ? theme.SecondaryForeground : theme.Foreground,
            BorderStyle = BorderStyle.None
        };
    }

    /// <summary>
    /// Crea una etiqueta con el estilo actual del tema
    /// </summary>
    public static Label CreateThemedLabel(string text, bool bold = false)
    {
        var theme = ThemeManager.CurrentTheme;

        return new Label
        {
            Text = text,
            BackColor = theme.LabelBackground,
            ForeColor = theme.LabelForeground,
            Font = new Font(theme.DefaultFontFamily ?? "Segoe UI",
                           theme.DefaultFontSize,
                           bold ? FontStyle.Bold : FontStyle.Regular),
            AutoSize = true
        };
    }

    /// <summary>
    /// Crea un separador visual con el color del tema
    /// </summary>
    public static Panel CreateThemedSeparator(int height = 1, bool horizontal = true)
    {
        var theme = ThemeManager.CurrentTheme;

        return new Panel
        {
            BackColor = theme.Border,
            Height = horizontal ? height : 0,
            Width = horizontal ? 0 : height,
            Dock = horizontal ? DockStyle.Top : DockStyle.Left
        };
    }

    /// <summary>
    /// Aplica efecto de resaltado al pasar el mouse sobre un control
    /// </summary>
    public static void EnableHoverEffect(this Control control, Color? hoverColor = null)
    {
        var theme = ThemeManager.CurrentTheme;
        var originalColor = control.BackColor;
        var targetHoverColor = hoverColor ?? ChangeColorBrightness(originalColor, 0.1f);

        control.MouseEnter += (s, e) => control.BackColor = targetHoverColor;
        control.MouseLeave += (s, e) => control.BackColor = originalColor;
    }

    /// <summary>
    /// Cambia el brillo de un color (método de utilidad)
    /// </summary>
    private static Color ChangeColorBrightness(Color color, float correctionFactor)
    {
        float red = color.R;
        float green = color.G;
        float blue = color.B;

        if (correctionFactor < 0)
        {
            correctionFactor = 1 + correctionFactor;
            red *= correctionFactor;
            green *= correctionFactor;
            blue *= correctionFactor;
        }
        else
        {
            red = (255 - red) * correctionFactor + red;
            green = (255 - green) * correctionFactor + green;
            blue = (255 - blue) * correctionFactor + blue;
        }

        return Color.FromArgb(color.A, (int)red, (int)green, (int)blue);
    }
}

/// <summary>
/// Tipos de estado para colores
/// </summary>
public enum StatusType
{
    Normal,
    Success,
    Warning,
    Error,
    Info
}
