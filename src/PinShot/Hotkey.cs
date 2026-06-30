using System.Text;

namespace PinShot;

[Flags]
internal enum HotkeyModifiers
{
    None = 0,
    Alt = 1,
    Control = 2,
    Shift = 4,
    Win = 8
}

internal sealed class Hotkey
{
    public Hotkey(HotkeyModifiers modifiers, Keys key)
    {
        Modifiers = modifiers;
        Key = key;
    }

    public HotkeyModifiers Modifiers { get; }
    public Keys Key { get; }

    public static Hotkey Default => new(HotkeyModifiers.Control | HotkeyModifiers.Alt, Keys.A);

    public static bool TryParse(string? value, out Hotkey hotkey)
    {
        hotkey = Default;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var modifiers = HotkeyModifiers.None;
        Keys key = Keys.None;

        foreach (var rawPart in value.Split('+', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
        {
            var part = rawPart.Equals("Ctrl", StringComparison.OrdinalIgnoreCase) ? "Control" : rawPart;

            if (part.Equals("Control", StringComparison.OrdinalIgnoreCase))
            {
                modifiers |= HotkeyModifiers.Control;
            }
            else if (part.Equals("Alt", StringComparison.OrdinalIgnoreCase))
            {
                modifiers |= HotkeyModifiers.Alt;
            }
            else if (part.Equals("Shift", StringComparison.OrdinalIgnoreCase))
            {
                modifiers |= HotkeyModifiers.Shift;
            }
            else if (part.Equals("Win", StringComparison.OrdinalIgnoreCase))
            {
                modifiers |= HotkeyModifiers.Win;
            }
            else if (Enum.TryParse(part, ignoreCase: true, out Keys parsedKey))
            {
                key = parsedKey;
            }
        }

        if (key is Keys.None or Keys.ControlKey or Keys.Menu or Keys.ShiftKey or Keys.LWin or Keys.RWin)
        {
            return false;
        }

        hotkey = new Hotkey(modifiers, key);
        return true;
    }

    public override string ToString()
    {
        var builder = new StringBuilder();

        AppendModifier(builder, HotkeyModifiers.Control, "Ctrl");
        AppendModifier(builder, HotkeyModifiers.Alt, "Alt");
        AppendModifier(builder, HotkeyModifiers.Shift, "Shift");
        AppendModifier(builder, HotkeyModifiers.Win, "Win");

        if (builder.Length > 0)
        {
            builder.Append('+');
        }

        builder.Append(Key);
        return builder.ToString();
    }

    private void AppendModifier(StringBuilder builder, HotkeyModifiers modifier, string text)
    {
        if (!Modifiers.HasFlag(modifier))
        {
            return;
        }

        if (builder.Length > 0)
        {
            builder.Append('+');
        }

        builder.Append(text);
    }
}
