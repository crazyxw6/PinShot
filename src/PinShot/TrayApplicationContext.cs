namespace PinShot;

internal sealed class TrayApplicationContext : ApplicationContext
{
    private readonly NotifyIcon trayIcon;
    private readonly Icon appIcon;
    private readonly HotkeyManager hotkeyManager;
    private SettingsData settings;
    private Hotkey captureHotkey;
    private bool isCapturing;

    public TrayApplicationContext()
    {
        settings = SettingsData.Load();
        captureHotkey = Hotkey.TryParse(settings.CaptureHotkey, out var parsedHotkey) ? parsedHotkey : Hotkey.Default;
        appIcon = Icon.ExtractAssociatedIcon(Application.ExecutablePath) ?? SystemIcons.Application;

        trayIcon = new NotifyIcon
        {
            Icon = appIcon,
            Text = "PinShot",
            Visible = true,
            ContextMenuStrip = BuildMenu()
        };
        trayIcon.DoubleClick += (_, _) => CaptureArea();

        hotkeyManager = new HotkeyManager(CaptureArea);
        RegisterHotkey();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            hotkeyManager.Dispose();
            trayIcon.Visible = false;
            trayIcon.Dispose();
            appIcon.Dispose();
        }

        base.Dispose(disposing);
    }

    private ContextMenuStrip BuildMenu()
    {
        var menu = new ContextMenuStrip();
        menu.Items.Add("区域截图", null, (_, _) => CaptureArea());
        menu.Items.Add("设置快捷键", null, (_, _) => OpenSettings());
        menu.Items.Add("关于", null, (_, _) => ShowAbout());
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("退出", null, (_, _) => ExitThread());
        return menu;
    }

    private static void ShowAbout()
    {
        var version = typeof(TrayApplicationContext).Assembly.GetName().Version?.ToString(3) ?? "1.0.0";
        MessageBox.Show(
            $"PinShot {version}\n\n轻量截图和贴图工具。\n\nMIT License",
            "关于 PinShot",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private void RegisterHotkey()
    {
        if (hotkeyManager.Register(captureHotkey))
        {
            return;
        }

        trayIcon.ShowBalloonTip(
            3000,
            "PinShot",
            $"快捷键 {captureHotkey} 已被占用。请在托盘菜单里重新设置。",
            ToolTipIcon.Warning);
    }

    private void CaptureArea()
    {
        if (isCapturing)
        {
            return;
        }

        isCapturing = true;
        try
        {
            using var desktop = CaptureService.CaptureDesktop(out var virtualScreen);
            using var capture = CaptureOverlayForm.SelectArea(desktop, virtualScreen);
            if (capture is null)
            {
                return;
            }

            using var editor = new AnnotationEditorForm(desktop, virtualScreen, capture.Image, capture.ImageBounds);
            if (editor.ShowDialog() != DialogResult.OK || editor.ResultImage is null)
            {
                return;
            }

            Clipboard.SetImage(editor.ResultImage);
            new PinForm(editor.ResultImage).Show();
        }
        finally
        {
            isCapturing = false;
        }
    }

    private void OpenSettings()
    {
        using var form = new SettingsForm(captureHotkey);
        if (form.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        var previousHotkey = captureHotkey;
        var selectedHotkey = form.SelectedHotkey;
        if (!hotkeyManager.Register(selectedHotkey))
        {
            captureHotkey = previousHotkey;
            RegisterHotkey();

            trayIcon.ShowBalloonTip(
                3000,
                "PinShot",
                $"快捷键 {selectedHotkey} 已被占用，已保留原快捷键 {previousHotkey}。",
                ToolTipIcon.Warning);
            return;
        }

        captureHotkey = selectedHotkey;
        settings.CaptureHotkey = captureHotkey.ToString();
        settings.Save();
    }
}
