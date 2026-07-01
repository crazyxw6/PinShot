namespace PinShot;

internal sealed class AnnotationEditorForm : Form
{
    private const int ResizeGripSize = 7;
    private const int MinimumSelectionSize = 24;

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
        e.Graphics.DrawImageUnscaled(desktopImage, 0, 0);

        using var dim = new SolidBrush(Color.FromArgb(88, Color.Black));
        e.Graphics.FillRectangle(dim, ClientRectangle);

        if (hasAnnotations)
        {
            e.Graphics.DrawImageUnscaled(canvas, imageBounds.Location);
        }
        else
        {
            e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            e.Graphics.DrawImage(desktopImage, imageBounds, imageBounds, GraphicsUnit.Pixel);
            e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Bilinear;
        }

        using var border = new Pen(Color.FromArgb(0, 122, 255), 2);