using System.Text.Json;

namespace PinShot;

internal sealed class SettingsData
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public string CaptureHotkey { get; set; } = Hotkey.Default.ToString();
    public bool HasSeenTrayHint { get; set; }

    public static string SettingsDirectory =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PinShot");

    public static string SettingsPath => Path.Combine(SettingsDirectory, "settings.json");

    public static SettingsData Load()
    {
        try
        {
            if (!File.Exists(SettingsPath))
            {
                return new SettingsData();
            }

            var json = File.ReadAllText(SettingsPath);
            return JsonSerializer.Deserialize<SettingsData>(json) ?? new SettingsData();
        }
        catch
        {
            return new SettingsData();
        }
    }

    public void Save()
    {
        Directory.CreateDirectory(SettingsDirectory);
        File.WriteAllText(SettingsPath, JsonSerializer.Serialize(this, JsonOptions));
    }
}
