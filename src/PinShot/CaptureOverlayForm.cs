using System.Drawing.Imaging;

namespace PinShot;

internal sealed class CaptureOverlayForm : Form
{
    private readonly Bitmap desktopImage;
    private readonly Rectangle virtualScreen;
    private Point startPoint;
    private Point currentPoint;
    private bool selecting;

    private CaptureOverlayForm(Bitmap desktopImage, Rectangle virtualScreen)
    {
        this.desktopImage = desktopImage;
        this.virtualScreen = virtualScreen;

        DoubleBuffered = true;
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.Manual;
        Bounds = virtualScreen;
        TopMost = true;
        ShowInTaskbar = false;
        Cursor = Cursors.Cross;
        KeyPreview = true;
    }

    public CaptureResult? Result { get; private set; }

    public static CaptureResult? SelectArea(Bitmap desktopImage, Rectangle virtualScreen)
    {
        using var form = new CaptureOverlayForm(desktopImage, virtualScreen);
        return form.ShowDialog() == DialogResult.OK ? form.Result : null;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        e.Graphics.DrawImageUnscaled(desktopImage, 0, 0);

        using var overlay = new SolidBrush(Color.FromArgb(120, Color.Black));
        e.Graphics.FillRectangle(overlay, ClientRectangle);

        var selection = GetSelectionRectangle();
        if (selection.Width <= 0 || selection.Height <= 0)
        {
            DrawHint(e.Graphics);
            return;
        }

        e.Graphics.SetClip(selection);
        e.Graphics.DrawImageUnscaled(desktopImage, 0, 0);
        e.Graphics.ResetClip();

        e.Graphics.DrawCrystalSelectionBorder(selection);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left)
        {
            return;
        }

        selecting = true;
        startPoint = e.Location;
        currentPoint = e.Location;
        Invalidate();
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (!selecting)
        {
            return;
        }

        currentPoint = e.Location;
        Invalidate();
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        if (!selecting || e.Button != MouseButtons.Left)
        {
            return;
        }

        selecting = false;
        currentPoint = e.Location;
        var selection = GetSelectionRectangle();

        if (selection.Width < 4 || selection.Height < 4)
        {
            Invalidate();
            return;
        }

        Result = new CaptureResult(desktopImage.Clone(selection, PixelFormat.Format32bppArgb), selection);
        DialogResult = DialogResult.OK;
        Close();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        base.OnKeyDown(e);
    }

    private Rectangle GetSelectionRectangle()
    {
        var left = Math.Min(startPoint.X, currentPoint.X);
        var top = Math.Min(startPoint.Y, currentPoint.Y);
        var right = Math.Max(startPoint.X, currentPoint.X);
        var bottom = Math.Max(startPoint.Y, currentPoint.Y);

        return Rectangle.Intersect(
            new Rectangle(left, top, right - left, bottom - top),
            new Rectangle(Point.Empty, virtualScreen.Size));
    }

    private static void DrawHint(Graphics graphics)
    {
        const string hint = "拖动选择截图区域，按 Esc 取消";
        using var font = new Font("Microsoft YaHei UI", 14, FontStyle.Regular);
        using var textBrush = new SolidBrush(Color.White);
        using var backgroundBrush = new SolidBrush(Color.FromArgb(150, Color.Black));

        var size = graphics.MeasureString(hint, font);
        var box = new RectangleF(24, 24, size.Width + 28, size.Height + 18);
        graphics.FillRoundedRectangle(backgroundBrush, box, 8);
        graphics.DrawString(hint, font, textBrush, box.X + 14, box.Y + 9);
    }
}
