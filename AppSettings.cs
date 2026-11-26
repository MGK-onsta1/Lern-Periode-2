using System.Text.Json;

namespace Morse
{
    /// <summary>
    /// Einstellungen, die beim nächsten Start wieder da sein sollen
    /// (%AppData%\Morse\einstellungen.json). Das Chat-Passwort wird absichtlich NICHT gespeichert.
    /// </summary>
    public sealed class AppSettings
    {
        public int Wpm { get; set; } = 12;
        public int Frequency { get; set; } = 600;
        public int Volume { get; set; } = 50;          // 0–100 %
        public decimal GapFactor { get; set; } = 1.0m; // 1 = normale Pausen, 2 = doppelt so lang (zum Lernen)
        public string ChatName { get; set; } = Environment.UserName;
        public bool PlayIncoming { get; set; } = true;

        public static readonly string DefaultPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Morse", "einstellungen.json");

        public static AppSettings Load(string? path = null)
        {
            try
            {
                path ??= DefaultPath;
                if (File.Exists(path))
                    return JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(path)) ?? new AppSettings();
            }
            catch (Exception) { }
            return new AppSettings();
        }

        public void Save(string? path = null)
        {
            try
            {
                path ??= DefaultPath;
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                File.WriteAllText(path, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch (Exception) { }
        }
    }
}
