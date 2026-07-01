namespace PinShot;

internal static class GraphicsExtensions
{
    public static void FillCrystalPanel(this Graphics graphics, RectangleF bounds, float radius)
    {
        // Keep the toolbar body visually transparent; buttons and borders carry the shape.
    }

    public static void DrawCrystalPanelBorder(this Graphics graphics, RectangleF bounds, float radius)
    {
        // Intentionally empty: the floating toolbar should have no visible container.
    }

    public static void DrawCrystalSelectionBorder(this Graphics graphics, Rectangle bounds)
    {
        if (bounds.Width <= 2 || bounds.Height <= 2)
        {
            return;
        }

        using var glowPen = new Pen(Color.FromArgb(74, 164, 226, 255), 5);
        using var edgeBrush = new LinearGradientBrush(
            bounds,
            Color.FromArgb(215, 255, 255, 255),
            Color.FromArgb(175, 54, 190, 255),
            LinearGradientMode.ForwardDiagonal);
        using var edgePen = new Pen(edgeBrush, 2);
        using var innerPen = new Pen(Color.FromArgb(105, 255, 255, 255), 1);

        graphics.DrawRectangle(glowPen, bounds);
        graphics.DrawRectangle(edgePen, bounds);

        if (bounds.Width > 8 && bounds.Height > 8)
        {
            graphics.DrawRectangle(innerPen, Rectangle.Inflate(bounds, -3, -3));
        }
    }

    public static void FillRoundedRectangle(this Graphics graphics, Brush brush, RectangleF bounds, float radius)
    {
        using var path = CreateRoundedRectanglePath(bounds, radius);
        graphics.FillPath(brush, path);
    }

    public static void DrawRoundedRectangle(this Graphics graphics, Pen pen, RectangleF bounds, float radius)
    {
        using var path = CreateRoundedRectanglePath(bounds, radius);
        graphics.DrawPath(pen, path);
    }

    public static System.Drawing.Drawing2D.GraphicsPath CreateRoundedRectanglePath(RectangleF bounds, float radius)
    {
        var path = new System.Drawing.Drawing2D.GraphicsPath();
        var diameter = radius * 2;

        path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
        path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
        path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();

        return path;
    }
}
