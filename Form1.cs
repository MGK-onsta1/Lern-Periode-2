namespace Morse
{
    public partial class Form1 : Form
    {
        private readonly MorseAudio _audio = new();
        private readonly MorseKeyer _keyer = new();
        private readonly LearnTrainer _trainer = new();
        private readonly AppSettings _settings = AppSettings.Load();

        /// <summary>Eingegebene Buchstaben-Codes der Leertasten-Eingabe; "/" steht für eine Wortpause.</summary>
        private readonly List<string> _keyedTokens = new();

        /// <summary>Gibt es gerade eine offene Aufgabe im Lern-Modus?</summary>
        private bool _learnActive;
        private bool _learnStarted;
        private bool _loadingSettings;

        /// <summary>Im Wort-Modus mit Leertaste: die bisher gemorsten Buchstaben-Codes.</summary>
        private readonly List<string> _learnWordCodes = new();

        private const int ModeKeySpace = 0;   // Zeichen sehen → mit Leertaste morsen
        private const int ModeListen = 1;     // Morse hören → Zeichen tippen
        private const int ModeRead = 2;       // Morse lesen → Zeichen tippen
        private const int ModeWordListen = 3; // Wort hören → Wort tippen
        private const int ModeWordKey = 4;    // Wort sehen → mit Leertaste morsen

        private bool IsWordMode => cmbLearnMode.SelectedIndex is ModeWordListen or ModeWordKey;
        private bool IsKeyMode => cmbLearnMode.SelectedIndex is ModeKeySpace or ModeWordKey;

        public Form1()
        {
            InitializeComponent();

            _keyer.SymbolAdded += Keyer_SymbolAdded;
            _keyer.LetterCompleted += Keyer_LetterCompleted;
            _keyer.WordCompleted += Keyer_WordCompleted;
            txtLearnAnswer.KeyDown += txtLearnAnswer_KeyDown;
        }

        private void Form1_Load(object? sender, EventArgs e)
        {
            cmbLearnMode.Items.AddRange(new object[]
            {
                "Zeichen sehen → mit Leertaste morsen",
                "Morse hören → Zeichen tippen",
                "Morse lesen → Zeichen tippen",
                "Wort hören → Wort tippen",
                "Wort sehen → mit Leertaste morsen",
            });
            foreach (var level in LearnTrainer.Levels)
                cmbLevel.Items.Add(level.Name);
            cmbLearnMode.SelectedIndex = 0;
            cmbLevel.SelectedIndex = 0;

            // gespeicherte Einstellungen in die Regler übernehmen
            _loadingSettings = true;
            numWpm.Value = Math.Clamp(_settings.Wpm, (int)numWpm.Minimum, (int)numWpm.Maximum);
            numFreq.Value = Math.Clamp(_settings.Frequency, (int)numFreq.Minimum, (int)numFreq.Maximum);
            trackVolume.Value = Math.Clamp(_settings.Volume, trackVolume.Minimum, trackVolume.Maximum);
            numGap.Value = Math.Clamp(_settings.GapFactor, numGap.Minimum, numGap.Maximum);
            _loadingSettings = false;

            ApplySettings();
            _trainer.Load();   // gespeicherten Punktestand laden
            UpdateScore();
            InitChat();
            timerKeyer.Start();
        }

        // ================================================================ Einstellungen

        private void numWpm_ValueChanged(object? sender, EventArgs e) => ApplySettings();
        private void numFreq_ValueChanged(object? sender, EventArgs e) => ApplySettings();
        private void settings_Changed(object? sender, EventArgs e) => ApplySettings();

        private void ApplySettings()
        {
            if (_loadingSettings) return;

            _audio.Wpm = (int)numWpm.Value;
            _audio.Frequency = (int)numFreq.Value;
            _audio.Volume = trackVolume.Value;
            _audio.GapFactor = (double)numGap.Value;
            _keyer.UnitMs = _audio.UnitMs;
            _keyer.GapFactor = _audio.GapFactor;

            string gapInfo = numGap.Value > 1
                ? $"   Pausen {numGap.Value:0.0}× länger (zum Lernen)"
                : "";
            lblUnit.Text = $"Punkt = {_audio.UnitMs:0} ms   Strich = {_audio.UnitMs * 3:0} ms   " +
                           $"(Strich ab {_audio.UnitMs * 2:0} ms Drücken)   Lautstärke {trackVolume.Value} %{gapInfo}";

            _settings.Wpm = (int)numWpm.Value;
            _settings.Frequency = (int)numFreq.Value;
            _settings.Volume = trackVolume.Value;
            _settings.GapFactor = numGap.Value;
            _settings.Save();
        }

        private void btnTestTone_Click(object? sender, EventArgs e)
        {
            _audio.Play(MorseTable.TextToMorse("MORSE"));
            SetStatus("Test: „MORSE“ wird mit den aktuellen Ton-Einstellungen abgespielt.");
            ActiveControl = null;
        }

        private void tabMain_SelectedIndexChanged(object? sender, EventArgs e)
        {
            // Beim Tab-Wechsel laufende Eingaben und Töne beenden
            _audio.Stop();
            _keyer.Reset();
            ShowKeyPressed(false);
            lblCurrentCode.Text = "";
            lblLearnInput.Text = "";
            if (tabMain.SelectedTab == tabLearn && !IsKeyMode)
                txtLearnAnswer.Focus();
        }

        private void SetStatus(string text) => lblStatus.Text = text;

        // ================================================================ Übersetzer

        private void btnTranslate_Click(object? sender, EventArgs e)
        {
            if (MorseTable.LooksLikeMorse(txtInput.Text)) TranslateToText();
            else TranslateToMorse();
        }

        private void btnToMorse_Click(object? sender, EventArgs e) => TranslateToMorse();
        private void btnToText_Click(object? sender, EventArgs e) => TranslateToText();

        private void TranslateToMorse()
        {
            if (string.IsNullOrWhiteSpace(txtInput.Text))
            {
                SetStatus("Bitte zuerst etwas eingeben.");
                return;
            }
            string morse = MorseTable.TextToMorse(txtInput.Text, out string unknown);
            txtOutput.Text = morse;
            SetStatus(unknown.Length == 0
                ? "Text → Morse übersetzt."
                : $"Text → Morse übersetzt. Nicht übersetzbare Zeichen übersprungen: {unknown}");
        }

        private void TranslateToText()
        {
            if (string.IsNullOrWhiteSpace(txtInput.Text))
            {
                SetStatus("Bitte zuerst etwas eingeben.");
                return;
            }
            if (!MorseTable.LooksLikeMorse(txtInput.Text))
            {
                SetStatus("Die Eingabe ist kein Morsecode (erlaubt: . - Leerzeichen /).");
                return;
            }
            string text = MorseTable.MorseToText(txtInput.Text, out int unknownCount);
            txtOutput.Text = text;
            SetStatus(unknownCount == 0
                ? "Morse → Text übersetzt."
                : $"Morse → Text übersetzt. {unknownCount} unbekannte Code(s) als '?' dargestellt.");
        }

        private void btnPlay_Click(object? sender, EventArgs e)
        {
            // Der Morsecode kommt aus der Eingabe (falls sie Morse ist) oder wird aus dem Text erzeugt.
            string input = txtInput.Text;
            string morse;
            if (MorseTable.LooksLikeMorse(input)) morse = input;
            else if (MorseTable.LooksLikeMorse(txtOutput.Text)) morse = txtOutput.Text;
            else morse = MorseTable.TextToMorse(input);

            if (string.IsNullOrWhiteSpace(morse))
            {
                SetStatus("Nichts zum Abspielen.");
                return;
            }
            _audio.Play(morse);
            SetStatus($"Spiele ab … (Dauer ca. {_audio.DurationMs(morse) / 1000:0.0} s)");
        }

        private void btnStop_Click(object? sender, EventArgs e)
        {
            _audio.Stop();
            SetStatus("Wiedergabe gestoppt.");
        }

        private void btnClear_Click(object? sender, EventArgs e)
        {
            txtInput.Clear();
            txtOutput.Clear();
            txtInput.Focus();
            SetStatus("Bereit.");
        }

        private void btnSwap_Click(object? sender, EventArgs e)
        {
            if (txtOutput.TextLength == 0) return;
            txtInput.Text = txtOutput.Text;
            btnTranslate_Click(sender, e);
        }

        private void btnCopy_Click(object? sender, EventArgs e)
        {
            if (txtOutput.TextLength == 0) return;
            Clipboard.SetText(txtOutput.Text);
            SetStatus("Ausgabe in die Zwischenablage kopiert.");
        }

        // ================================================================ Leertasten-Eingabe

        /// <summary>Soll die Leertaste gerade als Morsetaste dienen?</summary>
        private bool SpaceIsMorseKey =>
            tabMain.SelectedTab == tabKey ||
            (tabMain.SelectedTab == tabLearn && IsKeyMode) ||
            ChatKeyActive;

        private void Form1_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Space || !SpaceIsMorseKey) return;
            e.Handled = true;
            e.SuppressKeyPress = true;

            if (_keyer.IsPressed) return; // automatische Tastenwiederholung von Windows
            if (tabMain.SelectedTab == tabLearn && !_learnActive) return;

            _keyer.Press();
            ShowKeyPressed(true);
            if (chkKeySound.Checked) _audio.StartTone();
        }

        private void Form1_KeyUp(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Space || !SpaceIsMorseKey) return;
            e.Handled = true;
            e.SuppressKeyPress = true;
            ReleaseKey();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _trainer.Save();   // beim Schliessen sicherheitshalber nochmal speichern
            ShutdownChat();
            base.OnFormClosing(e);
        }

        private void Form1_Deactivate(object? sender, EventArgs e)
        {
            // Fenster verliert den Fokus, während die Taste gedrückt ist → sauber loslassen
            if (_keyer.IsPressed) ReleaseKey();
        }

        private void ReleaseKey()
        {
            if (!_keyer.IsPressed) return;
            _audio.Stop();
            ShowKeyPressed(false);
            _keyer.Release();
        }

        private void ShowKeyPressed(bool pressed)
        {
            pnlKey.BackColor = pressed ? Color.Gold : Color.Gainsboro;
            lblKeyState.ForeColor = pressed ? Color.Black : Color.DimGray;
        }

        private void timerKeyer_Tick(object? sender, EventArgs e) => _keyer.Tick();

        private void Keyer_SymbolAdded(char symbol)
        {
            string pretty = MorseTable.Pretty(_keyer.CurrentCode);
            if (ChatKeyActive)
            {
                ShowChatKeyInput(_keyer.CurrentCode);
            }
            else if (tabMain.SelectedTab == tabKey)
            {
                lblCurrentCode.Text = pretty;
                lblKeyState.Text = symbol == '.' ? "·  Punkt" : "−  Strich";
            }
            else if (cmbLearnMode.SelectedIndex == ModeWordKey)
            {
                ShowLearnWordInput(_keyer.CurrentCode);
            }
            else
            {
                lblLearnInput.Text = pretty;
            }
        }

        private void Keyer_LetterCompleted(string code)
        {
            if (ChatKeyActive)
            {
                ChatKeyToken(code);
            }
            else if (tabMain.SelectedTab == tabKey)
            {
                _keyedTokens.Add(code);
                lblCurrentCode.Text = "";
                lblKeyState.Text = "LEERTASTE";
                RefreshKeyedText();
            }
            else if (tabMain.SelectedTab == tabLearn && _learnActive)
            {
                if (cmbLearnMode.SelectedIndex == ModeWordKey)
                {
                    _learnWordCodes.Add(code);
                    ShowLearnWordInput("");
                }
                else
                {
                    CheckLearnAnswer(code: code);
                }
            }
        }

        /// <summary>Zeigt im Wort-Modus die schon gemorsten Buchstaben an (plus den aktuellen Code).</summary>
        private void ShowLearnWordInput(string current)
        {
            string letters = MorseTable.MorseToText(string.Join(" ", _learnWordCodes));
            string codes = MorseTable.Pretty(string.Join(" ", _learnWordCodes.Append(current)).Trim());
            lblLearnInput.Text = letters.Length == 0 ? codes : $"{letters}   {codes}";
        }

        private void Keyer_WordCompleted()
        {
            if (ChatKeyActive) { ChatKeyToken("/"); return; }
            if (tabMain.SelectedTab == tabLearn && cmbLearnMode.SelectedIndex == ModeWordKey
                && _learnActive && _learnWordCodes.Count > 0)
            {
                CheckWordAnswer(codes: _learnWordCodes);
                return;
            }
            if (tabMain.SelectedTab != tabKey) return;
            if (_keyedTokens.Count > 0 && _keyedTokens[^1] != "/")
            {
                _keyedTokens.Add("/");
                RefreshKeyedText();
            }
        }

        private void RefreshKeyedText()
        {
            string morse = string.Join(" ", _keyedTokens);
            txtKeyMorse.Text = MorseTable.Pretty(morse);
            txtKeyText.Text = MorseTable.MorseToText(morse);
        }

        private void btnKeyClear_Click(object? sender, EventArgs e)
        {
            _keyedTokens.Clear();
            _keyer.Reset();
            lblCurrentCode.Text = "";
            RefreshKeyedText();
            ActiveControl = null;
        }

        private void btnKeyBack_Click(object? sender, EventArgs e)
        {
            if (_keyedTokens.Count > 0)
            {
                _keyedTokens.RemoveAt(_keyedTokens.Count - 1);
                RefreshKeyedText();
            }
            ActiveControl = null;
        }

        private void btnKeyTransfer_Click(object? sender, EventArgs e)
        {
            if (_keyedTokens.Count == 0) return;
            txtInput.Text = string.Join(" ", _keyedTokens).Trim(' ', '/');
            tabMain.SelectedTab = tabTranslate;
            TranslateToText();
        }

        // ================================================================ Lern-Modus

        private void cmbLearnMode_SelectedIndexChanged(object? sender, EventArgs e)
        {
            int mode = cmbLearnMode.SelectedIndex;
            bool typing = !IsKeyMode;
            lblAnswerCaption.Visible = typing;
            txtLearnAnswer.Visible = typing;

            // Für ganze Wörter braucht das Antwortfeld mehr Platz
            txtLearnAnswer.MaxLength = IsWordMode ? 30 : 1;
            txtLearnAnswer.Width = IsWordMode ? 240 : 50;
            txtLearnAnswer.TextAlign = IsWordMode ? HorizontalAlignment.Left : HorizontalAlignment.Center;
            lblAnswerCaption.Text = IsWordMode ? "Deine Antwort (Wort + Enter):" : "Deine Antwort (Zeichen tippen):";

            lblLearnHelp.Text = mode switch
            {
                ModeKeySpace => "Oben erscheint ein Zeichen. Morse es mit der LEERTASTE (kurz = Punkt, lang = Strich). " +
                                "Nach einer kurzen Pause wird automatisch geprüft.",
                ModeListen => "Du hörst ein Morsezeichen. Tippe den passenden Buchstaben bzw. die Ziffer ins Antwortfeld.",
                ModeRead => "Oben steht ein Morsecode. Tippe den passenden Buchstaben bzw. die Ziffer ins Antwortfeld.",
                ModeWordListen => "Du hörst ein ganzes Wort. Tippe es ins Antwortfeld und drücke Enter. " +
                                  "Tipp: Oben „Pausen ×“ auf 2 stellen, dann hörst du die Buchstaben besser.",
                _ => "Oben steht ein Wort. Morse es Buchstabe für Buchstabe mit der LEERTASTE. " +
                     "Nach einer langen Pause (Wortpause) wird geprüft.",
            };
            StopLearning();
        }

        private void cmbLevel_SelectedIndexChanged(object? sender, EventArgs e)
        {
            int i = Math.Max(0, cmbLevel.SelectedIndex);
            _trainer.Chars = LearnTrainer.Levels[i].Chars;
            StopLearning();
        }

        private void StopLearning()
        {
            timerLearnNext.Stop();
            _learnActive = false;
            _learnStarted = false;
            _keyer.Reset();
            lblChallenge.Text = "–";
            lblLearnInput.Text = "";
            lblFeedback.Text = "";
            SetLearnAnswerSilently("");
        }

        private void SetLearnAnswerSilently(string text)
        {
            txtLearnAnswer.TextChanged -= txtLearnAnswer_TextChanged;
            txtLearnAnswer.Text = text;
            txtLearnAnswer.TextChanged += txtLearnAnswer_TextChanged;
        }

        private void btnLearnStart_Click(object? sender, EventArgs e) => NextChallenge();

        private void timerLearnNext_Tick(object? sender, EventArgs e)
        {
            timerLearnNext.Stop();
            NextChallenge();
        }

        private void NextChallenge()
        {
            timerLearnNext.Stop();
            _keyer.Reset();
            _learnStarted = true;
            _learnWordCodes.Clear();

            lblFeedback.Text = "";
            lblLearnInput.Text = "";
            SetLearnAnswerSilently("");

            if (IsWordMode)
            {
                string word = _trainer.NextWord();
                if (cmbLearnMode.SelectedIndex == ModeWordListen)
                {
                    lblChallenge.Text = $"🔊 ? ({word.Length} Zeichen)";
                    _audio.Play(_trainer.CurrentWordCode);
                    txtLearnAnswer.Focus();
                }
                else
                {
                    lblChallenge.Text = word;
                    ActiveControl = null;
                }
                _learnActive = true;
                return;
            }

            char c = _trainer.Next();
            string code = _trainer.CurrentCode;

            switch (cmbLearnMode.SelectedIndex)
            {
                case ModeKeySpace:
                    lblChallenge.Text = c.ToString();
                    ActiveControl = null; // kein Button soll die Leertaste "fangen"
                    break;
                case ModeListen:
                    lblChallenge.Text = "🔊 ?";
                    _audio.Play(code);
                    txtLearnAnswer.Focus();
                    break;
                case ModeRead:
                    lblChallenge.Text = MorseTable.Pretty(code);
                    txtLearnAnswer.Focus();
                    break;
            }
            _learnActive = true;
        }

        private void txtLearnAnswer_TextChanged(object? sender, EventArgs e)
        {
            if (!_learnActive || IsWordMode || txtLearnAnswer.TextLength == 0) return;
            CheckLearnAnswer(typed: txtLearnAnswer.Text[0]);
        }

        private void txtLearnAnswer_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter || !IsWordMode) return;
            e.Handled = e.SuppressKeyPress = true; // kein "Ding"-Ton
            if (_learnActive && txtLearnAnswer.Text.Trim().Length > 0)
                CheckWordAnswer(typed: txtLearnAnswer.Text);
        }

        private void CheckWordAnswer(string? typed = null, List<string>? codes = null)
        {
            _learnActive = false;
            string word = _trainer.CurrentWord;
            string wordCode = MorseTable.Pretty(_trainer.CurrentWordCode);

            bool ok = typed != null ? _trainer.CheckWord(typed) : _trainer.CheckWordCodes(codes!);
            string yours = typed != null
                ? typed.Trim().ToUpperInvariant()
                : MorseTable.MorseToText(string.Join(" ", codes!));

            if (ok)
            {
                lblFeedback.ForeColor = Color.ForestGreen;
                lblFeedback.Text = $"✔ Richtig!  {word}";
            }
            else
            {
                lblFeedback.ForeColor = Color.Firebrick;
                lblFeedback.Text = $"✘ Falsch. Du hattest „{yours}“ – richtig ist „{word}“";
                _audio.Play(_trainer.CurrentWordCode);
            }
            lblChallenge.Text = word;
            lblLearnInput.Text = wordCode;

            UpdateScore();
            _trainer.Save();
            timerLearnNext.Interval = ok ? 1800 : 3500;
            timerLearnNext.Start();
        }

        private void CheckLearnAnswer(string? code = null, char? typed = null)
        {
            _learnActive = false;
            char target = _trainer.Current;
            string targetCode = MorseTable.Pretty(_trainer.CurrentCode);

            bool ok = code != null ? _trainer.CheckCode(code) : _trainer.CheckChar(typed!.Value);

            if (ok)
            {
                lblFeedback.ForeColor = Color.ForestGreen;
                lblFeedback.Text = $"✔ Richtig!  {target} = {targetCode}";
            }
            else
            {
                lblFeedback.ForeColor = Color.Firebrick;
                string yours = code != null
                    ? $"Du hast {MorseTable.Pretty(code)} gemorst"
                    : $"Du hast „{typed}“ getippt";
                lblFeedback.Text = $"✘ Falsch. {yours} – richtig ist {target} = {targetCode}";
                _audio.Play(_trainer.CurrentCode); // richtige Lösung zum Mithören
            }

            if (cmbLearnMode.SelectedIndex != ModeKeySpace)
                lblChallenge.Text = $"{target}   {targetCode}";

            UpdateScore();
            _trainer.Save();   // nach jeder Antwort speichern
            timerLearnNext.Interval = ok ? 1200 : 2500;
            timerLearnNext.Start();
        }

        private void btnLearnRepeat_Click(object? sender, EventArgs e)
        {
            if (!_learnStarted) return;
            _audio.Play(IsWordMode ? _trainer.CurrentWordCode : _trainer.CurrentCode);
            RestoreLearnFocus();
        }

        private void btnLearnHint_Click(object? sender, EventArgs e)
        {
            if (!_learnStarted) return;
            string code = IsWordMode ? _trainer.CurrentWordCode : _trainer.CurrentCode;
            lblFeedback.ForeColor = Color.DarkGoldenrod;
            lblFeedback.Text = cmbLearnMode.SelectedIndex switch
            {
                ModeWordListen => $"💡 Das Wort beginnt mit „{_trainer.CurrentWord[0]}“ und hat {_trainer.CurrentWord.Length} Zeichen.",
                ModeWordKey => $"💡 {MorseTable.Pretty(code)}",
                ModeKeySpace => $"💡 {_trainer.Current} = {MorseTable.Pretty(code)}",
                ModeListen => $"💡 Das war: {MorseTable.Pretty(code)}",
                _ => "💡 Kommt in Frage: " + string.Join(" ", _trainer.Chars.Where(ch => MorseTable.Codes[ch].Length == code.Length && MorseTable.Codes[ch][0] == code[0])),
            };
            RestoreLearnFocus();
        }

        private void btnLearnReset_Click(object? sender, EventArgs e)
        {
            _trainer.ResetScore();
            _trainer.Save();
            UpdateScore();
            RestoreLearnFocus();
        }

        private void RestoreLearnFocus()
        {
            if (!IsKeyMode) txtLearnAnswer.Focus();
            else ActiveControl = null;
        }

        private void UpdateScore()
        {
            lblScore.Text = _trainer.ScoreText;
            string weak = _trainer.WeakChars;
            lblWeak.Text = weak.Length == 0
                ? "Der Punktestand wird automatisch gespeichert."
                : $"Diese Zeichen solltest du noch üben (kommen öfter dran): {weak}";
        }
    }
}
