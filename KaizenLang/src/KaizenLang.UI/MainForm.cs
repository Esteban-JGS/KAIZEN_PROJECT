using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using KaizenLang.UI.Components;
using KaizenLang.UI.Services;
using KaizenLang.UI.Theming;

namespace KaizenLang.UI;

public partial class MainForm : Form
{
    private readonly CompilationService compilationService;
    private readonly ExecutionService executionService;
    private ToolStripStatusLabel? statusLabel;
    private ToolStripStatusLabel? statusIcon;
    private ToolStripStatusLabel? lineColumnLabel;
    private ToolStripStatusLabel? timeLabel;
    private LineNumberPanel? lineNumberPanel;
    private SidebarPanel? sidebarInfo;
    private string? currentFilePath;
    private bool isModified;

    public MainForm()
    {
        InitializeComponent();

        compilationService = new CompilationService();
        executionService = new ExecutionService();
        executionService.InputProvider = prompt => Prompt.Show("Entrada requerida", prompt);

        InitializeCustomComponents();
        SetupKeyboardShortcuts();

        // Configurar ícono de la aplicación
        LoadApplicationIcon();

        this.ApplyCurrentThemeRecursive();

        // Cargar ejemplo y configurar números de línea cuando el formulario esté listo
        this.Load += (s, e) =>
        {
            SetupEnhancedFeatures();
            TestSyntaxHighlighting();
        };
    }

    private void InitializeCustomComponents()
    {
        // Configurar iconos del menú
        SetupMenuIcons();

        // Aplicar efectos visuales modernos
        ApplyVisualEffects();

        // Eventos de botones
        compileButton.Click += async (s, e) => await RunWithUIFeedback(
            compileButton, "Compilando...", "Compilar", async () =>
            {
                var result = await Task.Run(() => compilationService.CompileCode(codeRichTextBox.Text));
                DisplayResult(result.Output, result.IsSuccessful);
                UpdateStatus(result.IsSuccessful ? "Listo" : "Error", result.IsSuccessful);
            });

        executeButton.Click += async (s, e) => await RunWithUIFeedback(
            executeButton, "Ejecutando...", "Ejecutar", async () =>
            {
                var result = await Task.Run(() => executionService.ExecuteCode(codeRichTextBox.Text));
                DisplayResult(result.Output, result.IsSuccessful);
                UpdateStatus(result.IsSuccessful ? "Listo" : "Error", result.IsSuccessful);
            });

        // Menú Archivo
        nuevoToolStripMenuItem.Click += (s, e) => NewFile();
        abrirToolStripMenuItem.Click += (s, e) => OpenFile();
        guardarToolStripMenuItem.Click += (s, e) => SaveFile();
        guardarComoToolStripMenuItem.Click += (s, e) => SaveFileAs();

        // Items de menú -> snippets de código
        reservedWordsToolStripMenuItem.Click += (s, e) => InsertCodeSnippet(CodeSnippets.ReservedWords);

        // IF
        ifSimpleToolStripMenuItem.Click += (s, e) => InsertCodeSnippet(CodeSnippets.IfSimple);
        ifElseToolStripMenuItem.Click += (s, e) => InsertCodeSnippet(CodeSnippets.IfElse);
        ifComparacionToolStripMenuItem.Click += (s, e) => InsertCodeSnippet(CodeSnippets.IfComparacion);
        ifBooleanoToolStripMenuItem.Click += (s, e) => InsertCodeSnippet(CodeSnippets.IfBooleano);
        ifStringToolStripMenuItem.Click += (s, e) => InsertCodeSnippet(CodeSnippets.IfString);

        // WHILE
        whileContadorToolStripMenuItem.Click += (s, e) => InsertCodeSnippet(CodeSnippets.WhileContador);
        whileSumaToolStripMenuItem.Click += (s, e) => InsertCodeSnippet(CodeSnippets.WhileSuma);
        whileCondicionToolStripMenuItem.Click += (s, e) => InsertCodeSnippet(CodeSnippets.WhileCondicion);
        whileMenuToolStripMenuItem.Click += (s, e) => InsertCodeSnippet(CodeSnippets.WhileMenu);

        // FOR
        forContadorToolStripMenuItem.Click += (s, e) => InsertCodeSnippet(CodeSnippets.ForContador);
        forSumaToolStripMenuItem.Click += (s, e) => InsertCodeSnippet(CodeSnippets.ForSuma);
        forTablaToolStripMenuItem.Click += (s, e) => InsertCodeSnippet(CodeSnippets.ForTabla);
        forParesToolStripMenuItem.Click += (s, e) => InsertCodeSnippet(CodeSnippets.ForPares);
        forDescendenteToolStripMenuItem.Click += (s, e) => InsertCodeSnippet(CodeSnippets.ForDescendente);

        // FUNCTIONS
        functionSimpleToolStripMenuItem.Click += (s, e) => InsertCodeSnippet(CodeSnippets.FunctionSimple);
        functionConParametrosToolStripMenuItem.Click += (s, e) => InsertCodeSnippet(CodeSnippets.FunctionConParametros);
        functionMultiplicarToolStripMenuItem.Click += (s, e) => InsertCodeSnippet(CodeSnippets.FunctionMultiplicar);
        functionEsParToolStripMenuItem.Click += (s, e) => InsertCodeSnippet(CodeSnippets.FunctionEsPar);
        functionMaximoToolStripMenuItem.Click += (s, e) => InsertCodeSnippet(CodeSnippets.FunctionMaximo);

        // OPERATIONS
        operationsAritmeticasToolStripMenuItem.Click += (s, e) => InsertCodeSnippet(CodeSnippets.OperationsAritmeticas);
        operationsComparacionToolStripMenuItem.Click += (s, e) => InsertCodeSnippet(CodeSnippets.OperationsComparacion);
        operationsLogicasToolStripMenuItem.Click += (s, e) => InsertCodeSnippet(CodeSnippets.OperationsLogicas);

        // INPUT/OUTPUT
        inputOutputBasicoToolStripMenuItem.Click += (s, e) => InsertCodeSnippet(CodeSnippets.InputOutputBasico);
        inputOutputCalculadoraToolStripMenuItem.Click += (s, e) => InsertCodeSnippet(CodeSnippets.InputOutputCalculadora);

        // DATA TYPES
        dataTypesBasicoToolStripMenuItem.Click += (s, e) => InsertCodeSnippet(CodeSnippets.DataTypesBasico);
        dataTypesConversionesToolStripMenuItem.Click += (s, e) => InsertCodeSnippet(CodeSnippets.DataTypesConversiones);

        // Barra de estado mejorada
        SetupEnhancedStatusBar();

        // Ajustes extra para editor y salida
        codeRichTextBox.ScrollBars = RichTextBoxScrollBars.Both;
        codeRichTextBox.AcceptsTab = true;

        outputRichTextBox.ReadOnly = true;
        outputRichTextBox.ShortcutsEnabled = true;
        outputRichTextBox.ScrollBars = RichTextBoxScrollBars.Vertical;

        ThemeManager.ThemeChanged += ThemeManagerOnThemeChanged;
        RefreshEditorVisuals();
    }

    private void ApplyVisualEffects()
    {
        var theme = ThemeManager.CurrentTheme;

        // Aplicar efectos 3D reales a los botones principales
        EnhancedVisualEffects.MakeButtonModern(compileButton, theme.ButtonBackground, true);
        EnhancedVisualEffects.MakeButtonModern(executeButton, theme.ButtonBackground, true);

        // Mejorar la apariencia de los RichTextBox con efectos reales y resalte de sintaxis
        EnhancedVisualEffects.MakeRichTextBoxModern(codeRichTextBox, true);  // dejo resaltado de sintaxis
        EnhancedVisualEffects.MakeRichTextBoxModern(outputRichTextBox, false); // sin resaltado en salida

        // Aplicar efectos de profundidad a paneles
        ApplyPanelEffects();

        // Mejorar el espaciado general
        ImproveLayoutSpacing();

        // Agregar gradiente de fondo al formulario principal
        ApplyFormBackgroundGradient();
    }

    private void ApplyPanelEffects()
    {
        // Buscar y mejorar todos los paneles
        foreach (Control control in this.Controls)
        {
            if (control is Panel panel)
            {
                EnhancedVisualEffects.MakePanelModern(panel);
            }
        }
    }

    private void ApplyFormBackgroundGradient()
    {
        // Aplicar gradiente sutil al fondo del formulario
        this.Paint += (s, e) =>
        {
            var g = e.Graphics;
            var rect = this.ClientRectangle;
            var theme = ThemeManager.CurrentTheme;

            using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(rect,
                theme.Background,
                EnhancedVisualEffects.DarkenColor(theme.Background, 8),
                System.Drawing.Drawing2D.LinearGradientMode.Vertical))
            {
                g.FillRectangle(brush, rect);
            }
        };
    }

    private void AddVisualSeparators()
    {
        // Este método se puede expandir para agregar separadores entre secciones
        // Por ahora, mejoramos el espaciado de los controles existentes
    }

    private void ImproveLayoutSpacing()
    {
        // Mejorar márgenes y espaciado general
        this.Padding = new Padding(15);

        // Ajustar espaciado de botones
        if (compileButton != null && executeButton != null)
        {
            compileButton.Margin = new Padding(5);
            executeButton.Margin = new Padding(5);

            // Asegurar altura consistente para botones
            compileButton.Height = 40;
            executeButton.Height = 40;
            compileButton.MinimumSize = new Size(120, 40);
            executeButton.MinimumSize = new Size(120, 40);
        }
    }

    private void InsertCodeSnippet(string snippet)
    {
        codeRichTextBox.Clear();
        codeRichTextBox.Text = snippet;
    }

    private void SetupMenuIcons()
    {
        // Configurar iconos de los elementos principales del menú
        languageStructuresToolStripMenuItem.Image = IconFactory.GetIcon("examples", 16, 16);
        reservedWordsToolStripMenuItem.Image = IconFactory.GetIcon("keywords", 16, 16);
        syntaxToolStripMenuItem.Image = IconFactory.GetIcon("syntax", 16, 16);
        dataTypesToolStripMenuItem.Image = IconFactory.GetIcon("datatypes", 16, 16);

        // Configurar iconos de submenús
        controlToolStripMenuItem.Image = IconFactory.GetIcon("control", 16, 16);
        functionsToolStripMenuItem.Image = IconFactory.GetIcon("functions", 16, 16);
        operationsToolStripMenuItem.Image = IconFactory.GetIcon("operations", 16, 16);
        inputOutputToolStripMenuItem.Image = IconFactory.GetIcon("input", 16, 16);

        // Configurar iconos de los botones principales
        SetupButtonIcons();

        // Aplicar efectos visuales al menú
        ApplyMenuEffects();
    }

    private void ApplyMenuEffects()
    {
        // Buscar el MenuStrip en los controles
        foreach (Control control in this.Controls)
        {
            if (control is MenuStrip menuStrip)
            {
                var theme = ThemeManager.CurrentTheme;

                // Configurar renderer personalizado para efectos visuales
                menuStrip.RenderMode = ToolStripRenderMode.Professional;
                menuStrip.Renderer = new ModernMenuRenderer(theme);

                // Aplicar font más elegante
                menuStrip.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);

                break;
            }
        }
    }

    private void SetupButtonIcons()
    {
        // Asignar iconos a los botones principales con mejor espaciado
        var compileIcon = IconFactory.GetIcon("compile", 18, 18);
        var executeIcon = IconFactory.GetIcon("execute", 18, 18);

        if (compileIcon != null)
        {
            compileButton.Image = compileIcon;
            compileButton.ImageAlign = ContentAlignment.MiddleLeft;
            compileButton.TextAlign = ContentAlignment.MiddleCenter;
            compileButton.Text = "   Compilar";
            compileButton.TextImageRelation = TextImageRelation.ImageBeforeText;
        }

        if (executeIcon != null)
        {
            executeButton.Image = executeIcon;
            executeButton.ImageAlign = ContentAlignment.MiddleLeft;
            executeButton.TextAlign = ContentAlignment.MiddleCenter;
            executeButton.Text = "   Ejecutar";
            executeButton.TextImageRelation = TextImageRelation.ImageBeforeText;
        }
    }

    private void SetupEnhancedStatusBar()
    {
        var statusStrip = new StatusStrip();
        var theme = ThemeManager.CurrentTheme;

        // Configurar tema de la barra de estado
        statusStrip.BackColor = theme.SecondaryBackground;
        statusStrip.ForeColor = theme.SecondaryForeground;
        statusStrip.RenderMode = ToolStripRenderMode.Professional;

        // Ícono de estado
        statusIcon = new ToolStripStatusLabel
        {
            Image = IconFactory.GetIcon("info", 16, 16),
            DisplayStyle = ToolStripItemDisplayStyle.Image,
            AutoSize = false,
            Width = 20
        };

        // Mensaje principal
        statusLabel = new ToolStripStatusLabel("Listo")
        {
            Spring = true,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Segoe UI", 9F, FontStyle.Regular)
        };

        // Posición del cursor (línea, columna)
        lineColumnLabel = new ToolStripStatusLabel("Ln 1, Col 1")
        {
            AutoSize = false,
            Width = 80,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 9F, FontStyle.Regular),
            BorderSides = ToolStripStatusLabelBorderSides.Left
        };

        // Hora actual
        timeLabel = new ToolStripStatusLabel(DateTime.Now.ToString("HH:mm"))
        {
            AutoSize = false,
            Width = 50,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 9F, FontStyle.Regular),
            BorderSides = ToolStripStatusLabelBorderSides.Left
        };

        // Agregar elementos a la barra
        statusStrip.Items.AddRange(new ToolStripItem[]
        {
            statusIcon,
            statusLabel,
            lineColumnLabel,
            timeLabel
        });

        this.Controls.Add(statusStrip);

        // Configurar eventos para actualizaciones
        SetupStatusBarEvents();
    }

    private void SetupStatusBarEvents()
    {
        // Timer para actualizar la hora cada minuto
        var timeTimer = new System.Windows.Forms.Timer { Interval = 60000 }; // 1 minuto
        timeTimer.Tick += (s, e) =>
        {
            if (timeLabel != null)
                timeLabel.Text = DateTime.Now.ToString("HH:mm");
        };
        timeTimer.Start();

        // Eventos para actualizar posición del cursor
        if (codeRichTextBox != null)
        {
            codeRichTextBox.SelectionChanged += UpdateCursorPosition;
            codeRichTextBox.TextChanged += UpdateCursorPosition;
        }
    }

    private void UpdateCursorPosition(object? sender, EventArgs e)
    {
        if (lineColumnLabel == null || codeRichTextBox == null)
            return;

        try
        {
            var selectionStart = codeRichTextBox.SelectionStart;
            var line = codeRichTextBox.GetLineFromCharIndex(selectionStart) + 1;
            var column = selectionStart - codeRichTextBox.GetFirstCharIndexFromLine(line - 1) + 1;

            lineColumnLabel.Text = $"Ln {line}, Col {column}";
        }
        catch
        {
            lineColumnLabel.Text = "Ln 1, Col 1";
        }
    }

    private void UpdateStatus(string message, bool success)
    {
        if (statusLabel == null || statusIcon == null)
            return;

        // Si es error, cambiar el mensaje a "Error"
        statusLabel.Text = success ? message : "Error";

        // Cambiar color según el estado
        if (success)
        {
            statusLabel.ForeColor = Color.FromArgb(50, 205, 50); // Verde
        }
        else
        {
            statusLabel.ForeColor = Color.FromArgb(220, 50, 50); // Rojo
        }

        // Actualizar ícono según el estado
        var iconName = success ? "success" : "error";
        var icon = IconFactory.GetIcon(iconName, 16, 16);
        if (icon != null)
        {
            statusIcon.Image = icon;
        }
    }

    private void UpdateStatusWithIcon(string message, string iconName)
    {
        if (statusLabel == null || statusIcon == null)
            return;

        statusLabel.Text = message;

        // Color verde para "Listo", color normal para otros
        if (message == "Listo")
        {
            statusLabel.ForeColor = Color.FromArgb(50, 205, 50); // Verde
        }
        else
        {
            statusLabel.ForeColor = SystemColors.ControlText; // Color normal
        }

        var icon = IconFactory.GetIcon(iconName, 16, 16);
        if (icon != null)
        {
            statusIcon.Image = icon;
        }
    }

    /// <summary>
    /// Centraliza la lógica de feedback visual: deshabilita botón, cambia texto, ejecuta acción, restaura estado.
    /// </summary>
    private async Task RunWithUIFeedback(Button button, string workingText, string defaultText, Func<Task> action)
    {
        try
        {
            button.Enabled = false;
            button.Text = workingText;
            UpdateStatusWithIcon("Procesando...", "loading");

            await action();
        }
        catch (Exception ex)
        {
            DisplayResult($"ERROR inesperado: {ex.Message}", false);
            UpdateStatus("Error", false);
        }
        finally
        {
            button.Enabled = true;
            button.Text = defaultText;
            // NO restaurar estado aquí - el UpdateStatus de la acción ya lo estableció correctamente
        }
    }

    /// <summary>
    /// Muestra resultados en el RichTextBox usando el color de primer plano del tema actual.
    /// </summary>
    private void DisplayResult(string output, bool success)
    {
        var theme = ThemeManager.CurrentTheme;

        outputRichTextBox.ReadOnly = false;
        outputRichTextBox.Clear();
        var foreground = success ? theme.SecondaryForeground : theme.Error;
        outputRichTextBox.SelectionColor = foreground;
        outputRichTextBox.AppendText(output);
        outputRichTextBox.SelectionColor = outputRichTextBox.ForeColor;
        outputRichTextBox.ReadOnly = true;
        outputRichTextBox.SelectionStart = outputRichTextBox.TextLength;
        outputRichTextBox.ScrollToCaret();
    }

    /// <summary>
    /// Prueba el sistema de syntax highlighting cargando código de ejemplo
    /// </summary>
    public void TestSyntaxHighlighting()
    {
        try
        {
            // Código de ejemplo embebido para probar syntax highlighting con sintaxis KaizenLang
            var exampleCode = @"// ============================================
// EJEMPLOS DE CÓDIGO EN KAIZENLANG
// ============================================

// EJEMPLO 1: Declaraciones básicas de variables
gear numero = 42;
grimoire mensaje = ""Hola KaizenLang"";
shin activo = true;
shikai precio = 19.99;

// EJEMPLO 2: Estructuras de control con ying/yang
if (numero > 0) ying
    output(""El número es positivo"");
yang else ying
    output(""El número es negativo o cero"");
yang

// EJEMPLO 3: Bucle while
gear contador = 0;
while (contador < 5) ying
    output(""Contador: "" + contador);
    contador = contador + 1;
yang

// EJEMPLO 4: Bucle for
for (gear i = 0; i < 10; i++) ying
    output(""Iteración: "" + i);
yang

// EJEMPLO 5: Función con sintaxis KaizenLang
gear factorial(gear n) ying
    if (n <= 1) ying
        return 1;
    yang
    return n * factorial(n - 1);
yang

// EJEMPLO 6: Función void
void saludar(grimoire nombre) ying
    output(""Hola "" + nombre + ""!"");
yang

// EJEMPLO 7: Chainsaw y Hogyoku
chainsaw<gear> numeros = [1, 2, 3, 4, 5];
hogyoku<gear> tabla = [[1, 2, 3], [4, 5, 6], [7, 8, 9]];

// EJEMPLO 8: Funciones builtin
grimoire name = input();
output(name);
gear len = length(numeros);

// EJEMPLO 9: Operaciones complejas
gear resultado = (10 + 5) * 2;
shin comparacion = (resultado > 25) && (numero < 100);

// EJEMPLO 10: Acceso a chainsaw y hogyoku
gear primero = numeros[0];
gear elemento = tabla[0][1];

saludar(""KaizenLang"");";
            codeRichTextBox.Text = exampleCode;

            // Aplicar syntax highlighting manualmente (forzado para el ejemplo inicial)
            SyntaxHighlighter.ApplySyntaxHighlighting(codeRichTextBox, true);

            DisplayResult("✅ Syntax highlighting para KaizenLang aplicado correctamente!\n\nPuedes ver:\n- Comentarios en verde e itálica (//)\n- Palabras clave en bold (if, else, for, while, ying, yang)\n- Tipos en bold (integer, string, bool, chainsaw<>, hogyoku<>)\n- Funciones builtin (input, output, length)\n- Cadenas de texto coloreadas\n- Números resaltados\n- Operadores y símbolos destacados", true);
        }
        catch (Exception ex)
        {
            DisplayResult($"❌ Error al cargar ejemplo de syntax highlighting: {ex.Message}", false);
        }
    }

    private void ThemeManagerOnThemeChanged(object? sender, EventArgs e)
    {
        RefreshEditorVisuals();
    }

    private void RefreshEditorVisuals()
    {
        if (codeSectionLayout == null || outputSectionLayout == null)
            return;

        var theme = ThemeManager.CurrentTheme;

        codeSectionLayout.BackColor = BlendColor(theme.PanelBackground, theme.Background, 0.15f);
        codeSectionLayout.Padding = new Padding(12, 14, 12, 14);

        outputSectionLayout.BackColor = BlendColor(theme.SecondaryBackground, theme.Background, 0.08f);
        outputSectionLayout.Padding = new Padding(12, 14, 12, 14);

        if (codeHeaderLabel != null)
            codeHeaderLabel.Font = ChooseFont(12.5f, FontStyle.Bold, "Segoe UI Semibold", "Segoe UI", "Calibri", "Verdana");

        if (codeSubtitleLabel != null)
        {
            codeSubtitleLabel.Font = ChooseFont(9.75f, FontStyle.Regular, "Segoe UI", "Calibri", "Verdana");
            codeSubtitleLabel.ForeColor = ControlPaint.Light(theme.Foreground, 0.35f);
        }

        if (outputHeaderLabel != null)
            outputHeaderLabel.Font = ChooseFont(11.5f, FontStyle.Bold, "Segoe UI Semibold", "Segoe UI", "Calibri", "Verdana");

        if (outputSubtitleLabel != null)
        {
            outputSubtitleLabel.Font = ChooseFont(9.5f, FontStyle.Regular, "Segoe UI", "Calibri", "Verdana");
            outputSubtitleLabel.ForeColor = ControlPaint.Light(theme.Foreground, 0.35f);
        }

        if (codeRichTextBox != null)
        {
            codeRichTextBox.Font = ChooseFont(13f, FontStyle.Regular, "Cascadia Code", "JetBrains Mono", "Fira Code", "Consolas");
            codeRichTextBox.ForeColor = theme.TextBoxForeground;
            codeRichTextBox.BackColor = BlendColor(theme.TextBoxBackground, theme.PanelBackground, 0.05f);
        }

        if (outputRichTextBox != null)
        {
            outputRichTextBox.Font = ChooseFont(10.5f, FontStyle.Regular, "Segoe UI", "Inter", "Calibri", "Arial");
            outputRichTextBox.ForeColor = theme.SecondaryForeground;
            outputRichTextBox.BackColor = BlendColor(theme.SecondaryBackground, theme.Background, 0.12f);
        }
    }

    private static Font ChooseFont(float size, FontStyle style, params string[] candidates)
    {
        foreach (var candidate in candidates)
        {
            try
            {
                using (var test = new Font(candidate, size, style, GraphicsUnit.Point))
                {
                    if (string.Equals(test.Name, candidate, StringComparison.OrdinalIgnoreCase))
                        return new Font(candidate, size, style, GraphicsUnit.Point);
                }
            }
            catch
            {
                // Ignore invalid fonts and continue with the next candidate
            }
        }

        return new Font(FontFamily.GenericMonospace, size, style, GraphicsUnit.Point);
    }

    private static Color BlendColor(Color source, Color target, float amount)
    {
        amount = Math.Clamp(amount, 0f, 1f);
        int r = (int)Math.Round(source.R + (target.R - source.R) * amount);
        int g = (int)Math.Round(source.G + (target.G - source.G) * amount);
        int b = (int)Math.Round(source.B + (target.B - source.B) * amount);
        return Color.FromArgb(r, g, b);
    }

    private void LoadApplicationIcon()
    {
        try
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var possiblePaths = new[]
            {
                Path.Combine(baseDir, "Resources", "icon.ico"),
                Path.Combine(baseDir, "..", "..", "Resources", "icon.ico"),
                Path.Combine(baseDir, "..", "..", "..", "..", "..", "Resources", "icon.ico"),
                Path.Combine(Directory.GetCurrentDirectory(), "Resources", "icon.ico"),
                Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "Resources", "icon.ico")
            };

            foreach (var iconPath in possiblePaths)
            {
                var fullPath = Path.GetFullPath(iconPath);
                if (File.Exists(fullPath))
                {
                    this.Icon = new Icon(fullPath);
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"No se pudo cargar el ícono de la aplicación: {ex.Message}");
        }
    }

    private void SetupEnhancedFeatures()
    {
        // Agregar números de línea al editor
        lineNumberPanel = new LineNumberPanel();

        var theme = ThemeManager.CurrentTheme;
        lineNumberPanel.BackColor = theme.TextBoxBackground;
        lineNumberPanel.ForeColor = Color.FromArgb(100, 100, 100);
        lineNumberPanel.Visible = true;

        // Buscar el contenedor padre del codeRichTextBox
        Control? parent = codeRichTextBox.Parent;

        if (parent != null)
        {
            // Agregar el panel directamente al padre del RichTextBox
            parent.Controls.Add(lineNumberPanel);
            lineNumberPanel.BringToFront();

            // Adjuntar al textbox (esto establecerá el margen izquierdo)
            lineNumberPanel.AttachToTextBox(codeRichTextBox);

            System.Diagnostics.Debug.WriteLine($"LineNumberPanel agregado: Visible={lineNumberPanel.Visible}, Width={lineNumberPanel.Width}");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"⚠️ No se pudo agregar LineNumberPanel. Parent es null");
        }

        // Configurar el sidebar con información
        if (sidebarPanel != null)
        {
            sidebarInfo = new SidebarPanel();
            sidebarInfo.UpdateTheme(theme);
            sidebarPanel.Controls.Add(sidebarInfo);
        }

        // Detectar cambios en el código
        codeRichTextBox.TextChanged += (s, e) =>
        {
            isModified = true;
            UpdateTitle();
        };
    }

    private void SetupKeyboardShortcuts()
    {
        this.KeyPreview = true;
        this.KeyDown += (s, e) =>
        {
            // F5 - Ejecutar
            if (e.KeyCode == Keys.F5)
            {
                e.Handled = true;
                executeButton.PerformClick();
            }
            // F6 - Compilar
            else if (e.KeyCode == Keys.F6)
            {
                e.Handled = true;
                compileButton.PerformClick();
            }
            // Ctrl+N - Nuevo
            else if (e.Control && e.KeyCode == Keys.N)
            {
                e.Handled = true;
                NewFile();
            }
            // Ctrl+O - Abrir
            else if (e.Control && e.KeyCode == Keys.O)
            {
                e.Handled = true;
                OpenFile();
            }
            // Ctrl+S - Guardar
            else if (e.Control && e.KeyCode == Keys.S)
            {
                e.Handled = true;
                SaveFile();
            }
        };
    }

    private void NewFile()
    {
        if (isModified)
        {
            var result = MessageBox.Show(
                "¿Desea guardar los cambios antes de crear un nuevo archivo?",
                "Cambios sin guardar",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                SaveFile();
            }
            else if (result == DialogResult.Cancel)
            {
                return;
            }
        }

        codeRichTextBox.Clear();
        outputRichTextBox.Clear();
        currentFilePath = null;
        isModified = false;
        UpdateTitle();
        UpdateStatus("Nuevo archivo creado", true);
    }

    private void OpenFile()
    {
        using (var openFileDialog = new OpenFileDialog())
        {
            openFileDialog.Filter = "KaizenLang Files (*.kz)|*.kz|Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
            openFileDialog.Title = "Abrir archivo KaizenLang";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var content = File.ReadAllText(openFileDialog.FileName);
                    codeRichTextBox.Text = content;
                    currentFilePath = openFileDialog.FileName;
                    isModified = false;
                    UpdateTitle();
                    UpdateStatus($"Archivo cargado: {Path.GetFileName(currentFilePath)}", true);

                    // Aplicar syntax highlighting
                    SyntaxHighlighter.ApplySyntaxHighlighting(codeRichTextBox, true);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al abrir el archivo:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }

    private void SaveFile()
    {
        if (string.IsNullOrEmpty(currentFilePath))
        {
            SaveFileAs();
        }
        else
        {
            try
            {
                File.WriteAllText(currentFilePath, codeRichTextBox.Text);
                isModified = false;
                UpdateTitle();
                UpdateStatus($"Archivo guardado: {Path.GetFileName(currentFilePath)}", true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar el archivo:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void SaveFileAs()
    {
        using (var saveFileDialog = new SaveFileDialog())
        {
            saveFileDialog.Filter = "KaizenLang Files (*.kz)|*.kz|Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
            saveFileDialog.Title = "Guardar archivo KaizenLang";
            saveFileDialog.DefaultExt = "kz";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    File.WriteAllText(saveFileDialog.FileName, codeRichTextBox.Text);
                    currentFilePath = saveFileDialog.FileName;
                    isModified = false;
                    UpdateTitle();
                    UpdateStatus($"Archivo guardado: {Path.GetFileName(currentFilePath)}", true);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al guardar el archivo:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }

    private void UpdateTitle()
    {
        var fileName = string.IsNullOrEmpty(currentFilePath) ? "Sin título" : Path.GetFileName(currentFilePath);
        var modified = isModified ? "*" : "";
        this.Text = $"KaizenLang IDE - {fileName}{modified}";
    }
}
