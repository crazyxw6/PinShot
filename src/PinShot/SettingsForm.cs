namespace PinShot;

internal sealed class SettingsForm : Form
{
    private readonly TextBox hotkeyBox;
    private Hotkey selectedHotkey;

    public SettingsForm(Hotkey currentHotkey)
    {
        selectedHotkey = currentHotkey;

        Text = "PinShot 设置";
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(360, 150);

        var label = new Label
        {
            Text = "截图快捷键",
            Location = new Point(20, 22),
            AutoSize = true
        };

        hotkeyBox = new TextBox
        {
            Location = new Point(20, 48),
            Width = 315,
            ReadOnly = true,
            Text = selectedHotkey.ToString()
        };
        hotkeyBox.KeyDown += OnHotkeyKeyDown;

        var saveButton = new Button
        {
            Text = "保存",
            Location = new Point(175, 100),
            Width = 75,
            DialogResult = DialogResult.OK
        };
        saveButton.Click += (_, _) => Close();

        var cancelButton = new Button
        {
            Text = "取消",
            Location = new Point(260, 100),
            Width = 75,
            DialogResult = DialogResult.Cancel
        };

        Controls.AddRange(new Control[] { label, hotkeyBox, saveButton, cancelButton });
        AcceptButton = saveButton;
        CancelButton = cancelButton;
    }

    public Hotkey SelectedHotkey => selectedHotkey;

    private void OnHotkeyKeyDown(object? sender, KeyEventArgs e)
    {
        e.SuppressKeyPress = true;

        var key = e.KeyCode;
        if (key is Keys.ControlKey or Keys.ShiftKey or Keys.Menu or Keys.LWin or Keys.RWin)
        {
            return;
        }

        var modifiers = HotkeyModifiers.None;
        if (e.Control)
        {
            modifiers |= HotkeyModifiers.Control;
        }

        if (e.Alt)
        {
            modifiers |= HotkeyModifiers.Alt;
        }

        if (e.Shift)
        {
            modifiers |= HotkeyModifiers.Shift;
        }

        selectedHotkey = new Hotkey(modifiers, key);
        hotkeyBox.Text = selectedHotkey.ToString();
    }
}
