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
        BackColor = Color.FromArgb(0, 122, 255);
        MinimumSize = new Size(80, 60);
        Padding = new Padding(1);
        ClientSize = GetInitialSize(image.Size);

        pictureBox = new PictureBox
        {
            Dock = DockStyle.Fill,
            Image = image,
            SizeMode = PictureBoxSizeMode.Zoom
        };

        pictureBox.MouseDown += OnDragMouseDown;
        pictureBox.MouseMove += OnDragMouseMove;
        pictureBox.MouseWheel += OnMouseWheelResize;
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
        var nextWidth = Math.Clamp((int)(Width * factor), 80, 3000);
        var nextHeight = Math.Clamp((int)(nextWidth / aspectRatio), 60, 2200);

        if (nextHeight is 60 or 2200)
        {
            nextWidth = Math.Clamp((int)(nextHeight * aspectRatio), 80, 3000);
        }

        Size = new Size(nextWidth, nextHeight);
    }

    private static Size GetInitialSize(Size imageSize)
    {
        const int maxWidth = 900;
        const int maxHeight = 700;

        var scale = Math.Min(1.0, Math.Min((double)maxWidth / imageSize.Width, (double)maxHeight / imageSize.Height));
        return new Size(Math.Max(80, (int)(imageSize.Width * scale)), Math.Max(60, (int)(imageSize.Height * scale)));
    }
}
