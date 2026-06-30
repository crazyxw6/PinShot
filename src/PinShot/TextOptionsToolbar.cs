namespace PinShot;

internal sealed class TextOptionsToolbar : Control
{
    private const int ButtonSize = 30;
    private const int Gap = 5;
    private const int PaddingSize = 7;
    private readonly ToolTip toolTip = new();
    private readonly TextOptions options;
    private readonly List<OptionItem> items;
    private int hoveredIndex = -1;
    private int pressedIndex = -1;

    public TextOptionsToolbar(TextOptions options)
    {
        this.options = options;
        items =
        [
            OptionItem.ForCommand(TextOptionCommand.Bold, "粗体"),
            OptionItem.ForCommand(TextOptionCommand.Italic, "斜体"),
            OptionItem.ForCommand(TextOptionCommand.Outline, "描边"),
            OptionItem.Separator(),
            OptionItem.Size(16, "小字"),
            OptionItem.Size(20, "中字"),
            OptionItem.Size(28, "大字"),
            OptionItem.Separator(),
            OptionItem.ForColor(Color.Red, "红色"),
            OptionItem.ForColor(Color.Yellow, "黄色"),
            OptionItem.ForColor(Color.Lime, "绿色"),
            OptionItem.ForColor(Color.FromArgb(0, 122, 255), "蓝色"),
            OptionItem.ForColor(Color.White, "白色"),
            OptionItem.ForColor(Color.Black, "黑色"),
            OptionItem.ForColor(Color.Magenta, "粉色")
        ];

        Size = GetPreferredSize(Size.Empty);
        BackColor = Color.FromArgb(18, 18, 20);
        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw |
            ControlStyles.UserPaint,
            true);
    }

    public event Action? OptionsChanged;

    public override Size GetPreferredSize(Size proposedSize)
    {
        var width = PaddingSize * 2;
        foreach (var item in items)
        {
            width += item.IsSeparator ? 10 : ButtonSize;
            width += Gap;
        }

        width -= Gap;
        return new Size(width, ButtonSize + PaddingSize * 2);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        e.Graphics.Clear(BackColor);

        using var panelBrush = new SolidBrush(Color.FromArgb(226, 18, 18, 20));
        using var borderPen = new Pen(Color.FromArgb(120, 255, 255, 255), 1);
        var panelRect = new RectangleF(0.5f, 0.5f, Width - 1, Height - 1);
        e.Graphics.FillRoundedRectangle(panelBrush, panelRect, 8);
        e.Graphics.DrawRoundedRectangle(borderPen, panelRect, 8);

        for (var i = 0; i < items.Count; i++)
        {
            var bounds = GetItemBounds(i);
            var item = items[i];
            if (item.IsSeparator)
            {
                using var pen = new Pen(Color.FromArgb(120, 255, 255, 255), 1);
                var x = bounds.Left + bounds.Width / 2;
                e.Graphics.DrawLine(pen, x, bounds.Top + 7, x, bounds.Bottom - 7);
                continue;
            }

            DrawItem(e.Graphics, item, bounds, i);
        }
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);

        using var path = GraphicsExtensions.CreateRoundedRectanglePath(new RectangleF(0, 0, Width, Height), 8);
        Region = new Region(path);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        var hit = HitTest(e.Location);
        if (hit != hoveredIndex)
        {
            hoveredIndex = hit;
            Cursor = hit >= 0 ? Cursors.Hand : Cursors.Default;
            toolTip.SetToolTip(this, hit >= 0 ? items[hit].Tooltip : string.Empty);
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
            Apply(items[hit]);
        }

        base.OnMouseUp(e);
    }

    private void DrawItem(Graphics graphics, OptionItem item, Rectangle bounds, int index)
    {
        var selected = IsSelected(item);
        var fill = selected
            ? Color.FromArgb(70, 60, 150, 255)
            : index == pressedIndex
                ? Color.FromArgb(54, 255, 255, 255)
                : index == hoveredIndex
                    ? Color.FromArgb(34, 255, 255, 255)
                    : Color.FromArgb(8, 255, 255, 255);

        using var fillBrush = new SolidBrush(fill);
        graphics.FillRoundedRectangle(fillBrush, new RectangleF(bounds.X, bounds.Y, bounds.Width, bounds.Height), 5);

        if (item.Color is not null)
        {
            using var colorBrush = new SolidBrush(item.Color.Value);
            using var borderPen = new Pen(Color.FromArgb(210, 255, 255, 255), 1);
            var swatch = new Rectangle(bounds.Left + 8, bounds.Top + 8, bounds.Width - 16, bounds.Height - 16);
            graphics.FillRectangle(colorBrush, swatch);
            graphics.DrawRectangle(borderPen, swatch);
            return;
        }

        using var textBrush = new SolidBrush(Color.White);
        using var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        var label = item.Command switch
        {
            TextOptionCommand.Bold => "B",
            TextOptionCommand.Italic => "I",
            TextOptionCommand.Outline => "A",
            _ => item.FontSize switch
            {
                16 => ".",
                20 => "●",
                28 => "●",
                _ => string.Empty
            }
        };

        var style = item.Command switch
        {
            TextOptionCommand.Bold => FontStyle.Bold,
            TextOptionCommand.Italic => FontStyle.Italic,
            _ => FontStyle.Regular
        };
        var size = item.FontSize == 28 ? 18 : item.FontSize == 16 ? 10 : 13;

        using var font = new Font("Segoe UI", size, style);
        graphics.DrawString(label, font, textBrush, bounds, format);

        if (item.Command == TextOptionCommand.Outline)
        {
            using var pen = new Pen(Color.White, 1);
            graphics.DrawLine(pen, bounds.Left + 9, bounds.Bottom - 8, bounds.Right - 9, bounds.Bottom - 8);
        }
    }

    private Rectangle GetItemBounds(int index)
    {
        var x = PaddingSize;
        for (var i = 0; i < index; i++)
        {
            x += items[i].IsSeparator ? 10 : ButtonSize;
            x += Gap;
        }

        var width = items[index].IsSeparator ? 10 : ButtonSize;
        return new Rectangle(x, PaddingSize, width, ButtonSize);
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

    private bool IsSelected(OptionItem item)
    {
        if (item.Color is not null)
        {
            return item.Color.Value.ToArgb() == options.Color.ToArgb();
        }

        if (item.FontSize is not null)
        {
            return item.FontSize.Value == options.FontSize;
        }

        return item.Command switch
        {
            TextOptionCommand.Bold => options.Bold,
            TextOptionCommand.Italic => options.Italic,
            TextOptionCommand.Outline => options.Outline,
            _ => false
        };
    }

    private void Apply(OptionItem item)
    {
        if (item.Color is not null)
        {
            options.Color = item.Color.Value;
        }
        else if (item.FontSize is not null)
        {
            options.FontSize = item.FontSize.Value;
        }
        else if (item.Command == TextOptionCommand.Bold)
        {
            options.Bold = !options.Bold;
        }
        else if (item.Command == TextOptionCommand.Italic)
        {
            options.Italic = !options.Italic;
        }
        else if (item.Command == TextOptionCommand.Outline)
        {
            options.Outline = !options.Outline;
        }

        OptionsChanged?.Invoke();
        Invalidate();
    }

    private enum TextOptionCommand
    {
        None,
        Bold,
        Italic,
        Outline
    }

    private sealed class OptionItem
    {
        private OptionItem(TextOptionCommand command, int? fontSize, Color? color, string tooltip, bool isSeparator)
        {
            Command = command;
            FontSize = fontSize;
            Color = color;
            Tooltip = tooltip;
            IsSeparator = isSeparator;
        }

        public TextOptionCommand Command { get; }
        public int? FontSize { get; }
        public Color? Color { get; }
        public string Tooltip { get; }
        public bool IsSeparator { get; }

        public static OptionItem ForCommand(TextOptionCommand command, string tooltip) => new(command, null, null, tooltip, false);
        public static OptionItem Size(int size, string tooltip) => new(TextOptionCommand.None, size, null, tooltip, false);
        public static OptionItem ForColor(Color color, string tooltip) => new(TextOptionCommand.None, null, color, tooltip, false);
        public static OptionItem Separator() => new(TextOptionCommand.None, null, null, string.Empty, true);
    }
}
