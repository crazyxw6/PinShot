namespace PinShot;

internal sealed class ToolbarIconButton : Control
{
    private bool hovered;
    private bool pressed;
    private bool selected;

    public ToolbarIconButton(string icon)
    {
        Text = icon;
        Size = new Size(38, 34);
        Margin = new Padding(3, 0, 3, 0);
        Cursor = Cursors.Hand;
        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw |
            ControlStyles.UserPaint,
            true);
    }

    public bool Selected
    {
        get => selected;
        set
        {
            selected = value;
            Invalidate();
        }
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        hovered = true;
        Invalidate();
        base.OnMouseEnter(e);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        hovered = false;
        pressed = false;
        Invalidate();
        base.OnMouseLeave(e);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            pressed = true;
            Invalidate();
        }

        base.OnMouseDown(e);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        pressed = false;
        Invalidate();
        base.OnMouseUp(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        var fill = selected
            ? Color.FromArgb(235, 73, 158, 255)
            : pressed
                ? Color.FromArgb(96, 255, 255, 255)
                : hovered
                    ? Color.FromArgb(62, 255, 255, 255)
                    : Color.FromArgb(28, 255, 255, 255);

        using var clearBrush = new SolidBrush(Color.FromArgb(232, 16, 16, 18));
        e.Graphics.FillRectangle(clearBrush, ClientRectangle);

        using var fillBrush = new SolidBrush(fill);
        e.Graphics.FillRoundedRectangle(fillBrush, new RectangleF(2, 2, Width - 4, Height - 4), 5);

        var iconColor = selected ? Color.White : Color.FromArgb(242, 255, 255, 255);
        using var iconPen = new Pen(iconColor, 2)
        {
            StartCap = System.Drawing.Drawing2D.LineCap.Round,
            EndCap = System.Drawing.Drawing2D.LineCap.Round,
            LineJoin = System.Drawing.Drawing2D.LineJoin.Round
        };
        using var iconBrush = new SolidBrush(iconColor);

        DrawIcon(e.Graphics, iconPen, iconBrush);
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
    }

    private void DrawIcon(Graphics graphics, Pen pen, Brush brush)
    {
        var rect = new Rectangle(10, 8, Width - 20, Height - 16);

        switch (Text)
        {
            case "□":
                graphics.DrawRectangle(pen, rect);
                break;
            case "○":
                graphics.DrawEllipse(pen, rect);
                break;
            case "↗":
                pen.CustomEndCap = new System.Drawing.Drawing2D.AdjustableArrowCap(4, 5);
                graphics.DrawLine(pen, 11, Height - 10, Width - 11, 9);
                break;
            case "✎":
                graphics.DrawLine(pen, 12, Height - 10, Width - 11, 10);
                graphics.FillEllipse(brush, Width - 13, 8, 5, 5);
                break;
            case "▦":
                DrawMosaicIcon(graphics, pen);
                break;
            case "A":
                using (var font = new Font("Segoe UI", 15, FontStyle.Regular))
                using (var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                {
                    graphics.DrawString("A", font, brush, ClientRectangle, format);
                }

                break;
            case "↶":
                DrawUndoIcon(graphics, pen);
                break;
            case "↓":
                DrawSaveIcon(graphics, pen);
                break;
            case "×":
                graphics.DrawLine(pen, 13, 11, Width - 13, Height - 11);
                graphics.DrawLine(pen, Width - 13, 11, 13, Height - 11);
                break;
            case "✓":
                graphics.DrawLines(pen, new[] { new Point(10, 18), new Point(16, 24), new Point(28, 10) });
                break;
        }
    }

    private static void DrawMosaicIcon(Graphics graphics, Pen pen)
    {
        var size = 4;
        for (var y = 9; y <= 21; y += 6)
        {
            for (var x = 11; x <= 23; x += 6)
            {
                graphics.DrawRectangle(pen, x, y, size, size);
            }
        }
    }

    private static void DrawUndoIcon(Graphics graphics, Pen pen)
    {
        pen.CustomStartCap = new System.Drawing.Drawing2D.AdjustableArrowCap(4, 5);
        graphics.DrawArc(pen, 11, 10, 18, 16, 205, 245);
    }

    private static void DrawSaveIcon(Graphics graphics, Pen pen)
    {
        using var arrowPen = (Pen)pen.Clone();
        arrowPen.CustomEndCap = new System.Drawing.Drawing2D.AdjustableArrowCap(4, 5);
        graphics.DrawLine(arrowPen, 19, 8, 19, 22);
        graphics.DrawLine(pen, 12, 25, 26, 25);
    }
}
