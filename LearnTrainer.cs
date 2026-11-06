namespace Morse
{
    /// <summary>
    /// Logik für den Lern-Modus: wählt Übungszeichen aus, prüft Antworten und zählt Punkte.
    /// Zeichen, die oft falsch waren, kommen häufiger dran.
    /// </summary>
    public sealed class LearnTrainer
    {
        /// <summary>Lernstufen, aufgebaut nach der Koch-/Farnsworth-Idee: erst einfache Zeichen.</summary>
        public static readonly (string Name, string Chars)[] Levels =
        {
            ("Stufe 1: E T I A N M", "ETIANM"),
            ("Stufe 2: + S O K R D U G W", "ETIANMSOKRDUGW"),
            ("Stufe 3: alle Buchstaben", "ABCDEFGHIJKLMNOPQRSTUVWXYZ"),
            ("Stufe 4: Buchstaben + Ziffern", "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"),
            ("Stufe 5: alles inkl. Umlaute & Satzzeichen", new string(MorseTable.Codes.Keys.ToArray())),
        };

        private readonly Random _random = new();
        private readonly Dictionary<char, int> _mistakes = new();

        public string Chars { get; set; } = Levels[0].Chars;
        public char Current { get; private set; }
        public string CurrentCode => MorseTable.Codes[Current];

        public int Correct { get; private set; }
        public int Wrong { get; private set; }
        public int Streak { get; private set; }
        public int BestStreak { get; private set; }

        /// <summary>Wählt das nächste Zeichen (nie zweimal dasselbe hintereinander).</summary>
        public char Next()
        {
            // Gewichtung: jedes Zeichen 1 Los + 2 Lose pro bisherigem Fehler.
            var pool = new List<char>();
            foreach (char c in Chars)
            {
                if (c == Current && Chars.Length > 1) continue;
                int weight = 1 + 2 * _mistakes.GetValueOrDefault(c);
                for (int i = 0; i < weight; i++) pool.Add(c);
            }
            Current = pool[_random.Next(pool.Count)];
            return Current;
        }

        /// <summary>Prüft einen eingegebenen Morsecode gegen das aktuelle Zeichen.</summary>
        public bool CheckCode(string code) => Score(code == CurrentCode);

        /// <summary>Prüft ein eingetipptes Zeichen gegen das aktuelle Zeichen.</summary>
        public bool CheckChar(char c)
        {
            char up = c == 'ß' ? c : char.ToUpperInvariant(c);
            return Score(up == Current);
        }

        private bool Score(bool ok)
        {
            if (ok)
            {
                Correct++;
                Streak++;
                BestStreak = Math.Max(BestStreak, Streak);
                if (_mistakes.TryGetValue(Current, out int m) && m > 0)
                    _mistakes[Current] = m - 1;
            }
            else
            {
                Wrong++;
                Streak = 0;
                _mistakes[Current] = _mistakes.GetValueOrDefault(Current) + 1;
            }
            return ok;
        }

        public string ScoreText
        {
            get
            {
                int total = Correct + Wrong;
                int pct = total == 0 ? 0 : (int)Math.Round(100.0 * Correct / total);
                return $"Richtig: {Correct}   Falsch: {Wrong}   Quote: {pct} %   Serie: {Streak}   Beste Serie: {BestStreak}";
            }
        }

        /// <summary>Zeichen, die am häufigsten falsch waren (für die Anzeige).</summary>
        public string WeakChars =>
            string.Join(" ", _mistakes.Where(kv => kv.Value > 0)
                                     .OrderByDescending(kv => kv.Value)
                                     .Take(5)
                                     .Select(kv => kv.Key));

        public void ResetScore()
        {
            Correct = Wrong = Streak = BestStreak = 0;
            _mistakes.Clear();
        }

        // ---------------------------------------------------------------- Speichern / Laden

        /// <summary>Speicherort: %AppData%\Morse\lernstand.json</summary>
        public static readonly string SavePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Morse", "lernstand.json");

        /// <summary>Daten, die in die JSON-Datei geschrieben werden.</summary>
        private sealed record SaveData(int Correct, int Wrong, int Streak, int BestStreak, Dictionary<string, int> Mistakes);

        /// <summary>Speichert den Punktestand. Fehler beim Speichern werden ignoriert (das Spiel läuft weiter).</summary>
        public void Save(string? path = null)
        {
            path ??= SavePath;
            try
            {
                var data = new SaveData(Correct, Wrong, Streak, BestStreak,
                    _mistakes.ToDictionary(kv => kv.Key.ToString(), kv => kv.Value));
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                File.WriteAllText(path, System.Text.Json.JsonSerializer.Serialize(data,
                    new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
            }
            catch (Exception) { /* z. B. keine Schreibrechte – dann eben nicht speichern */ }
        }

        /// <summary>Lädt den Punktestand, falls eine Datei existiert. Kaputte Dateien werden ignoriert.</summary>
        public void Load(string? path = null)
        {
            path ??= SavePath;
            try
            {
                if (!File.Exists(path)) return;
                var data = System.Text.Json.JsonSerializer.Deserialize<SaveData>(File.ReadAllText(path));
                if (data == null) return;

                Correct = data.Correct;
                Wrong = data.Wrong;
                Streak = data.Streak;
                BestStreak = data.BestStreak;
                _mistakes.Clear();
                foreach (var kv in data.Mistakes ?? new())
                    if (kv.Key.Length == 1) _mistakes[kv.Key[0]] = kv.Value;
            }
            catch (Exception) { /* kaputte Datei → mit 0 anfangen */ }
        }
    }
}
