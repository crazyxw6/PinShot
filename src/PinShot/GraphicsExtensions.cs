namespace PinShot;

internal static class GraphicsExtensions
{
    public static void FillCrystalPanel(this Graphics graphics, RectangleF bounds, float radius)
    {
        using var path = CreateRoundedRectanglePath(bounds, radius);
        using var brush = new SolidBrush(Color.FromArgb(156, 12, 24, 38));
        graphics.FillPath(brush, path);
    }

    public static void DrawCrystalPanelBorder(this Graphics graphics, RectangleF bounds, float radius)
    {
        using var borderPen = new Pen(Color.FromArgb(190, 178, 230, 255), 1);
        graphics.DrawRoundedRectangle(borderPen, bounds, radius);
    }

    public static void DrawCrystalSelectionBorder(this Graphics graphics, Rectangle bounds)
    {
        if (bounds.Width <= 2 || bounds.Height <= 2)
        {
            return;
        }

        using var borderPen = new Pen(Color.FromArgb(72, 182, 255), 2);
        graphics.DrawRectangle(borderPen, bounds);
    }

    public static void DrawSelectionSizeBadge(this Graphics graphics, Rectangle bounds)
    {
        if (bounds.Width <= 2 || bounds.Height <= 2)
        {
            return;
        }

        var text = $"{bounds.Width} x {bounds.Height}";
        using var font = new Font("Segoe UI", 11, FontStyle.Regular);
        var textSize = graphics.MeasureString(text, font);
        var badge = new RectangleF(
            bounds.Left,
            Math.Max(2, bounds.Top - textSize.Height - 3),
            textSize.Width + 8,
            textSize.Height + 2);

        using var background = new SolidBrush(Color.FromArgb(232, 72, 182, 255));
        using var textBrush = new SolidBrush(Color.White);

        graphics.FillRectangle(background, badge);
        graphics.DrawString(text, font, textBrush, badge.X + 4, badge.Y);
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
