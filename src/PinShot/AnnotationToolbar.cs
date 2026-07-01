namespace PinShot;

internal sealed class AnnotationToolbar : Control
{
    private const int ButtonSize = 34;
    private const int ButtonGap = 6;
    private const int SeparatorWidth = 12;
    private const int ToolbarPadding = 8;

    private readonly ToolTip toolTip = new();
    private readonly List<ToolbarItem> items;
    private int hoveredIndex = -1;
    private int pressedIndex = -1;
    private AnnotationTool? currentTool;

    public AnnotationToolbar()
    {
        items =
        [
            ToolbarItem.ForTool(AnnotationTool.Rectangle, "矩形"),
            ToolbarItem.ForTool(AnnotationTool.Ellipse, "圆形"),
            ToolbarItem.ForTool(AnnotationTool.Arrow, "箭头"),
            ToolbarItem.ForTool(AnnotationTool.Pen, "画笔"),
            ToolbarItem.ForTool(AnnotationTool.Mosaic, "马赛克"),
            ToolbarItem.ForTool(AnnotationTool.Text, "文字"),
            ToolbarItem.Action(ToolbarCommand.Pin, "固定到屏幕"),
            ToolbarItem.Separator(),
            ToolbarItem.Action(ToolbarCommand.Undo, "撤销"),
            ToolbarItem.Action(ToolbarCommand.Save, "保存"),
            ToolbarItem.Action(ToolbarCommand.Cancel, "取消"),
            ToolbarItem.Action(ToolbarCommand.Confirm, "确认并贴图")
        ];

        Size = GetPreferredSize(Size.Empty);
        Cursor = Cursors.Default;
        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw |
            ControlStyles.SupportsTransparentBackColor |
            ControlStyles.UserPaint,
            true);
        BackColor = Color.Transparent;
    }

    public event Action<AnnotationTool>? ToolChanged;
    public event Action? UndoRequested;
    public event Action? SaveRequested;
    public event Action? CancelRequested;
    public event Action? PinRequested;
    public event Action? ConfirmRequested;

    public AnnotationTool? CurrentTool
    {
        get => currentTool;
        set
        {
            currentTool = value;
            Invalidate();
        }
    }

    public override Size GetPreferredSize(Size proposedSize)
    {
        var width = ToolbarPadding * 2;

        foreach (var item in items)
        {
            width += item.IsSeparator ? SeparatorWidth : ButtonSize;
            width += ButtonGap;
        }

        width -= ButtonGap;
        return new Size(width, ButtonSize + ToolbarPadding * 2);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        var panelRect = new RectangleF(0.5f, 0.5f, Width - 1, Height - 1);
        e.Graphics.FillCrystalPanel(panelRect, 9);
        e.Graphics.DrawCrystalPanelBorder(panelRect, 9);

        for (var i = 0; i < items.Count; i++)
        {
            var item = items[i];
            var bounds = GetItemBounds(i);

            if (item.IsSeparator)
            {
                DrawSeparator(e.Graphics, bounds);
                continue;
            }

            DrawButton(e.Graphics, item, bounds, i);
        }
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);

        using var path = GraphicsExtensions.CreateRoundedRectanglePath(
            new RectangleF(0, 0, Width, Height),
            8);
        Region = new Region(path);
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        var nextHovered = HitTest(e.Location);
        if (hoveredIndex != nextHovered)
        {
            hoveredIndex = nextHovered;
            Cursor = hoveredIndex >= 0 ? Cursors.Hand : Cursors.Default;
            toolTip.SetToolTip(this, hoveredIndex >= 0 ? items[hoveredIndex].Tooltip : string.Empty);
            Invalidate();
        }

        base.OnMouseMove(e);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        hoveredIndex = -1;
        pressedIndex = -1;
        Cursor = Cursors.Default;
        Invalidate();
        base.OnMouseLeave(e);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            pressedIndex = HitTest(e.Location);
            Invalidate();
        }

        base.OnMouseDown(e);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        var hit = HitTest(e.Location);
        var shouldInvoke = e.Button == MouseButtons.Left && hit >= 0 && hit == pressedIndex;
        pressedIndex = -1;
        Invalidate();

        if (shouldInvoke)
        {
            InvokeItem(items[hit]);
        }

        base.OnMouseUp(e);
    }

    private void DrawButton(Graphics graphics, ToolbarItem item, Rectangle bounds, int index)
    {
        var selected = item.Tool is not null && item.Tool == currentTool;
        var pressed = index == pressedIndex;
        var hovered = index == hoveredIndex;

        var fill = selected
            ? Color.FromArgb(34, 43, 171, 255)
            : pressed
                ? Color.FromArgb(24, 255, 255, 255)
                : hovered
                    ? Color.FromArgb(14, 255, 255, 255)
                    : Color.Transparent;

        using var fillBrush = new SolidBrush(fill);
        graphics.FillRoundedRectangle(fillBrush, new RectangleF(bounds.X, bounds.Y, bounds.Width, bounds.Height), 5);

        var iconColor = selected ? Color.White : Color.FromArgb(238, 255, 255, 255);
        using var pen = new Pen(iconColor, 2)
        {
            StartCap = System.Drawing.Drawing2D.LineCap.Round,
            EndCap = System.Drawing.Drawing2D.LineCap.Round,
            LineJoin = System.Drawing.Drawing2D.LineJoin.Round
        };
        using var brush = new SolidBrush(iconColor);

        DrawIcon(graphics, item, bounds, pen, brush);
    }

    private static void DrawSeparator(Graphics graphics, Rectangle bounds)
    {
    }

    private static void DrawIcon(Graphics graphics, ToolbarItem item, Rectangle bounds, Pen pen, Brush brush)
    {
        var left = bounds.Left;
        var top = bounds.Top;
        var right = bounds.Right;
        var bottom = bounds.Bottom;

        if (item.Tool == AnnotationTool.Rectangle)
        {
            graphics.DrawRectangle(pen, left + 9, top + 8, 16, 16);
        }
        else if (item.Tool == AnnotationTool.Ellipse)
        {
            graphics.DrawEllipse(pen, left + 8, top + 8, 17, 17);
        }
        else if (item.Tool == AnnotationTool.Arrow)
        {
            using var arrowPen = (Pen)pen.Clone();
            arrowPen.CustomEndCap = new System.Drawing.Drawing2D.AdjustableArrowCap(4, 5);
            graphics.DrawLine(arrowPen, left + 10, bottom - 10, right - 10, top + 9);
        }
        else if (item.Tool == AnnotationTool.Pen)
        {
            graphics.DrawLine(pen, left + 10, bottom - 10, right - 10, top + 10);
            graphics.FillEllipse(brush, right - 13, top + 8, 5, 5);
        }
        else if (item.Tool == AnnotationTool.Mosaic)
        {
            DrawMosaicIcon(graphics, pen, bounds);
        }
        else if (item.Tool == AnnotationTool.Text)
        {
            using var font = new Font("Segoe UI", 15, FontStyle.Regular);
            using var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            graphics.DrawString("A", font, brush, bounds, format);
        }
        else if (item.Command == ToolbarCommand.Undo)
        {
            using var undoPen = (Pen)pen.Clone();
            undoPen.CustomStartCap = new System.Drawing.Drawing2D.AdjustableArrowCap(4, 5);
            graphics.DrawArc(undoPen, left + 9, top + 9, 18, 16, 205, 245);
        }
        else if (item.Command == ToolbarCommand.Pin)
        {
            DrawPinIcon(graphics, pen, brush, bounds);
        }
        else if (item.Command == ToolbarCommand.Save)
        {
            using var arrowPen = (Pen)pen.Clone();
            arrowPen.CustomEndCap = new System.Drawing.Drawing2D.AdjustableArrowCap(4, 5);
            graphics.DrawLine(arrowPen, left + 17, top + 8, left + 17, bottom - 12);
            graphics.DrawLine(pen, left + 10, bottom - 8, right - 10, bottom - 8);
        }
        else if (item.Command == ToolbarCommand.Cancel)
        {
            graphics.DrawLine(pen, left + 11, top + 11, right - 11, bottom - 11);
            graphics.DrawLine(pen, right - 11, top + 11, left + 11, bottom - 11);
        }
        else if (item.Command == ToolbarCommand.Confirm)
        {
            graphics.DrawLines(pen, new[] { new Point(left + 9, top + 18), new Point(left + 15, top + 24), new Point(right - 8, top + 10) });
        }
    }

    private static void DrawMosaicIcon(Graphics graphics, Pen pen, Rectangle bounds)
    {
        const int size = 4;
        for (var y = bounds.Top + 8; y <= bounds.Top + 20; y += 6)
        {
            for (var x = bounds.Left + 10; x <= bounds.Left + 22; x += 6)
            {
                graphics.DrawRectangle(pen, x, y, size, size);
            }
        }
    }

    private static void DrawPinIcon(Graphics graphics, Pen pen, Brush brush, Rectangle bounds)
    {
        using var pinPen = (Pen)pen.Clone();
        pinPen.Width = 2.2f;

        var head = new[]
        {
            new Point(bounds.Left + 14, bounds.Top + 8),
            new Point(bounds.Left + 24, bounds.Top + 18),
            new Point(bounds.Left + 20, bounds.Top + 22),
            new Point(bounds.Left + 10, bounds.Top + 12)
        };

        graphics.DrawPolygon(pinPen, head);
        graphics.DrawLine(pinPen, bounds.Left + 18, bounds.Top + 20, bounds.Left + 11, bounds.Bottom - 8);
        graphics.DrawLine(pinPen, bounds.Left + 16, bounds.Top + 23, bounds.Left + 22, bounds.Bottom - 7);
    }

    private Rectangle GetItemBounds(int index)
    {
        var x = ToolbarPadding;

        for (var i = 0; i < index; i++)
        {
            x += items[i].IsSeparator ? SeparatorWidth : ButtonSize;
            x += ButtonGap;
        }

        var item = items[index];
        var width = item.IsSeparator ? SeparatorWidth : ButtonSize;
        return new Rectangle(x, ToolbarPadding, width, ButtonSize);
    }

    private int HitTest(Point point)
    {
        for (var i = 0; i < items.Count; i++)
        {
            if (!items[i].IsSeparator && GetItemBounds(i).Contains(point))
            {
                return i;
            }
        }

        return -1;
    }

    private void InvokeItem(ToolbarItem item)
    {
        if (item.Tool is not null)
        {
            CurrentTool = item.Tool.Value;
            ToolChanged?.Invoke(item.Tool.Value);
            return;
        }

        switch (item.Command)
        {
            case ToolbarCommand.Pin:
                PinRequested?.Invoke();
                break;
            case ToolbarCommand.Undo:
                UndoRequested?.Invoke();
                break;
            case ToolbarCommand.Save:
                SaveRequested?.Invoke();
                break;
            case ToolbarCommand.Cancel:
                CancelRequested?.Invoke();
                break;
            case ToolbarCommand.Confirm:
                ConfirmRequested?.Invoke();
                break;
        }
    }

    private enum ToolbarCommand
    {
        None,
        Pin,
        Undo,
        Save,
        Cancel,
        Confirm
    }

    private sealed class ToolbarItem
    {
        private ToolbarItem(AnnotationTool? tool, ToolbarCommand command, string tooltip, bool isSeparator)
        {
            Tool = tool;
            Command = command;
            Tooltip = tooltip;
            IsSeparator = isSeparator;
        }

        public AnnotationTool? Tool { get; }
        public ToolbarCommand Command { get; }
        public string Tooltip { get; }
        public bool IsSeparator { get; }

        public static ToolbarItem ForTool(AnnotationTool tool, string tooltip) => new(tool, ToolbarCommand.None, tooltip, false);
        public static ToolbarItem Action(ToolbarCommand command, string tooltip) => new(null, command, tooltip, false);
        public static ToolbarItem Separator() => new(null, ToolbarCommand.None, string.Empty, true);
    }
}
