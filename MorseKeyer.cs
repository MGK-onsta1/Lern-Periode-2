using System.Diagnostics;

namespace Morse
{
    /// <summary>
    /// Wertet Tastendrücke (Leertaste) als Morse-Eingabe aus.
    ///   kurz gedrückt  (&lt; 2 Einheiten) → Punkt
    ///   lang gedrückt  (≥ 2 Einheiten) → Strich
    ///   Pause ≥ 3 Einheiten → Buchstabe fertig
    ///   Pause ≥ 7 Einheiten → Wort fertig
    /// <see cref="Tick"/> muss regelmäßig (z. B. per Timer) aufgerufen werden.
    /// </summary>
    public sealed class MorseKeyer
    {
        private readonly Stopwatch _pressWatch = new();
        private readonly Stopwatch _pauseWatch = new();
        private bool _wordPending;

        public bool IsPressed { get; private set; }

        /// <summary>Länge einer Einheit in ms (kommt aus dem eingestellten Tempo).</summary>
        public double UnitMs { get; set; } = 100;

        /// <summary>Punkte/Striche des Buchstabens, der gerade eingegeben wird.</summary>
        public string CurrentCode { get; private set; } = "";

        /// <summary>Ein Punkt oder Strich wurde erkannt.</summary>
        public event Action<char>? SymbolAdded;

        /// <summary>Ein Buchstabe ist fertig (Parameter: sein Morsecode).</summary>
        public event Action<string>? LetterCompleted;

        /// <summary>Eine Wortpause wurde erkannt.</summary>
        public event Action? WordCompleted;

        public void Press()
        {
            if (IsPressed) return; // Tasten-Wiederholung von Windows ignorieren
            IsPressed = true;
            _pauseWatch.Reset();
            _pressWatch.Restart();
        }

        public void Release()
        {
            if (!IsPressed) return;
            IsPressed = false;
            _pressWatch.Stop();

            char symbol = _pressWatch.Elapsed.TotalMilliseconds < 2 * UnitMs ? '.' : '-';
            CurrentCode += symbol;
            SymbolAdded?.Invoke(symbol);

            _pauseWatch.Restart();
        }

        public void Tick()
        {
            if (IsPressed || !_pauseWatch.IsRunning) return;
            double pause = _pauseWatch.Elapsed.TotalMilliseconds;

            if (CurrentCode.Length > 0 && pause >= 3 * UnitMs)
            {
                string code = CurrentCode;
                CurrentCode = "";
                _wordPending = true;
                LetterCompleted?.Invoke(code);
            }

            if (_wordPending && pause >= 7 * UnitMs)
            {
                _wordPending = false;
                _pauseWatch.Reset();
                WordCompleted?.Invoke();
            }
        }

        /// <summary>Verwirft die aktuelle Eingabe.</summary>
        public void Reset()
        {
            IsPressed = false;
            CurrentCode = "";
            _wordPending = false;
            _pressWatch.Reset();
            _pauseWatch.Reset();
        }
    }
}
