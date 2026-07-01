namespace PinShot;

internal sealed class AnnotationEditorForm : Form
{
    private const int ResizeGripSize = 7;
    private const int MinimumSelectionSize = 24;
    private const int MaxUndoSteps = 20;

    private readonly Bitmap desktopImage;
    private readonly Rectangle virtualScreen;
    private Bitmap canvas;
    private Rectangle imageBounds;
    private readonly AnnotationToolbar toolbar;
    private readonly TextOptions textOptions = new();
    private readonly TextOptionsToolbar textOptionsToolbar;
    private readonly Stack<Bitmap> undoStack = new();
    private readonly System.Windows.Forms.Timer caretTimer = new() { Interval = 520 };
    private string activeText = string.Empty;
    private Point activeTextImageLocation;
    private AnnotationTool? currentTool;
    private Point startPoint;
    private Point currentPoint;
    private Point moveStartPoint;
    private Rectangle moveStartBounds;
    private Rectangle toolbarDragBounds;
    private ResizeDirection resizeDirection;
    private bool drawing;
    private bool movingSelection;
    private bool resizingSelection;
    private bool hasAnnotations;
    private bool editingText;
    private bool showTextCaret = true;

    public AnnotationEditorForm(Bitmap desktopImage, Rectangle virtualScreen, Bitmap source, Rectangle imageBounds)
    {
        this.desktopImage = desktopImage;
        this.virtualScreen = virtualScreen;
        this.imageBounds = imageBounds;
        canvas = new Bitmap(source);

        Text = "PinShot 编辑";
        TopMost = true;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.Manual;
        FormBorderStyle = FormBorderStyle.None;
        Bounds = virtualScreen;
        BackColor = Color.Black;
        DoubleBuffered = true;
        KeyPreview = true;
        Cursor = Cursors.Cross;

        toolbar = new AnnotationToolbar();
        toolbar.ToolChanged += SetCurrentTool;
        toolbar.UndoRequested += Undo;
        toolbar.SaveRequested += SaveImage;
        toolbar.CancelRequested += Cancel;
        toolbar.PinRequested += Confirm;
        toolbar.ConfirmRequested += Confirm;

        textOptionsToolbar = new TextOptionsToolbar(textOptions)
        {
            Visible = false
        };
        textOptionsToolbar.OptionsChanged += ApplyTextOptionsToActiveTextBox;

        Controls.Add(toolbar);
        Controls.Add(textOptionsToolbar);
        PositionToolbar();

        caretTimer.Tick += (_, _) =>
        {
            if (!editingText)
            {
                return;
            }

            showTextCaret = !showTextCaret;
            Invalidate(GetActiveTextBounds());
        };
    }

    public Bitmap? ResultImage { get; private set; }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        e.Graphics.DrawImageUnscaled(desktopImage, 0, 0);

        using var dim = new SolidBrush(Color.FromArgb(88, Color.Black));
        e.Graphics.FillRectangle(dim, ClientRectangle);

        if (hasAnnotations)
        {
            e.Graphics.DrawImageUnscaled(canvas, imageBounds.Location);
        }
        else
        {
            e.Graphics.SetClip(imageBounds);
            e.Graphics.DrawImageUnscaled(desktopImage, 0, 0);
            e.Graphics.ResetClip();
        }

        e.Graphics.DrawCrystalSelectionBorder(imageBounds);
        e.Graphics.DrawSelectionSizeBadge(imageBounds);

        DrawPreview(e.Graphics);
        DrawActiveText(e.Graphics);
        DrawToolbarDragPreview(e.Graphics);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (editingText)
        {
            CommitActiveTextBox();
        }

        if (e.Button != MouseButtons.Left)
        {
            return;
        }

        if (currentTool is null)
        {
            if (hasAnnotations)
            {
                return;
            }

            resizeDirection = HitTestResize(e.Location);
            if (resizeDirection != ResizeDirection.None)
            {
                resizingSelection = true;
                moveStartPoint = e.Location;
                moveStartBounds = imageBounds;
                Cursor = GetResizeCursor(resizeDirection);
                BeginSelectionMove();
                return;
            }

            if (imageBounds.Contains(e.Location))
            {
                movingSelection = true;
                moveStartPoint = e.Location;
                moveStartBounds = imageBounds;
                Cursor = Cursors.SizeAll;
                BeginSelectionMove();
            }

            return;
        }

        if (!imageBounds.Contains(e.Location))
        {
            return;
        }

        if (currentTool == AnnotationTool.Text)
        {
            BeginInlineTextInput(e.Location);
            return;
        }

        drawing = true;
        SyncCanvasFromDesktop();
        startPoint = ToImagePoint(e.Location);
        currentPoint = startPoint;
        PushUndo();
        Invalidate();
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (resizingSelection)
        {
            var previousBounds = imageBounds;
            var previousToolbarBounds = toolbarDragBounds;
            imageBounds = ResizeImageBounds(moveStartBounds, moveStartPoint, e.Location, resizeDirection);
            UpdateToolbarDragBounds();
            InvalidateSelectionChange(previousBounds, previousToolbarBounds);
            return;
        }

        if (movingSelection)
        {
            var previousBounds = imageBounds;
            var previousToolbarBounds = toolbarDragBounds;
            var offsetX = e.X - moveStartPoint.X;
            var offsetY = e.Y - moveStartPoint.Y;
            imageBounds = ClampImageBounds(new Rectangle(
                moveStartBounds.Left + offsetX,
                moveStartBounds.Top + offsetY,
                moveStartBounds.Width,
                moveStartBounds.Height));
            UpdateToolbarDragBounds();
            InvalidateSelectionChange(previousBounds, previousToolbarBounds);
            return;
        }

        if (!drawing)
        {
            if (currentTool is null && !hasAnnotations)
            {
                var direction = HitTestResize(e.Location);
                Cursor = direction != ResizeDirection.None
                    ? GetResizeCursor(direction)
                    : imageBounds.Contains(e.Location)
                        ? Cursors.SizeAll
                        : Cursors.Cross;
            }
            else
            {
                Cursor = Cursors.Cross;
            }

            return;
        }

        var point = ToImagePoint(ClampToImageBounds(e.Location));

        if (currentTool == AnnotationTool.Pen)
        {
            using var graphics = Graphics.FromImage(canvas);
            using var pen = new Pen(Color.Red, 2)
            {
                StartCap = System.Drawing.Drawing2D.LineCap.Round,
                EndCap = System.Drawing.Drawing2D.LineCap.Round
            };
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            graphics.DrawLine(pen, currentPoint, point);
            hasAnnotations = true;
        }

        currentPoint = point;
        Invalidate();
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        if (resizingSelection)
        {
            resizingSelection = false;
            resizeDirection = ResizeDirection.None;
            Cursor = Cursors.Cross;
            EndSelectionMove();
            return;
        }

        if (movingSelection)
        {
            movingSelection = false;
            Cursor = Cursors.Cross;
            EndSelectionMove();
            return;
        }

        if (!drawing)
        {
            return;
        }

        drawing = false;
        currentPoint = ToImagePoint(ClampToImageBounds(e.Location));

        if (currentTool != AnnotationTool.Pen)
        {
            ApplyCurrentTool();
        }

        Invalidate();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (editingText)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                CommitActiveTextBox();
                return;
            }

            if (e.KeyCode == Keys.Escape)
            {
                e.SuppressKeyPress = true;
                CancelActiveTextBox();
                return;
            }

            if (e.KeyCode == Keys.Back && activeText.Length > 0)
            {
                e.SuppressKeyPress = true;
                Invalidate(GetActiveTextBounds());
                activeText = activeText[..^1];
                Invalidate(GetActiveTextBounds());
                return;
            }
        }

        if (e.KeyCode == Keys.Escape)
        {
            Cancel();
        }

        if (e.Control && e.KeyCode == Keys.Z)
        {
            Undo();
        }

        base.OnKeyDown(e);
    }

    protected override void OnKeyPress(KeyPressEventArgs e)
    {
        if (editingText && !char.IsControl(e.KeyChar))
        {
            e.Handled = true;
            Invalidate(GetActiveTextBounds());
            activeText += e.KeyChar;
            showTextCaret = true;
            Invalidate(GetActiveTextBounds());
            return;
        }

        base.OnKeyPress(e);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            caretTimer.Dispose();
            textOptionsToolbar.Dispose();
            canvas.Dispose();

            foreach (var image in undoStack)
            {
                image.Dispose();
            }

        }

        base.Dispose(disposing);
    }

    private void SetCurrentTool(AnnotationTool tool)
    {
        currentTool = tool;
        toolbar.CurrentTool = tool;
        textOptionsToolbar.Visible = tool == AnnotationTool.Text;
        PositionToolbar();
    }

    private void PositionToolbar()
    {
        toolbar.Bounds = GetToolbarBounds(imageBounds);

        textOptionsToolbar.Left = toolbar.Left;
        textOptionsToolbar.Top = toolbar.Top + toolbar.Height + 8;

        if (textOptionsToolbar.Bottom > ClientSize.Height - 8)
        {
            textOptionsToolbar.Top = toolbar.Top - textOptionsToolbar.Height - 8;
        }
    }

    private Rectangle GetToolbarBounds(Rectangle bounds)
    {
        var left = bounds.Left + (bounds.Width - toolbar.Width) / 2;
        var top = bounds.Bottom + 12;

        if (top + toolbar.Height > ClientSize.Height - 8)
        {
            top = bounds.Top - toolbar.Height - 12;
        }

        left = Math.Clamp(left, 8, Math.Max(8, ClientSize.Width - toolbar.Width - 8));
        top = Math.Clamp(top, 8, Math.Max(8, ClientSize.Height - toolbar.Height - 8));

        return new Rectangle(left, top, toolbar.Width, toolbar.Height);
    }

    private void BeginSelectionMove()
    {
        toolbarDragBounds = toolbar.Bounds;
        toolbar.Visible = false;
    }

    private void UpdateToolbarDragBounds()
    {
        toolbarDragBounds = GetToolbarBounds(imageBounds);
    }

    private void EndSelectionMove()
    {
        var previousToolbarBounds = toolbarDragBounds;
        PositionToolbar();
        toolbar.Visible = true;
        toolbarDragBounds = Rectangle.Empty;
        Invalidate(InflateForRepaint(previousToolbarBounds));
        Invalidate(InflateForRepaint(toolbar.Bounds));
    }

    private void DrawToolbarDragPreview(Graphics graphics)
    {
        if (toolbarDragBounds.IsEmpty)
        {
            return;
        }

        toolbar.DrawDragPreview(graphics, toolbarDragBounds.Location);
    }

    private void DrawPreview(Graphics graphics)
    {
        if (!drawing || currentTool is null || currentTool == AnnotationTool.Pen)
        {
            return;
        }

        using var pen = new Pen(Color.Red, 2);
        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        var start = ToScreenPoint(startPoint);
        var end = ToScreenPoint(currentPoint);
        var rect = GetRectangle(start, end);

        if (currentTool == AnnotationTool.Rectangle || currentTool == AnnotationTool.Mosaic || currentTool == AnnotationTool.Text)
        {
            graphics.DrawRectangle(pen, rect);
        }
        else if (currentTool == AnnotationTool.Ellipse)
        {
            graphics.DrawEllipse(pen, rect);
        }
        else if (currentTool == AnnotationTool.Arrow)
        {
            DrawArrow(graphics, pen, start, end);
        }
    }

    private void ApplyCurrentTool()
    {
        using var graphics = Graphics.FromImage(canvas);
        using var pen = new Pen(Color.Red, 2);
        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        var rect = GetRectangle(startPoint, currentPoint);

        if (currentTool == AnnotationTool.Arrow)
        {
            if (Math.Abs(startPoint.X - currentPoint.X) < 3 && Math.Abs(startPoint.Y - currentPoint.Y) < 3)
            {
                return;
            }

            DrawArrow(graphics, pen, startPoint, currentPoint);
            hasAnnotations = true;
            return;
        }

        if (rect.Width < 3 || rect.Height < 3)
        {
            return;
        }

        switch (currentTool)
        {
            case AnnotationTool.Rectangle:
                graphics.DrawRectangle(pen, rect);
                hasAnnotations = true;
                break;
            case AnnotationTool.Ellipse:
                graphics.DrawEllipse(pen, rect);
                hasAnnotations = true;
                break;
            case AnnotationTool.Mosaic:
                ApplyMosaic(rect);
                hasAnnotations = true;
                break;
        }
    }

    private bool AddText(Point location, string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        using var graphics = Graphics.FromImage(canvas);
        using var font = CreateTextFont();
        using var brush = new SolidBrush(textOptions.Color);

        if (textOptions.Outline)
        {
            using var outlineBrush = new SolidBrush(Color.White);
            foreach (var offset in new[] { new Point(-1, 0), new Point(1, 0), new Point(0, -1), new Point(0, 1) })
            {
                graphics.DrawString(text, font, outlineBrush, location.X + offset.X, location.Y + offset.Y);
            }
        }

        graphics.DrawString(text, font, brush, location);
        return true;
    }

    private void BeginInlineTextInput(Point screenLocation)
    {
        activeTextImageLocation = ToImagePoint(screenLocation);
        activeText = string.Empty;
        editingText = true;
        showTextCaret = true;
        caretTimer.Start();
        Focus();
        Invalidate(GetActiveTextBounds());
    }

    private void ApplyTextOptionsToActiveTextBox()
    {
        if (!editingText)
        {
            return;
        }

        Invalidate(GetActiveTextBounds());
    }

    private void CommitActiveTextBox()
    {
        if (!editingText)
        {
            return;
        }

        var text = activeText;
        editingText = false;
        activeText = string.Empty;
        caretTimer.Stop();

        if (string.IsNullOrWhiteSpace(text))
        {
            Invalidate();
            return;
        }

        SyncCanvasFromDesktop();
        PushUndo();
        hasAnnotations = AddText(activeTextImageLocation, text) || hasAnnotations;
        Invalidate();
    }

    private Font CreateTextFont()
    {
        return new Font("Microsoft YaHei UI", textOptions.FontSize, textOptions.FontStyle);
    }

    private void CancelActiveTextBox()
    {
        if (!editingText)
        {
            return;
        }

        editingText = false;
        activeText = string.Empty;
        caretTimer.Stop();
        Invalidate();
    }

    private void DrawActiveText(Graphics graphics)
    {
        if (!editingText)
        {
            return;
        }

        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        var location = ToScreenPoint(activeTextImageLocation);
        var displayText = activeText.Length == 0 ? " " : activeText;
        using var font = CreateTextFont();
        DrawTextContent(graphics, displayText, location, font);

        if (!showTextCaret)
        {
            return;
        }

        var textWidth = MeasureTextWidth(graphics, activeText, font);
        using var caretPen = new Pen(textOptions.Color, 1);
        var x = location.X + textWidth + 2;
        graphics.DrawLine(caretPen, x, location.Y + 3, x, location.Y + font.Height + 2);
    }

    private void DrawTextContent(Graphics graphics, string text, Point location, Font font)
    {
        using var brush = new SolidBrush(textOptions.Color);

        if (textOptions.Outline)
        {
            using var outlineBrush = new SolidBrush(Color.White);
            foreach (var offset in new[] { new Point(-1, 0), new Point(1, 0), new Point(0, -1), new Point(0, 1) })
            {
                graphics.DrawString(text, font, outlineBrush, location.X + offset.X, location.Y + offset.Y);
            }
        }

        graphics.DrawString(text, font, brush, location);
    }

    private Rectangle GetActiveTextBounds()
    {
        using var graphics = CreateGraphics();
        using var font = CreateTextFont();
        var location = ToScreenPoint(activeTextImageLocation);
        var width = Math.Max(32, MeasureTextWidth(graphics, activeText.Length == 0 ? "字" : activeText, font) + 12);
        var height = font.Height + 12;
        return new Rectangle(location.X - 6, location.Y - 5, width, height);
    }

    private static int MeasureTextWidth(Graphics graphics, string text, Font font)
    {
        if (string.IsNullOrEmpty(text))
        {
            return 0;
        }

        return (int)Math.Ceiling(graphics.MeasureString(text, font).Width);
    }

    private void ApplyMosaic(Rectangle rect)
    {
        var safeRect = Rectangle.Intersect(rect, new Rectangle(Point.Empty, canvas.Size));
        if (safeRect.Width <= 2 || safeRect.Height <= 2)
        {
            return;
        }

        const int blockSize = 10;
        using var graphics = Graphics.FromImage(canvas);
        for (var y = safeRect.Top; y < safeRect.Bottom; y += blockSize)
        {
            for (var x = safeRect.Left; x < safeRect.Right; x += blockSize)
            {
                var block = Rectangle.Intersect(
                    new Rectangle(x, y, blockSize, blockSize),
                    safeRect);
                using var brush = new SolidBrush(AverageColor(canvas, block));

                graphics.FillRectangle(brush, block);
            }
        }
    }

    private static Color AverageColor(Bitmap image, Rectangle rect)
    {
        var r = 0;
        var g = 0;
        var b = 0;
        var count = 0;

        for (var y = rect.Top; y < rect.Bottom; y++)
        {
            for (var x = rect.Left; x < rect.Right; x++)
            {
                var color = image.GetPixel(x, y);
                r += color.R;
                g += color.G;
                b += color.B;
                count++;
            }
        }

        return Color.FromArgb(r / count, g / count, b / count);
    }

    private void PushUndo()
    {
        undoStack.Push(new Bitmap(canvas));
        while (undoStack.Count > MaxUndoSteps)
        {
            var items = undoStack.ToArray();
            undoStack.Clear();
            items[^1].Dispose();

            for (var i = items.Length - 2; i >= 0; i--)
            {
                undoStack.Push(items[i]);
            }
        }
    }

    private void Undo()
    {
        if (undoStack.Count == 0)
        {
            return;
        }

        using var latest = undoStack.Pop();
        using var graphics = Graphics.FromImage(canvas);
        graphics.DrawImageUnscaled(latest, 0, 0);
        hasAnnotations = undoStack.Count > 0;
        Invalidate();
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
            using var image = CreateCurrentImage();
            image.Save(dialog.FileName, System.Drawing.Imaging.ImageFormat.Png);
        }
    }

    private void Cancel()
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }

    private void Confirm()
    {
        ResultImage = CreateCurrentImage();
        DialogResult = DialogResult.OK;
        Close();
    }

    private Point ClampToImageBounds(Point point)
    {
        var x = Math.Clamp(point.X, imageBounds.Left, imageBounds.Right - 1);
        var y = Math.Clamp(point.Y, imageBounds.Top, imageBounds.Bottom - 1);
        return new Point(x, y);
    }

    private Rectangle ClampImageBounds(Rectangle bounds)
    {
        var left = Math.Clamp(bounds.Left, 0, ClientSize.Width - bounds.Width);
        var top = Math.Clamp(bounds.Top, 0, ClientSize.Height - bounds.Height);
        return new Rectangle(left, top, bounds.Width, bounds.Height);
    }

    private Rectangle ResizeImageBounds(Rectangle startBounds, Point startMouse, Point currentMouse, ResizeDirection direction)
    {
        var dx = currentMouse.X - startMouse.X;
        var dy = currentMouse.Y - startMouse.Y;
        var left = startBounds.Left;
        var top = startBounds.Top;
        var right = startBounds.Right;
        var bottom = startBounds.Bottom;

        if (direction.HasFlag(ResizeDirection.Left))
        {
            left = Math.Clamp(startBounds.Left + dx, 0, startBounds.Right - MinimumSelectionSize);
        }

        if (direction.HasFlag(ResizeDirection.Right))
        {
            right = Math.Clamp(startBounds.Right + dx, startBounds.Left + MinimumSelectionSize, ClientSize.Width);
        }

        if (direction.HasFlag(ResizeDirection.Top))
        {
            top = Math.Clamp(startBounds.Top + dy, 0, startBounds.Bottom - MinimumSelectionSize);
        }

        if (direction.HasFlag(ResizeDirection.Bottom))
        {
            bottom = Math.Clamp(startBounds.Bottom + dy, startBounds.Top + MinimumSelectionSize, ClientSize.Height);
        }

        return new Rectangle(left, top, right - left, bottom - top);
    }

    private ResizeDirection HitTestResize(Point point)
    {
        var hitArea = imageBounds;
        hitArea.Inflate(ResizeGripSize, ResizeGripSize);
        if (!hitArea.Contains(point))
        {
            return ResizeDirection.None;
        }

        var direction = ResizeDirection.None;
        if (Math.Abs(point.X - imageBounds.Left) <= ResizeGripSize)
        {
            direction |= ResizeDirection.Left;
        }
        else if (Math.Abs(point.X - imageBounds.Right) <= ResizeGripSize)
        {
            direction |= ResizeDirection.Right;
        }

        if (Math.Abs(point.Y - imageBounds.Top) <= ResizeGripSize)
        {
            direction |= ResizeDirection.Top;
        }
        else if (Math.Abs(point.Y - imageBounds.Bottom) <= ResizeGripSize)
        {
            direction |= ResizeDirection.Bottom;
        }

        return direction;
    }

    private static Cursor GetResizeCursor(ResizeDirection direction)
    {
        if (direction is ResizeDirection.Left or ResizeDirection.Right)
        {
            return Cursors.SizeWE;
        }

        if (direction is ResizeDirection.Top or ResizeDirection.Bottom)
        {
            return Cursors.SizeNS;
        }

        if (direction is (ResizeDirection.Left | ResizeDirection.Top) or (ResizeDirection.Right | ResizeDirection.Bottom))
        {
            return Cursors.SizeNWSE;
        }

        if (direction is (ResizeDirection.Right | ResizeDirection.Top) or (ResizeDirection.Left | ResizeDirection.Bottom))
        {
            return Cursors.SizeNESW;
        }

        return Cursors.Cross;
    }

    private void InvalidateSelectionChange(Rectangle previousBounds, Rectangle previousToolbarBounds)
    {
        Invalidate(InflateSelectionForRepaint(previousBounds));
        Invalidate(InflateSelectionForRepaint(imageBounds));
        Invalidate(InflateForRepaint(previousToolbarBounds));
        Invalidate(InflateForRepaint(toolbar.Bounds));
    }

    private static Rectangle InflateSelectionForRepaint(Rectangle bounds)
    {
        var inflated = bounds;
        inflated.Inflate(14, 14);
        inflated.Y -= 32;
        inflated.Height += 32;
        return inflated;
    }

    private static Rectangle InflateForRepaint(Rectangle bounds)
    {
        var inflated = bounds;
        inflated.Inflate(14, 14);
        return inflated;
    }

    private void SyncCanvasFromDesktop()
    {
        if (hasAnnotations)
        {
            return;
        }

        var next = desktopImage.Clone(imageBounds, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        canvas.Dispose();
        canvas = next;
    }

    private Bitmap CreateCurrentImage()
    {
        if (hasAnnotations)
        {
            return new Bitmap(canvas);
        }

        return desktopImage.Clone(imageBounds, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
    }

    private Point ToImagePoint(Point screenPoint)
    {
        return new Point(screenPoint.X - imageBounds.Left, screenPoint.Y - imageBounds.Top);
    }

    private Point ToScreenPoint(Point imagePoint)
    {
        return new Point(imageBounds.Left + imagePoint.X, imageBounds.Top + imagePoint.Y);
    }

    private static Rectangle GetRectangle(Point a, Point b)
    {
        var left = Math.Min(a.X, b.X);
        var top = Math.Min(a.Y, b.Y);
        var right = Math.Max(a.X, b.X);
        var bottom = Math.Max(a.Y, b.Y);
        return new Rectangle(left, top, right - left, bottom - top);
    }

    private static void DrawArrow(Graphics graphics, Pen pen, Point start, Point end)
    {
        pen.CustomEndCap = new System.Drawing.Drawing2D.AdjustableArrowCap(5, 7);
        graphics.DrawLine(pen, start, end);
    }

    [Flags]
    private enum ResizeDirection
    {
        None = 0,
        Left = 1,
        Right = 2,
        Top = 4,
        Bottom = 8
    }
}
