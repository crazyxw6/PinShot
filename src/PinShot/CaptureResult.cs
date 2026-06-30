namespace PinShot;

internal sealed class CaptureResult : IDisposable
{
    public CaptureResult(Bitmap image, Rectangle imageBounds)
    {
        Image = image;
        ImageBounds = imageBounds;
    }

    public Bitmap Image { get; }
    public Rectangle ImageBounds { get; }

    public void Dispose()
    {
        Image.Dispose();
    }
}
