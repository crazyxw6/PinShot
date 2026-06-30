namespace PinShot;

internal sealed class ToolbarSeparator : Control
{
    public ToolbarSeparator()
    {
        Size = new Size(12, 34);
        Margin = new Padding(4, 0, 4, 0);
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        using var pen = new Pen(Color.FromArgb(110, 255, 255, 255), 1);
        var x = Width / 2;
        e.Graphics.DrawLine(pen, x, 7, x, Height - 7);
    }
}
