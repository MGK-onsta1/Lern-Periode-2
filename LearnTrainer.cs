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
    }
}
