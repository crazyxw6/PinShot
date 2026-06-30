namespace PinShot;

internal sealed class FloatingToolbarPanel : FlowLayoutPanel
{
    public FloatingToolbarPanel()
    {
        DoubleBuffered = true;
        BackColor = Color.FromArgb(16, 16, 18);
        FlowDirection = FlowDirection.LeftToRight;
        WrapContents = false;
        Padding = new Padding(8, 6, 8, 6);
        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw |
            ControlStyles.UserPaint,
            true);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        using var background = new SolidBrush(Color.FromArgb(232, 16, 16, 18));
        using var border = new Pen(Color.FromArgb(155, 255, 255, 255), 1);
        var bounds = new RectangleF(0.5f, 0.5f, Width - 1, Height - 1);

        e.Graphics.FillRoundedRectangle(background, bounds, 8);
        e.Graphics.DrawRoundedRectangle(border, bounds, 8);
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
    }

    protected override void OnResize(EventArgs eventargs)
    {
        base.OnResize(eventargs);

        using var path = GraphicsExtensions.CreateRoundedRectanglePath(
            new RectangleF(0, 0, Width, Height),
            8);
        Region = new Region(path);
    }
}
