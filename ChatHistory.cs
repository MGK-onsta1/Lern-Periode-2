using System.Text;
using System.Text.Json;

namespace Morse
{
    /// <summary>
    /// Speichert den Chatverlauf als JSON in %AppData%\Morse\chatverlauf.json,
    /// damit die Nachrichten nach dem Schließen nicht weg sind.
    /// </summary>
    public sealed class ChatHistory
    {
        /// <summary>Ein Eintrag. Who == null bedeutet Hinweis vom Programm (z. B. „Verbunden mit …“).</summary>
        public sealed record Entry(DateTime Time, string? Who, bool Mine, string Text);

        /// <summary>Mehr Einträge werden nicht aufgehoben (die ältesten fliegen raus).</summary>
        public const int MaxEntries = 1000;

        public static readonly string DefaultPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Morse", "chatverlauf.json");

        private readonly string _path;
        private readonly List<Entry> _entries = new();

        public ChatHistory(string? path = null) => _path = path ?? DefaultPath;

        public IReadOnlyList<Entry> Entries => _entries;

        public void Add(Entry entry)
        {
            _entries.Add(entry);
            if (_entries.Count > MaxEntries) _entries.RemoveRange(0, _entries.Count - MaxEntries);
            Save();
        }

        public void Clear()
        {
            _entries.Clear();
            Save();
        }

        public void Load()
        {
            try
            {
                if (!File.Exists(_path)) return;
                var list = JsonSerializer.Deserialize<List<Entry>>(File.ReadAllText(_path));
                _entries.Clear();
                if (list != null) _entries.AddRange(list.TakeLast(MaxEntries));
            }
            catch (Exception) { /* kaputte Datei → leerer Verlauf */ }
        }

        public void Save()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
                File.WriteAllText(_path, JsonSerializer.Serialize(_entries));
            }
            catch (Exception) { /* keine Schreibrechte → nicht speichern */ }
        }

        /// <summary>Verlauf als lesbare Textdatei (Text + Morsecode).</summary>
        public string ToText()
        {
            var sb = new StringBuilder();
            foreach (var e in _entries)
            {
                if (e.Who == null)
                    sb.AppendLine($"{e.Time:dd.MM.yyyy HH:mm}  ({e.Text})");
                else
                {
                    sb.AppendLine($"{e.Time:dd.MM.yyyy HH:mm}  {e.Who}: {MorseTable.MorseToText(e.Text)}");
                    sb.AppendLine($"                  {e.Text}");
                }
            }
            return sb.ToString();
        }
    }
}
