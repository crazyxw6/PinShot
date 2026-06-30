using System.Drawing.Imaging;

namespace PinShot;

internal static class CaptureService
{
    public static Bitmap CaptureDesktop(out Rectangle virtualScreen)
    {
        virtualScreen = SystemInformation.VirtualScreen;
        var image = new Bitmap(virtualScreen.Width, virtualScreen.Height, PixelFormat.Format32bppArgb);

        using var graphics = Graphics.FromImage(image);
        graphics.CopyFromScreen(virtualScreen.Left, virtualScreen.Top, 0, 0, virtualScreen.Size, CopyPixelOperation.SourceCopy);

        return image;
    }
}
