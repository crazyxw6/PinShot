namespace PinShot;

internal sealed class PinForm : Form
{
    private readonly Bitmap image;
    private readonly PictureBox pictureBox;
    private readonly double aspectRatio;
    private Point dragStart;

    public PinForm(Bitmap image)
    {
        this.image = image;
        aspectRatio = (double)image.Width / image.Height;

        Text = "PinShot 贴图";
        TopMost = true;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.None;
        BackColor = Color.Black;
        MinimumSize = new Size(80, 60);
        ClientSize = GetInitialSize(image.Size);

        pictureBox = new PictureBox
        {
            Dock = DockStyle.Fill,
            Image = image,
            SizeMode = PictureBoxSizeMode.StretchImage
        };

        pictureBox.MouseDown += OnDragMouseDown;
        pictureBox.MouseMove += OnDragMouseMove;
        pictureBox.MouseWheel += OnMouseWheelResize;
        pictureBox.Paint += DrawPinnedBorder;
        pictureBox.DoubleClick += (_, _) => Close();
        pictureBox.ContextMenuStrip = BuildMenu();

        Controls.Add(pictureBox);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            pictureBox.Image = null;
            image.Dispose();
            pictureBox.Dispose();
        }

        base.Dispose(disposing);
    }

    private ContextMenuStrip BuildMenu()
    {
        var menu = new ContextMenuStrip();
        menu.Items.Add("复制", null, (_, _) => Clipboard.SetImage(new Bitmap(image)));
        menu.Items.Add("保存为...", null, (_, _) => SaveImage());
        menu.Items.Add(new ToolStripSeparator());
        var topMostItem = new ToolStripMenuItem(GetTopMostMenuText());
        topMostItem.Click += (_, _) =>
        {
            TopMost = !TopMost;
            topMostItem.Text = GetTopMostMenuText();
        };
        menu.Items.Add(topMostItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("不透明度 100%", null, (_, _) => Opacity = 1.0);
        menu.Items.Add("不透明度 80%", null, (_, _) => Opacity = 0.8);
        menu.Items.Add("不透明度 60%", null, (_, _) => Opacity = 0.6);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("关闭", null, (_, _) => Close());
        return menu;
    }

    private string GetTopMostMenuText()
    {
        return TopMost ? "取消置顶" : "置顶";
    }

    private void SaveImage()
    {
        using var dialog = new SaveFileDialog
        {
            Filter = "PNG 图片|*.png",
            FileName = $"PinShot-{DateTime.Now:yyyyMMdd-HHmmss}.png"
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            image.Save(dialog.FileName, System.Drawing.Imaging.ImageFormat.Png);
        }
    }

    private void OnDragMouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            dragStart = e.Location;
        }
    }

    private void OnDragMouseMove(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left)
        {
            return;
        }

        Left += e.X - dragStart.X;
        Top += e.Y - dragStart.Y;
    }

    private void OnMouseWheelResize(object? sender, MouseEventArgs e)
    {
        var factor = e.Delta > 0 ? 1.08 : 0.92;
        var nextWidth = Math.Clamp((int)(ClientSize.Width * factor), 80, 3000);
        var nextHeight = Math.Clamp((int)(nextWidth / aspectRatio), 60, 2200);

        if (nextHeight is 60 or 2200)
        {
            nextWidth = Math.Clamp((int)(nextHeight * aspectRatio), 80, 3000);
        }

        ClientSize = new Size(nextWidth, nextHeight);
    }

    private static Size GetInitialSize(Size imageSize)
    {
        return new Size(
            Math.Max(80, imageSize.Width),
            Math.Max(60, imageSize.Height));
    }

    private static void DrawPinnedBorder(object? sender, PaintEventArgs e)
    {
        if (sender is not PictureBox box || box.Width <= 1 || box.Height <= 1)
        {
            return;
        }

        using var border = new Pen(Color.FromArgb(72, 182, 255), 1);
        e.Graphics.DrawRectangle(border, 0, 0, box.Width - 1, box.Height - 1);
    }
}
