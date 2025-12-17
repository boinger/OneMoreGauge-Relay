using System.Text.Json;

namespace OneMoreGaugeRelay.Models;

/// <summary>
/// Application settings, persisted to user's AppData folder
/// </summary>
public class AppSettings
{
    private static readonly string SettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "OneMoreGaugeRelay",
        "settings.json"
    );

    public int BroadcastPort { get; set; } = 20777;
    public int UpdateRate { get; set; } = 60;  // Hz
    public bool AutoStart { get; set; } = true;
    public bool StartWithWindows { get; set; } = false;

    /// <summary>
    /// Loads settings from disk, or returns defaults if not found
    /// </summary>
    public static AppSettings Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                var json = File.ReadAllText(SettingsPath);
                return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
        }
        catch
        {
            // If loading fails, use defaults
        }
        return new AppSettings();
    }

    /// <summary>
    /// Saves settings to disk
    /// </summary>
    public void Save()
    {
        try
        {
            var directory = Path.GetDirectoryName(SettingsPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SettingsPath, json);
        }
        catch
        {
            // Silently fail if we can't save settings
        }
    }
}
