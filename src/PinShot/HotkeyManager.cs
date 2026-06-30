namespace PinShot;

internal sealed class HotkeyManager : IDisposable
{
    private const int HotkeyId = 100;
    private const int WmHotkey = 0x0312;
    private readonly MessageWindow window;
    private bool registered;

    public HotkeyManager(Action onPressed)
    {
        window = new MessageWindow(onPressed);
    }

    public bool Register(Hotkey hotkey)
    {
        Unregister();
        registered = NativeMethods.RegisterHotKey(
            window.Handle,
            HotkeyId,
            (uint)hotkey.Modifiers,
            (uint)hotkey.Key);

        return registered;
    }

    public void Unregister()
    {
        if (!registered)
        {
            return;
        }

        NativeMethods.UnregisterHotKey(window.Handle, HotkeyId);
        registered = false;
    }

    public void Dispose()
    {
        Unregister();
        window.DestroyHandle();
    }

    private sealed class MessageWindow : NativeWindow
    {
        private readonly Action onPressed;

        public MessageWindow(Action onPressed)
        {
            this.onPressed = onPressed;
            CreateHandle(new CreateParams());
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WmHotkey && m.WParam.ToInt32() == HotkeyId)
            {
                onPressed();
                return;
            }

            base.WndProc(ref m);
        }
    }
}
