namespace Morse
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            if (disposing)
            {
                _audio.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            panelTop = new Panel();
            lblWpm = new Label();
            numWpm = new NumericUpDown();
            lblFreq = new Label();
            numFreq = new NumericUpDown();
            lblUnit = new Label();
            tabMain = new TabControl();
            tabTranslate = new TabPage();
            lblInput = new Label();
            txtInput = new TextBox();
            btnTranslate = new Button();
            btnToMorse = new Button();
            btnToText = new Button();
            btnPlay = new Button();
            btnStop = new Button();
            btnClear = new Button();
            lblOutput = new Label();
            btnSwap = new Button();
            btnCopy = new Button();
            txtOutput = new TextBox();
            tabKey = new TabPage();
            lblKeyHelp = new Label();
            pnlKey = new Panel();
            lblKeyState = new Label();
            lblCurrentCodeCaption = new Label();
            lblCurrentCode = new Label();
            lblKeyMorseCaption = new Label();
            txtKeyMorse = new TextBox();
            lblKeyTextCaption = new Label();
            txtKeyText = new TextBox();
            btnKeyClear = new Button();
            btnKeyBack = new Button();
            btnKeyTransfer = new Button();
            chkKeySound = new CheckBox();
            tabLearn = new TabPage();
            lblLearnMode = new Label();
            cmbLearnMode = new ComboBox();
            lblLevel = new Label();
            cmbLevel = new ComboBox();
            lblLearnHelp = new Label();
            lblChallenge = new Label();
            lblLearnInput = new Label();
            lblAnswerCaption = new Label();
            txtLearnAnswer = new TextBox();
            btnLearnStart = new Button();
            btnLearnRepeat = new Button();
            btnLearnHint = new Button();
            btnLearnReset = new Button();
            lblFeedback = new Label();
            lblScore = new Label();
            lblWeak = new Label();
            statusStrip = new StatusStrip();
            lblStatus = new ToolStripStatusLabel();
            timerKeyer = new System.Windows.Forms.Timer(components);
            timerLearnNext = new System.Windows.Forms.Timer(components);
            panelTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numWpm).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numFreq).BeginInit();
            tabMain.SuspendLayout();
            tabTranslate.SuspendLayout();
            tabKey.SuspendLayout();
            pnlKey.SuspendLayout();
            tabLearn.SuspendLayout();
            statusStrip.SuspendLayout();
            SuspendLayout();
            //
            // panelTop
            //
            panelTop.Controls.Add(lblWpm);
            panelTop.Controls.Add(numWpm);
            panelTop.Controls.Add(lblFreq);
            panelTop.Controls.Add(numFreq);
            panelTop.Controls.Add(lblUnit);
            panelTop.Dock = DockStyle.Top;
            panelTop.Location = new Point(0, 0);
            panelTop.Name = "panelTop";
            panelTop.Size = new Size(820, 44);
            panelTop.TabIndex = 0;
            //
            // lblWpm
            //
            lblWpm.AutoSize = true;
            lblWpm.Location = new Point(12, 14);
            lblWpm.Name = "lblWpm";
            lblWpm.Size = new Size(126, 20);
            lblWpm.Text = "Tempo (Wörter/Min):";
            //
            // numWpm
            //
            numWpm.Location = new Point(160, 10);
            numWpm.Maximum = new decimal(new int[] { 40, 0, 0, 0 });
            numWpm.Minimum = new decimal(new int[] { 5, 0, 0, 0 });
            numWpm.Name = "numWpm";
            numWpm.Size = new Size(60, 27);
            numWpm.TabIndex = 1;
            numWpm.Value = new decimal(new int[] { 12, 0, 0, 0 });
            numWpm.ValueChanged += numWpm_ValueChanged;
            //
            // lblFreq
            //
            lblFreq.AutoSize = true;
            lblFreq.Location = new Point(244, 14);
            lblFreq.Name = "lblFreq";
            lblFreq.Size = new Size(76, 20);
            lblFreq.Text = "Ton (Hz):";
            //
            // numFreq
            //
            numFreq.Increment = new decimal(new int[] { 50, 0, 0, 0 });
            numFreq.Location = new Point(320, 10);
            numFreq.Maximum = new decimal(new int[] { 1200, 0, 0, 0 });
            numFreq.Minimum = new decimal(new int[] { 300, 0, 0, 0 });
            numFreq.Name = "numFreq";
            numFreq.Size = new Size(70, 27);
            numFreq.TabIndex = 2;
            numFreq.Value = new decimal(new int[] { 600, 0, 0, 0 });
            numFreq.ValueChanged += numFreq_ValueChanged;
            //
            // lblUnit
            //
            lblUnit.AutoSize = true;
            lblUnit.ForeColor = SystemColors.GrayText;
            lblUnit.Location = new Point(410, 14);
            lblUnit.Name = "lblUnit";
            lblUnit.Size = new Size(200, 20);
            lblUnit.Text = "Punkt = 100 ms";
            //
            // tabMain
            //
            tabMain.Controls.Add(tabTranslate);
            tabMain.Controls.Add(tabKey);
            tabMain.Controls.Add(tabLearn);
            tabMain.Dock = DockStyle.Fill;
            tabMain.Location = new Point(0, 44);
            tabMain.Name = "tabMain";
            tabMain.SelectedIndex = 0;
            tabMain.Size = new Size(820, 534);
            tabMain.TabIndex = 3;
            tabMain.SelectedIndexChanged += tabMain_SelectedIndexChanged;
            //
            // tabTranslate
            //
            tabTranslate.Controls.Add(lblInput);
            tabTranslate.Controls.Add(txtInput);
            tabTranslate.Controls.Add(btnTranslate);
            tabTranslate.Controls.Add(btnToMorse);
            tabTranslate.Controls.Add(btnToText);
            tabTranslate.Controls.Add(btnPlay);
            tabTranslate.Controls.Add(btnStop);
            tabTranslate.Controls.Add(btnClear);
            tabTranslate.Controls.Add(lblOutput);
            tabTranslate.Controls.Add(btnSwap);
            tabTranslate.Controls.Add(btnCopy);
            tabTranslate.Controls.Add(txtOutput);
            tabTranslate.Location = new Point(4, 29);
            tabTranslate.Name = "tabTranslate";
            tabTranslate.Padding = new Padding(3);
            tabTranslate.Size = new Size(812, 501);
            tabTranslate.TabIndex = 0;
            tabTranslate.Text = "Übersetzer";
            tabTranslate.UseVisualStyleBackColor = true;
            //
            // lblInput
            //
            lblInput.AutoSize = true;
            lblInput.Location = new Point(12, 12);
            lblInput.Name = "lblInput";
            lblInput.Size = new Size(380, 20);
            lblInput.Text = "Eingabe (Text oder Morsecode mit . und -, Wörter mit / trennen):";
            //
            // txtInput
            //
            txtInput.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtInput.Font = new Font("Segoe UI", 11F);
            txtInput.Location = new Point(12, 36);
            txtInput.Multiline = true;
            txtInput.Name = "txtInput";
            txtInput.ScrollBars = ScrollBars.Vertical;
            txtInput.Size = new Size(786, 130);
            txtInput.TabIndex = 0;
            //
            // btnTranslate
            //
            btnTranslate.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnTranslate.Location = new Point(12, 176);
            btnTranslate.Name = "btnTranslate";
            btnTranslate.Size = new Size(130, 34);
            btnTranslate.TabIndex = 1;
            btnTranslate.Text = "Übersetzen";
            btnTranslate.UseVisualStyleBackColor = true;
            btnTranslate.Click += btnTranslate_Click;
            //
            // btnToMorse
            //
            btnToMorse.Location = new Point(150, 176);
            btnToMorse.Name = "btnToMorse";
            btnToMorse.Size = new Size(130, 34);
            btnToMorse.TabIndex = 2;
            btnToMorse.Text = "Text → Morse";
            btnToMorse.UseVisualStyleBackColor = true;
            btnToMorse.Click += btnToMorse_Click;
            //
            // btnToText
            //
            btnToText.Location = new Point(288, 176);
            btnToText.Name = "btnToText";
            btnToText.Size = new Size(130, 34);
            btnToText.TabIndex = 3;
            btnToText.Text = "Morse → Text";
            btnToText.UseVisualStyleBackColor = true;
            btnToText.Click += btnToText_Click;
            //
            // btnPlay
            //
            btnPlay.Location = new Point(426, 176);
            btnPlay.Name = "btnPlay";
            btnPlay.Size = new Size(120, 34);
            btnPlay.TabIndex = 4;
            btnPlay.Text = "▶ Abspielen";
            btnPlay.UseVisualStyleBackColor = true;
            btnPlay.Click += btnPlay_Click;
            //
            // btnStop
            //
            btnStop.Location = new Point(554, 176);
            btnStop.Name = "btnStop";
            btnStop.Size = new Size(100, 34);
            btnStop.TabIndex = 5;
            btnStop.Text = "■ Stopp";
            btnStop.UseVisualStyleBackColor = true;
            btnStop.Click += btnStop_Click;
            //
            // btnClear
            //
            btnClear.Location = new Point(662, 176);
            btnClear.Name = "btnClear";
            btnClear.Size = new Size(100, 34);
            btnClear.TabIndex = 6;
            btnClear.Text = "Leeren";
            btnClear.UseVisualStyleBackColor = true;
            btnClear.Click += btnClear_Click;
            //
            // lblOutput
            //
            lblOutput.AutoSize = true;
            lblOutput.Location = new Point(12, 226);
            lblOutput.Name = "lblOutput";
            lblOutput.Size = new Size(64, 20);
            lblOutput.Text = "Ausgabe:";
            //
            // btnSwap
            //
            btnSwap.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSwap.Location = new Point(560, 220);
            btnSwap.Name = "btnSwap";
            btnSwap.Size = new Size(120, 30);
            btnSwap.TabIndex = 7;
            btnSwap.Text = "⇅ Tauschen";
            btnSwap.UseVisualStyleBackColor = true;
            btnSwap.Click += btnSwap_Click;
            //
            // btnCopy
            //
            btnCopy.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCopy.Location = new Point(688, 220);
            btnCopy.Name = "btnCopy";
            btnCopy.Size = new Size(110, 30);
            btnCopy.TabIndex = 8;
            btnCopy.Text = "Kopieren";
            btnCopy.UseVisualStyleBackColor = true;
            btnCopy.Click += btnCopy_Click;
            //
            // txtOutput
            //
            txtOutput.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtOutput.BackColor = SystemColors.Window;
            txtOutput.Font = new Font("Consolas", 13F);
            txtOutput.Location = new Point(12, 256);
            txtOutput.Multiline = true;
            txtOutput.Name = "txtOutput";
            txtOutput.ReadOnly = true;
            txtOutput.ScrollBars = ScrollBars.Vertical;
            txtOutput.Size = new Size(786, 232);
            txtOutput.TabIndex = 9;
            //
            // tabKey
            //
            tabKey.Controls.Add(lblKeyHelp);
            tabKey.Controls.Add(pnlKey);
            tabKey.Controls.Add(lblCurrentCodeCaption);
            tabKey.Controls.Add(lblCurrentCode);
            tabKey.Controls.Add(lblKeyMorseCaption);
            tabKey.Controls.Add(txtKeyMorse);
            tabKey.Controls.Add(lblKeyTextCaption);
            tabKey.Controls.Add(txtKeyText);
            tabKey.Controls.Add(btnKeyClear);
            tabKey.Controls.Add(btnKeyBack);
            tabKey.Controls.Add(btnKeyTransfer);
            tabKey.Controls.Add(chkKeySound);
            tabKey.Location = new Point(4, 29);
            tabKey.Name = "tabKey";
            tabKey.Padding = new Padding(3);
            tabKey.Size = new Size(812, 501);
            tabKey.TabIndex = 1;
            tabKey.Text = "Leertasten-Eingabe";
            tabKey.UseVisualStyleBackColor = true;
            //
            // lblKeyHelp
            //
            lblKeyHelp.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblKeyHelp.Location = new Point(12, 10);
            lblKeyHelp.Name = "lblKeyHelp";
            lblKeyHelp.Size = new Size(786, 46);
            lblKeyHelp.Text = "LEERTASTE gedrückt halten: kurz = Punkt (·), lang = Strich (−). Eine kurze Pause (3 Punkte lang) beendet den Buchstaben, eine lange Pause (7 Punkte lang) das Wort. Das Tempo stellst du oben ein.";
            //
            // pnlKey
            //
            pnlKey.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            pnlKey.BackColor = Color.Gainsboro;
            pnlKey.BorderStyle = BorderStyle.FixedSingle;
            pnlKey.Controls.Add(lblKeyState);
            pnlKey.Location = new Point(12, 60);
            pnlKey.Name = "pnlKey";
            pnlKey.Size = new Size(786, 100);
            pnlKey.TabIndex = 0;
            //
            // lblKeyState
            //
            lblKeyState.Dock = DockStyle.Fill;
            lblKeyState.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            lblKeyState.ForeColor = Color.DimGray;
            lblKeyState.Location = new Point(0, 0);
            lblKeyState.Name = "lblKeyState";
            lblKeyState.Size = new Size(784, 98);
            lblKeyState.Text = "LEERTASTE";
            lblKeyState.TextAlign = ContentAlignment.MiddleCenter;
            //
            // lblCurrentCodeCaption
            //
            lblCurrentCodeCaption.AutoSize = true;
            lblCurrentCodeCaption.Location = new Point(12, 180);
            lblCurrentCodeCaption.Name = "lblCurrentCodeCaption";
            lblCurrentCodeCaption.Size = new Size(145, 20);
            lblCurrentCodeCaption.Text = "Aktueller Buchstabe:";
            //
            // lblCurrentCode
            //
            lblCurrentCode.AutoSize = true;
            lblCurrentCode.Font = new Font("Consolas", 20F, FontStyle.Bold);
            lblCurrentCode.ForeColor = Color.DarkBlue;
            lblCurrentCode.Location = new Point(170, 170);
            lblCurrentCode.Name = "lblCurrentCode";
            lblCurrentCode.Size = new Size(0, 40);
            //
            // lblKeyMorseCaption
            //
            lblKeyMorseCaption.AutoSize = true;
            lblKeyMorseCaption.Location = new Point(12, 222);
            lblKeyMorseCaption.Name = "lblKeyMorseCaption";
            lblKeyMorseCaption.Size = new Size(82, 20);
            lblKeyMorseCaption.Text = "Morsecode:";
            //
            // txtKeyMorse
            //
            txtKeyMorse.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtKeyMorse.BackColor = SystemColors.Window;
            txtKeyMorse.Font = new Font("Consolas", 13F);
            txtKeyMorse.Location = new Point(12, 246);
            txtKeyMorse.Multiline = true;
            txtKeyMorse.Name = "txtKeyMorse";
            txtKeyMorse.ReadOnly = true;
            txtKeyMorse.ScrollBars = ScrollBars.Vertical;
            txtKeyMorse.Size = new Size(786, 80);
            txtKeyMorse.TabIndex = 1;
            txtKeyMorse.TabStop = false;
            //
            // lblKeyTextCaption
            //
            lblKeyTextCaption.AutoSize = true;
            lblKeyTextCaption.Location = new Point(12, 336);
            lblKeyTextCaption.Name = "lblKeyTextCaption";
            lblKeyTextCaption.Size = new Size(40, 20);
            lblKeyTextCaption.Text = "Text:";
            //
            // txtKeyText
            //
            txtKeyText.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtKeyText.BackColor = SystemColors.Window;
            txtKeyText.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            txtKeyText.Location = new Point(12, 360);
            txtKeyText.Multiline = true;
            txtKeyText.Name = "txtKeyText";
            txtKeyText.ReadOnly = true;
            txtKeyText.Size = new Size(786, 60);
            txtKeyText.TabIndex = 2;
            txtKeyText.TabStop = false;
            //
            // btnKeyClear
            //
            btnKeyClear.Location = new Point(12, 434);
            btnKeyClear.Name = "btnKeyClear";
            btnKeyClear.Size = new Size(110, 34);
            btnKeyClear.TabIndex = 3;
            btnKeyClear.TabStop = false;
            btnKeyClear.Text = "Leeren";
            btnKeyClear.UseVisualStyleBackColor = true;
            btnKeyClear.Click += btnKeyClear_Click;
            //
            // btnKeyBack
            //
            btnKeyBack.Location = new Point(130, 434);
            btnKeyBack.Name = "btnKeyBack";
            btnKeyBack.Size = new Size(170, 34);
            btnKeyBack.TabIndex = 4;
            btnKeyBack.TabStop = false;
            btnKeyBack.Text = "⌫ Letztes löschen";
            btnKeyBack.UseVisualStyleBackColor = true;
            btnKeyBack.Click += btnKeyBack_Click;
            //
            // btnKeyTransfer
            //
            btnKeyTransfer.Location = new Point(308, 434);
            btnKeyTransfer.Name = "btnKeyTransfer";
            btnKeyTransfer.Size = new Size(210, 34);
            btnKeyTransfer.TabIndex = 5;
            btnKeyTransfer.TabStop = false;
            btnKeyTransfer.Text = "In Übersetzer übernehmen";
            btnKeyTransfer.UseVisualStyleBackColor = true;
            btnKeyTransfer.Click += btnKeyTransfer_Click;
            //
            // chkKeySound
            //
            chkKeySound.AutoSize = true;
            chkKeySound.Checked = true;
            chkKeySound.CheckState = CheckState.Checked;
            chkKeySound.Location = new Point(540, 440);
            chkKeySound.Name = "chkKeySound";
            chkKeySound.Size = new Size(150, 24);
            chkKeySound.TabIndex = 6;
            chkKeySound.TabStop = false;
            chkKeySound.Text = "Ton beim Drücken";
            chkKeySound.UseVisualStyleBackColor = true;
            //
            // tabLearn
            //
            tabLearn.Controls.Add(lblLearnMode);
            tabLearn.Controls.Add(cmbLearnMode);
            tabLearn.Controls.Add(lblLevel);
            tabLearn.Controls.Add(cmbLevel);
            tabLearn.Controls.Add(lblLearnHelp);
            tabLearn.Controls.Add(lblChallenge);
            tabLearn.Controls.Add(lblLearnInput);
            tabLearn.Controls.Add(lblAnswerCaption);
            tabLearn.Controls.Add(txtLearnAnswer);
            tabLearn.Controls.Add(btnLearnStart);
            tabLearn.Controls.Add(btnLearnRepeat);
            tabLearn.Controls.Add(btnLearnHint);
            tabLearn.Controls.Add(btnLearnReset);
            tabLearn.Controls.Add(lblFeedback);
            tabLearn.Controls.Add(lblScore);
            tabLearn.Controls.Add(lblWeak);
            tabLearn.Location = new Point(4, 29);
            tabLearn.Name = "tabLearn";
            tabLearn.Padding = new Padding(3);
            tabLearn.Size = new Size(812, 501);
            tabLearn.TabIndex = 2;
            tabLearn.Text = "Lern-Modus";
            tabLearn.UseVisualStyleBackColor = true;
            //
            // lblLearnMode
            //
            lblLearnMode.AutoSize = true;
            lblLearnMode.Location = new Point(12, 15);
            lblLearnMode.Name = "lblLearnMode";
            lblLearnMode.Size = new Size(54, 20);
            lblLearnMode.Text = "Modus:";
            //
            // cmbLearnMode
            //
            cmbLearnMode.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbLearnMode.Location = new Point(72, 12);
            cmbLearnMode.Name = "cmbLearnMode";
            cmbLearnMode.Size = new Size(290, 28);
            cmbLearnMode.TabIndex = 0;
            cmbLearnMode.TabStop = false;
            cmbLearnMode.SelectedIndexChanged += cmbLearnMode_SelectedIndexChanged;
            //
            // lblLevel
            //
            lblLevel.AutoSize = true;
            lblLevel.Location = new Point(378, 15);
            lblLevel.Name = "lblLevel";
            lblLevel.Size = new Size(44, 20);
            lblLevel.Text = "Stufe:";
            //
            // cmbLevel
            //
            cmbLevel.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbLevel.Location = new Point(428, 12);
            cmbLevel.Name = "cmbLevel";
            cmbLevel.Size = new Size(370, 28);
            cmbLevel.TabIndex = 1;
            cmbLevel.TabStop = false;
            cmbLevel.SelectedIndexChanged += cmbLevel_SelectedIndexChanged;
            //
            // lblLearnHelp
            //
            lblLearnHelp.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblLearnHelp.ForeColor = SystemColors.GrayText;
            lblLearnHelp.Location = new Point(12, 48);
            lblLearnHelp.Name = "lblLearnHelp";
            lblLearnHelp.Size = new Size(786, 40);
            lblLearnHelp.Text = "Wähle Modus und Stufe und klicke auf „Start“.";
            //
            // lblChallenge
            //
            lblChallenge.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblChallenge.BackColor = Color.WhiteSmoke;
            lblChallenge.BorderStyle = BorderStyle.FixedSingle;
            lblChallenge.Font = new Font("Segoe UI", 48F, FontStyle.Bold);
            lblChallenge.Location = new Point(12, 92);
            lblChallenge.Name = "lblChallenge";
            lblChallenge.Size = new Size(786, 110);
            lblChallenge.Text = "–";
            lblChallenge.TextAlign = ContentAlignment.MiddleCenter;
            //
            // lblLearnInput
            //
            lblLearnInput.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblLearnInput.Font = new Font("Consolas", 22F, FontStyle.Bold);
            lblLearnInput.ForeColor = Color.DarkBlue;
            lblLearnInput.Location = new Point(12, 208);
            lblLearnInput.Name = "lblLearnInput";
            lblLearnInput.Size = new Size(786, 44);
            lblLearnInput.TextAlign = ContentAlignment.MiddleCenter;
            //
            // lblAnswerCaption
            //
            lblAnswerCaption.AutoSize = true;
            lblAnswerCaption.Location = new Point(12, 265);
            lblAnswerCaption.Name = "lblAnswerCaption";
            lblAnswerCaption.Size = new Size(212, 20);
            lblAnswerCaption.Text = "Deine Antwort (Zeichen tippen):";
            //
            // txtLearnAnswer
            //
            txtLearnAnswer.CharacterCasing = CharacterCasing.Upper;
            txtLearnAnswer.Font = new Font("Consolas", 14F, FontStyle.Bold);
            txtLearnAnswer.Location = new Point(240, 258);
            txtLearnAnswer.MaxLength = 1;
            txtLearnAnswer.Name = "txtLearnAnswer";
            txtLearnAnswer.Size = new Size(50, 35);
            txtLearnAnswer.TabIndex = 2;
            txtLearnAnswer.TextAlign = HorizontalAlignment.Center;
            txtLearnAnswer.TextChanged += txtLearnAnswer_TextChanged;
            //
            // btnLearnStart
            //
            btnLearnStart.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnLearnStart.Location = new Point(12, 306);
            btnLearnStart.Name = "btnLearnStart";
            btnLearnStart.Size = new Size(150, 36);
            btnLearnStart.TabIndex = 3;
            btnLearnStart.TabStop = false;
            btnLearnStart.Text = "Start / Weiter";
            btnLearnStart.UseVisualStyleBackColor = true;
            btnLearnStart.Click += btnLearnStart_Click;
            //
            // btnLearnRepeat
            //
            btnLearnRepeat.Location = new Point(170, 306);
            btnLearnRepeat.Name = "btnLearnRepeat";
            btnLearnRepeat.Size = new Size(160, 36);
            btnLearnRepeat.TabIndex = 4;
            btnLearnRepeat.TabStop = false;
            btnLearnRepeat.Text = "🔊 Nochmal hören";
            btnLearnRepeat.UseVisualStyleBackColor = true;
            btnLearnRepeat.Click += btnLearnRepeat_Click;
            //
            // btnLearnHint
            //
            btnLearnHint.Location = new Point(338, 306);
            btnLearnHint.Name = "btnLearnHint";
            btnLearnHint.Size = new Size(100, 36);
            btnLearnHint.TabIndex = 5;
            btnLearnHint.TabStop = false;
            btnLearnHint.Text = "💡 Tipp";
            btnLearnHint.UseVisualStyleBackColor = true;
            btnLearnHint.Click += btnLearnHint_Click;
            //
            // btnLearnReset
            //
            btnLearnReset.Location = new Point(446, 306);
            btnLearnReset.Name = "btnLearnReset";
            btnLearnReset.Size = new Size(180, 36);
            btnLearnReset.TabIndex = 6;
            btnLearnReset.TabStop = false;
            btnLearnReset.Text = "Punkte zurücksetzen";
            btnLearnReset.UseVisualStyleBackColor = true;
            btnLearnReset.Click += btnLearnReset_Click;
            //
            // lblFeedback
            //
            lblFeedback.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblFeedback.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            lblFeedback.Location = new Point(12, 354);
            lblFeedback.Name = "lblFeedback";
            lblFeedback.Size = new Size(786, 36);
            //
            // lblScore
            //
            lblScore.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblScore.Font = new Font("Segoe UI", 10F);
            lblScore.Location = new Point(12, 400);
            lblScore.Name = "lblScore";
            lblScore.Size = new Size(786, 26);
            //
            // lblWeak
            //
            lblWeak.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblWeak.ForeColor = SystemColors.GrayText;
            lblWeak.Location = new Point(12, 430);
            lblWeak.Name = "lblWeak";
            lblWeak.Size = new Size(786, 26);
            //
            // statusStrip
            //
            statusStrip.ImageScalingSize = new Size(20, 20);
            statusStrip.Items.AddRange(new ToolStripItem[] { lblStatus });
            statusStrip.Location = new Point(0, 578);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new Size(820, 26);
            statusStrip.TabIndex = 4;
            //
            // lblStatus
            //
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(50, 20);
            lblStatus.Text = "Bereit.";
            //
            // timerKeyer
            //
            timerKeyer.Interval = 15;
            timerKeyer.Tick += timerKeyer_Tick;
            //
            // timerLearnNext
            //
            timerLearnNext.Interval = 1400;
            timerLearnNext.Tick += timerLearnNext_Tick;
            //
            // Form1
            //
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(820, 604);
            Controls.Add(tabMain);
            Controls.Add(panelTop);
            Controls.Add(statusStrip);
            KeyPreview = true;
            MinimumSize = new Size(700, 560);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Morse-Übersetzer";
            Deactivate += Form1_Deactivate;
            Load += Form1_Load;
            KeyDown += Form1_KeyDown;
            KeyUp += Form1_KeyUp;
            panelTop.ResumeLayout(false);
            panelTop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numWpm).EndInit();
            ((System.ComponentModel.ISupportInitialize)numFreq).EndInit();
            tabMain.ResumeLayout(false);
            tabTranslate.ResumeLayout(false);
            tabTranslate.PerformLayout();
            tabKey.ResumeLayout(false);
            tabKey.PerformLayout();
            pnlKey.ResumeLayout(false);
            tabLearn.ResumeLayout(false);
            tabLearn.PerformLayout();
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel panelTop;
        private Label lblWpm;
        private NumericUpDown numWpm;
        private Label lblFreq;
        private NumericUpDown numFreq;
        private Label lblUnit;
        private TabControl tabMain;
        private TabPage tabTranslate;
        private Label lblInput;
        private TextBox txtInput;
        private Button btnTranslate;
        private Button btnToMorse;
        private Button btnToText;
        private Button btnPlay;
        private Button btnStop;
        private Button btnClear;
        private Label lblOutput;
        private Button btnSwap;
        private Button btnCopy;
        private TextBox txtOutput;
        private TabPage tabKey;
        private Label lblKeyHelp;
        private Panel pnlKey;
        private Label lblKeyState;
        private Label lblCurrentCodeCaption;
        private Label lblCurrentCode;
        private Label lblKeyMorseCaption;
        private TextBox txtKeyMorse;
        private Label lblKeyTextCaption;
        private TextBox txtKeyText;
        private Button btnKeyClear;
        private Button btnKeyBack;
        private Button btnKeyTransfer;
        private CheckBox chkKeySound;
        private TabPage tabLearn;
        private Label lblLearnMode;
        private ComboBox cmbLearnMode;
        private Label lblLevel;
        private ComboBox cmbLevel;
        private Label lblLearnHelp;
        private Label lblChallenge;
        private Label lblLearnInput;
        private Label lblAnswerCaption;
        private TextBox txtLearnAnswer;
        private Button btnLearnStart;
        private Button btnLearnRepeat;
        private Button btnLearnHint;
        private Button btnLearnReset;
        private Label lblFeedback;
        private Label lblScore;
        private Label lblWeak;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel lblStatus;
        private System.Windows.Forms.Timer timerKeyer;
        private System.Windows.Forms.Timer timerLearnNext;
    }
}
