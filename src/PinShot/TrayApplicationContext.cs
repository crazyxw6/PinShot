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
