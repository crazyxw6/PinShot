namespace PinShot;

internal sealed class TextOptions
{
    public Color Color { get; set; } = Color.Red;
    public int FontSize { get; set; } = 20;
    public bool Bold { get; set; } = true;
    public bool Italic { get; set; }
    public bool Outline { get; set; }

    public FontStyle FontStyle
    {
        get
        {
            var style = FontStyle.Regular;
            if (Bold)
            {
                style |= FontStyle.Bold;
            }

            if (Italic)
            {
                style |= FontStyle.Italic;
            }

            return style;
        }
    }
}
