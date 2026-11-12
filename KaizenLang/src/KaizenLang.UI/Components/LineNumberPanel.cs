using System;
using System.Drawing;
using System.Windows.Forms;

namespace KaizenLang.UI.Components;

public class LineNumberPanel : Panel
{
    private RichTextBox? associatedTextBox;
    private Font lineNumberFont;

    public LineNumberPanel()
    {
        lineNumberFont = new Font("Consolas", 9F);
        SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
        Width = 45;
        Dock = DockStyle.None;
        BorderStyle = BorderStyle.None;
    }

    public void AttachToTextBox(RichTextBox textBox)
    {
        associatedTextBox = textBox;
        if (associatedTextBox != null)
        {
            // Establecer margen izquierdo directamente
            SetTextBoxLeftMargin(associatedTextBox, 45); // 55 píxeles de margen

            // Posicionar el panel sobre el textbox
            this.Location = new Point(0, 0);
            this.Height = associatedTextBox.Height;
            this.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;

            associatedTextBox.TextChanged += (s, e) => Invalidate();
            associatedTextBox.VScroll += (s, e) => Invalidate();
            associatedTextBox.Resize += (s, e) =>
            {
                this.Height = associatedTextBox.Height;
                Invalidate();
            };
            associatedTextBox.SelectionChanged += (s, e) => Invalidate();
        }
    }

    private void SetTextBoxLeftMargin(RichTextBox textBox, int marginPixels)
    {
        // Guardar la posición actual del cursor
        int originalStart = textBox.SelectionStart;
        int originalLength = textBox.SelectionLength;

        // Seleccionar todo el texto
        textBox.SelectAll();

        // Establecer el margen izquierdo en píxeles
        textBox.SelectionIndent = marginPixels;

        // Restaurar la selección original
        textBox.Select(originalStart, originalLength);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        if (associatedTextBox == null)
            return;

        var g = e.Graphics;
        g.Clear(BackColor);
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        int firstIndex = associatedTextBox.GetCharIndexFromPosition(new Point(0, 0));
        int firstLine = associatedTextBox.GetLineFromCharIndex(firstIndex);

        int currentLine = associatedTextBox.GetLineFromCharIndex(associatedTextBox.SelectionStart);

        // Dibujar número 1 siempre
        bool isCurrentLine = currentLine == 0;
        using (Brush brush = new SolidBrush(isCurrentLine ? Color.FromArgb(255, 220, 220, 100) : ForeColor))
        {
            var lineText = "1";
            var textSize = g.MeasureString(lineText, lineNumberFont);
            var x = Width - textSize.Width - 8;
            var y = associatedTextBox.GetPositionFromCharIndex(0).Y;
            g.DrawString(lineText, lineNumberFont, brush, x, y);
        }

        int lastVisibleLine = firstLine;
        for (int i = firstIndex; i < associatedTextBox.TextLength; i++)
        {
            Point p = associatedTextBox.GetPositionFromCharIndex(i);
            if (p.Y > associatedTextBox.ClientSize.Height)
                break;

            int lineNumber = associatedTextBox.GetLineFromCharIndex(i);
            if (lineNumber > lastVisibleLine && lineNumber > 0)
            {
                lastVisibleLine = lineNumber;
                int displayLineNumber = lineNumber + 1;

                isCurrentLine = lineNumber == currentLine;

                using (Brush brush = new SolidBrush(isCurrentLine ? Color.FromArgb(255, 220, 220, 100) : ForeColor))
                {
                    var lineText = displayLineNumber.ToString();
                    var textSize = g.MeasureString(lineText, lineNumberFont);
                    var x = Width - textSize.Width - 8;
                    var y = p.Y;

                    g.DrawString(lineText, lineNumberFont, brush, x, y);
                }
            }
        }

        // Dibujar línea divisoria a la derecha
        using (Pen pen = new Pen(Color.FromArgb(60, 60, 60), 1))
        {
            g.DrawLine(pen, Width - 1, 0, Width - 1, Height);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            lineNumberFont?.Dispose();
        }
        base.Dispose(disposing);
    }
}
