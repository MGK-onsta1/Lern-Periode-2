using System.Net;
using System.Net.Sockets;

namespace Morse
{
    /// <summary>
    /// Tab „Chat“: Morse-Nachrichten mit Freunden austauschen.
    ///   • WLAN:     ChatNode (TCP, AES-GCM-verschlüsselt) + LanDiscovery (Freunde automatisch finden)
    ///   • Browser:  WebBridge (Mini-Webserver, Handy öffnet Link oder scannt QR-Code)
    ///   • Internet: TunnelManager (öffentlicher Link über cloudflared)
    /// Der Verlauf wird mit ChatHistory gespeichert.
    /// Der Tab wird im Code gebaut (nicht im Designer), weil er recht groß ist.
    /// </summary>
    public partial class Form1
    {
        private readonly ChatNode _node = new();
        private readonly LanDiscovery _discovery = new();
        private readonly WebBridge _web = new();
        private readonly TunnelManager _tunnel = new();
        private readonly ChatHistory _history = new();
        private readonly Dictionary<string, FoundPeer> _found = new();
        private readonly System.Windows.Forms.Timer _foundCleanup = new() { Interval = 3000 };
        private bool _tunnelBusy;

        private TabPage tabChat = null!;
        private TextBox txtChatName = null!, txtChatPassword = null!, txtHost = null!, txtWebLinks = null!, txtChatInput = null!;
        private Button btnOnline = null!, btnConnect = null!, btnQr = null!, btnTunnel = null!, btnSend = null!;
        private Label lblMyIp = null!, lblChatPreview = null!;
        private CheckBox chkLanVisible = null!, chkWeb = null!, chkPlayIncoming = null!, chkChatKey = null!;
        private readonly List<string> _chatKeyTokens = new();   // mit der Leertaste gemorste Buchstaben-Codes
        private ListBox listFound = null!, listPeers = null!;
        private RichTextBox rtbChat = null!;

        /// <summary>Ein im WLAN gefundener Freund.</summary>
        private sealed record FoundPeer(string Name, IPAddress Ip, int Port, DateTime Seen)
        {
            public string Key => $"{Ip}:{Port}";
            public override string ToString() => $"{Name}  ({Ip})";
        }

        // ================================================================ Aufbau

        private void InitChat()
        {
            BuildChatTab();
            WireChatEvents();

            _history.Load();
            if (_history.Entries.Count > 0)
            {
                foreach (var entry in _history.Entries) WriteChatLine(entry);
                WriteSeparator("— gespeicherter Verlauf —");
            }
            WriteChatLine(new ChatHistory.Entry(DateTime.Now, null, false,
                "Gib deinen Namen und ein Passwort ein und klicke auf „Online gehen“. Deine Freunde brauchen dasselbe Passwort."));
            UpdateWeb();
        }

        private void BuildChatTab()
        {
            tabChat = new TabPage { Text = "Chat", UseVisualStyleBackColor = true, Padding = new Padding(3) };

            // ---------- linke Spalte: Verbindung ----------
            var left = new Panel { Dock = DockStyle.Left, Width = 300, AutoScroll = true, Padding = new Padding(0, 0, 8, 0) };
            int y = 6;
            Label Caption(string text)
            {
                var l = new Label { Text = text, AutoSize = true, Location = new Point(8, y) };
                left.Controls.Add(l);
                y += 22;
                return l;
            }

            Caption("Dein Name:");
            txtChatName = new TextBox { Location = new Point(8, y), Width = 256, Text = _settings.ChatName, MaxLength = 30 };
            left.Controls.Add(txtChatName); y += 34;

            Caption("Passwort (alle brauchen dasselbe):");
            txtChatPassword = new TextBox { Location = new Point(8, y), Width = 256, UseSystemPasswordChar = true };
            left.Controls.Add(txtChatPassword); y += 34;

            btnOnline = new Button { Text = "Online gehen", Location = new Point(8, y), Size = new Size(256, 34), Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            left.Controls.Add(btnOnline); y += 42;

            lblMyIp = new Label { Text = "Deine IP: –", AutoSize = true, Location = new Point(8, y), ForeColor = Color.DimGray };
            left.Controls.Add(lblMyIp); y += 28;

            Caption("Freund verbinden (IP-Adresse):");
            txtHost = new TextBox { Location = new Point(8, y), Width = 164, PlaceholderText = "z. B. 192.168.1.23" };
            btnConnect = new Button { Text = "Verbinden", Location = new Point(178, y - 2), Size = new Size(86, 30) };
            left.Controls.Add(txtHost);
            left.Controls.Add(btnConnect); y += 36;

            chkLanVisible = new CheckBox { Text = "Im WLAN sichtbar + suchen", Checked = true, AutoSize = true, Location = new Point(8, y) };
            left.Controls.Add(chkLanVisible); y += 26;

            Caption("Im WLAN gefunden (Doppelklick):");
            listFound = new ListBox { Location = new Point(8, y), Size = new Size(256, 64), IntegralHeight = false };
            left.Controls.Add(listFound); y += 72;

            Caption("Verbunden:");
            listPeers = new ListBox { Location = new Point(8, y), Size = new Size(256, 56), IntegralHeight = false, SelectionMode = SelectionMode.None };
            left.Controls.Add(listPeers); y += 66;

            chkWeb = new CheckBox { Text = "Browser-Zugang (Handy / PC)", Checked = true, AutoSize = true, Location = new Point(8, y) };
            left.Controls.Add(chkWeb); y += 26;

            txtWebLinks = new TextBox
            {
                Location = new Point(8, y), Size = new Size(256, 50), Multiline = true, ReadOnly = true,
                BackColor = SystemColors.Window, Font = new Font("Consolas", 8.5F), ScrollBars = ScrollBars.Vertical,
            };
            left.Controls.Add(txtWebLinks); y += 58;

            btnQr = new Button { Text = "📱 QR-Code", Location = new Point(8, y), Size = new Size(110, 32) };
            btnTunnel = new Button { Text = "🌍 Internet-Link erstellen", Location = new Point(122, y), Size = new Size(142, 32) };
            left.Controls.Add(btnQr);
            left.Controls.Add(btnTunnel);

            // ---------- rechte Seite: Verlauf + Eingabe ----------
            rtbChat = new RichTextBox
            {
                Dock = DockStyle.Fill, ReadOnly = true, BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 10.5F), DetectUrls = true,
            };

            var bottom = new Panel { Dock = DockStyle.Bottom, Height = 132 };
            txtChatInput = new TextBox
            {
                Location = new Point(0, 8), Width = bottom.Width - 110, Font = new Font("Segoe UI", 11F),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                PlaceholderText = "Text oder Morsecode eingeben, Enter = senden",
            };
            btnSend = new Button
            {
                Text = "Senden ➤", Size = new Size(102, 32), Location = new Point(bottom.Width - 102, 6),
                Anchor = AnchorStyles.Top | AnchorStyles.Right, Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            };
            lblChatPreview = new Label
            {
                Location = new Point(0, 42), AutoSize = false, Height = 22, Width = bottom.Width,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Font = new Font("Consolas", 10F), ForeColor = Color.DimGray, AutoEllipsis = true,
            };
            chkPlayIncoming = new CheckBox { Text = "Ton bei neuen Nachrichten", Checked = _settings.PlayIncoming, AutoSize = true, Location = new Point(0, 70) };
            chkChatKey = new CheckBox { Text = "🔑 Mit Leertaste morsen", AutoSize = true, Location = new Point(190, 70) };
            var btnClearHistory = new Button
            {
                Text = "🗑 Verlauf löschen", Size = new Size(130, 30), Location = new Point(bottom.Width - 130, 98),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
            };
            var btnExport = new Button
            {
                Text = "💾 Als Text…", Size = new Size(100, 30), Location = new Point(bottom.Width - 236, 98),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
            };
            var tip = new ToolTip();
            tip.SetToolTip(btnExport, "Den Chatverlauf als .txt-Datei speichern");
            tip.SetToolTip(chkChatKey, "Leertaste kurz = Punkt, lang = Strich. Rücktaste = letzten Buchstaben löschen, Enter = senden.");
            btnExport.Click += (_, _) => ExportHistory();
            btnClearHistory.Click += (_, _) => ClearHistory();
            bottom.Controls.AddRange(new Control[] { txtChatInput, btnSend, lblChatPreview, chkPlayIncoming, chkChatKey, btnExport, btnClearHistory });

            var right = new Panel { Dock = DockStyle.Fill, Padding = new Padding(8, 4, 4, 0) };
            right.Controls.Add(rtbChat);
            right.Controls.Add(bottom);

            tabChat.Controls.Add(right);
            tabChat.Controls.Add(left);
            tabMain.TabPages.Add(tabChat);
        }

        private void WireChatEvents()
        {
            btnOnline.Click += (_, _) => ToggleOnline();
            btnConnect.Click += async (_, _) => await ConnectToHost();
            txtHost.KeyDown += async (_, e) =>
            {
                if (e.KeyCode != Keys.Enter) return;
                e.SuppressKeyPress = true;
                await ConnectToHost();
            };
            listFound.DoubleClick += async (_, _) =>
            {
                if (listFound.SelectedItem is FoundPeer fp) await Connect(fp.Ip.ToString(), fp.Port);
            };
            chkLanVisible.CheckedChanged += (_, _) => UpdateDiscovery();
            chkWeb.CheckedChanged += (_, _) => UpdateWeb();
            btnQr.Click += (_, _) => ShowQr();
            btnTunnel.Click += async (_, _) => await ToggleTunnel();
            btnSend.Click += (_, _) => SendChat();
            txtChatInput.KeyDown += (_, e) =>
            {
                if (e.KeyCode == Keys.Back && chkChatKey.Checked)
                {
                    e.SuppressKeyPress = true;
                    ChatKeyBackspace();
                    return;
                }
                if (e.KeyCode != Keys.Enter) return;
                e.SuppressKeyPress = true;
                SendChat();
            };
            chkChatKey.CheckedChanged += (_, _) => SetChatKeyMode(chkChatKey.Checked);
            txtChatInput.TextChanged += (_, _) => UpdateChatPreview();
            txtChatName.TextChanged += (_, _) => { _settings.ChatName = txtChatName.Text.Trim(); _settings.Save(); };
            chkPlayIncoming.CheckedChanged += (_, _) => { _settings.PlayIncoming = chkPlayIncoming.Checked; _settings.Save(); };

            // ---------- Freunde mit dem Programm (TCP) ----------
            _node.Log += msg => Ui(() => SystemMessage(msg));
            _node.PeerConnected += p => Ui(() =>
            {
                SystemMessage($"Verbunden mit {p.Name} ({p.Endpoint}).");
                _web.Notice($"{p.Name} ist dabei.");
                RefreshPeers();
                _web.UpdateOnline();
            });
            _node.PeerDisconnected += p => Ui(() =>
            {
                SystemMessage($"{p.Name} hat die Verbindung getrennt.");
                _web.Notice($"{p.Name} ist gegangen.");
                RefreshPeers();
                _web.UpdateOnline();
            });
            _node.MessageReceived += (p, name, morse) => Ui(() =>
            {
                ChatMessage(name, false, morse);
                _web.Broadcast(name, morse); // an Browser-Freunde weiterreichen
                if (chkPlayIncoming.Checked) _audio.Play(morse);
            });

            // ---------- Freunde im Browser ----------
            _web.OnlineNames = () => _node.Peers.Select(p => p.Name).Concat(_web.Clients.Select(c => c.Name))
                .OfType<string>().Prepend(_node.Name);
            _web.Log += msg => Ui(() => SystemMessage(msg));
            _web.ClientJoined += c => Ui(() =>
            {
                SystemMessage($"🌐 {c.Name} ist per Browser beigetreten.");
                _web.Notice($"{c.Name} ist dabei.", except: c);
                RefreshPeers();
                _web.UpdateOnline();
            });
            _web.ClientLeft += c => Ui(() =>
            {
                SystemMessage($"🌐 {c.Name} hat den Browser-Chat verlassen.");
                _web.Notice($"{c.Name} ist gegangen.");
                RefreshPeers();
                _web.UpdateOnline();
            });
            _web.MessageReceived += (c, morse) => Ui(() =>
            {
                var name = c.Name ?? "?";
                ChatMessage(name + " 🌐", false, morse);
                _node.Broadcast(morse, from: name);       // an Programm-Freunde weiterreichen
                _web.Broadcast(name, morse, except: c);   // an die anderen Browser-Freunde
                if (chkPlayIncoming.Checked) _audio.Play(morse);
            });

            // ---------- Internet-Link ----------
            _tunnel.UrlReady += url => Ui(() =>
            {
                SystemMessage("🌍 Internet-Link bereit: " + url + "  – den kannst du jetzt an deine Freunde schicken.");
                UpdateWeb();
            });
            _tunnel.Stopped += reason => Ui(() =>
            {
                if (reason != null) SystemMessage(reason);
                UpdateWeb();
            });

            // ---------- WLAN-Suche ----------
            _discovery.PeerFound += (name, ip, port) => Ui(() =>
            {
                var fp = new FoundPeer(name, ip, port, DateTime.Now);
                _found[fp.Key] = fp;
                RefreshFound();
            });
            _foundCleanup.Tick += (_, _) =>
            {
                // Wer sich 7 Sekunden nicht gemeldet hat, ist nicht mehr da
                var old = _found.Values.Where(f => DateTime.Now - f.Seen > TimeSpan.FromSeconds(7)).Select(f => f.Key).ToList();
                if (old.Count == 0) return;
                foreach (var k in old) _found.Remove(k);
                RefreshFound();
            };
            _foundCleanup.Start();
        }

        /// <summary>Netzwerk-Ereignisse kommen evtl. von einem anderen Thread → auf den UI-Thread wechseln.</summary>
        private void Ui(Action action)
        {
            if (IsDisposed || Disposing) return;
            if (InvokeRequired) BeginInvoke(action);
            else action();
        }

        private void ShutdownChat()
        {
            _foundCleanup.Stop();
            _history.Save();
            _discovery.Dispose();
            _node.Dispose();
            _web.Dispose();
            _tunnel.Dispose();
        }

        // ================================================================ Online / Verbinden

        private void ToggleOnline()
        {
            if (_node.IsRunning)
            {
                _discovery.Stop();
                _node.Stop();
                UpdateWeb();
                _found.Clear();
                RefreshFound();
                RefreshPeers();
                SetOnlineUi(false);
                SystemMessage("Offline.");
                return;
            }
            GoOnline();
        }

        private bool GoOnline()
        {
            if (_node.IsRunning) return true;
            if (string.IsNullOrWhiteSpace(txtChatName.Text)) { SystemMessage("Bitte zuerst einen Namen eingeben."); return false; }

            _node.Name = txtChatName.Text.Trim();
            _node.SetPassword(txtChatPassword.Text);
            try { _node.Start(ChatNode.DefaultPort); }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.AddressAlreadyInUse)
            {
                SystemMessage("Konnte nicht online gehen: Der Morse-Übersetzer ist schon in einem anderen Fenster online. Schliesse das andere Fenster.");
                return false;
            }
            catch (SocketException ex) { SystemMessage("Konnte nicht online gehen: " + ex.Message); return false; }

            SetOnlineUi(true);
            lblMyIp.Text = "Deine IP: " + (LocalAddresses().FirstOrDefault() ?? "–");
            SystemMessage($"Online. Deine Adresse für Freunde: {string.Join(", ", LocalAddresses())}");
            if (txtChatPassword.TextLength == 0)
                SystemMessage("⚠ Kein Passwort gesetzt – jeder im Netz könnte mitlesen. Vereinbart ein Passwort!");
            _web.SetPassword(txtChatPassword.Text);
            UpdateDiscovery();
            UpdateWeb();
            return true;
        }

        private void SetOnlineUi(bool online)
        {
            btnOnline.Text = online ? "Offline gehen" : "Online gehen";
            btnOnline.BackColor = online ? Color.FromArgb(220, 245, 220) : SystemColors.Control;
            txtChatName.Enabled = txtChatPassword.Enabled = !online;
            if (!online) lblMyIp.Text = "Deine IP: –";
        }

        private void UpdateDiscovery()
        {
            if (_node.IsRunning && chkLanVisible.Checked)
            {
                try { _discovery.Start(() => _node.Name, _node.Port); }
                catch (SocketException ex) { SystemMessage("WLAN-Suche nicht möglich: " + ex.Message); }
            }
            else
            {
                _discovery.Stop();
                _found.Clear();
                RefreshFound();
            }
        }

        private async Task ConnectToHost()
        {
            var input = txtHost.Text.Trim();
            if (input.Length == 0) { SystemMessage("Bitte die IP deines Freundes eingeben."); return; }

            // Erlaubt auch "adresse:port"
            int port = ChatNode.DefaultPort;
            var idx = input.LastIndexOf(':');
            if (idx > 0 && input.IndexOf(':') == idx && int.TryParse(input[(idx + 1)..], out var p))
            {
                port = p;
                input = input[..idx];
            }
            await Connect(input, port);
        }

        private async Task Connect(string host, int port)
        {
            if (!GoOnline()) return;
            SystemMessage($"Verbinde mit {host} …");
            try { await _node.ConnectAsync(host, port); }
            catch (Exception ex) { SystemMessage($"Verbindung fehlgeschlagen: {ex.Message}"); }
        }

        // ================================================================ Browser + Internet

        private void UpdateWeb()
        {
            if (_node.IsRunning && chkWeb.Checked)
            {
                if (!_web.IsRunning)
                {
                    try
                    {
                        _web.Start(WebBridge.DefaultPort);
                        SystemMessage("🌐 Browser-Zugang an. Freunde öffnen den Link links im Browser (oder scannen den QR-Code) und geben das Passwort ein.");
                    }
                    catch (SocketException ex) { SystemMessage("Browser-Zugang nicht möglich: " + ex.Message); }
                }
            }
            else
            {
                _web.Stop();
                _tunnel.Stop();
            }

            var lines = new List<string>();
            if (_tunnel.Url != null) lines.Add("🌍 " + _tunnel.Url);
            else if (_tunnel.IsRunning) lines.Add("🌍 Internet-Link wird erstellt …");
            if (_web.IsRunning) lines.AddRange(LocalAddresses().Select(ip => $"🏠 http://{ip}:{_web.Port}"));
            txtWebLinks.Text = _web.IsRunning ? string.Join(Environment.NewLine, lines)
                : _node.IsRunning ? "Browser-Zugang ist aus." : "Erst „Online gehen“ klicken.";

            if (!_tunnelBusy)
            {
                btnTunnel.Text = _tunnel.IsRunning ? "🌍 Link beenden" : "🌍 Internet-Link";
                btnTunnel.Enabled = _web.IsRunning || _tunnel.IsRunning || !_node.IsRunning;
            }
        }

        private void ShowQr()
        {
            if (!_web.IsRunning) { SystemMessage("Erst „Online gehen“ klicken – dann gibt es einen Link für den QR-Code."); return; }

            var links = new List<(string, string)>();
            if (_tunnel.Url != null) links.Add(("🌍 Internet", _tunnel.Url));
            var lan = LocalAddresses().ToList();
            foreach (var ip in lan)
                links.Add((lan.Count > 1 ? $"🏠 WLAN ({ip})" : "🏠 WLAN", $"http://{ip}:{_web.Port}"));
            if (links.Count == 0) { SystemMessage("Keine Adresse gefunden – bist du mit einem WLAN verbunden?"); return; }

            using var window = new QrWindow(links);
            window.ShowDialog(this);
        }

        private async Task ToggleTunnel()
        {
            if (_tunnel.IsRunning)
            {
                _tunnel.Stop();
                SystemMessage("🌍 Internet-Link beendet.");
                UpdateWeb();
                return;
            }
            if (!GoOnline()) return;
            if (!chkWeb.Checked) chkWeb.Checked = true;
            if (!_web.IsRunning) return;

            if (!TunnelManager.IsInstalled)
            {
                var answer = MessageBox.Show(this,
                    "Für den Internet-Link wird einmalig das kostenlose Programm „cloudflared“ von Cloudflare " +
                    "heruntergeladen (ca. 53 MB, von github.com/cloudflare).\n\nJetzt herunterladen?",
                    "Internet-Link", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (answer != DialogResult.Yes) return;

                _tunnelBusy = true;
                btnTunnel.Enabled = false;
                try
                {
                    var progress = new Progress<int>(p => btnTunnel.Text = $"⬇ Lade … {p} %");
                    await TunnelManager.DownloadAsync(progress, CancellationToken.None);
                    SystemMessage("cloudflared wurde heruntergeladen.");
                }
                catch (Exception ex)
                {
                    SystemMessage("Download fehlgeschlagen: " + ex.Message);
                    return;
                }
                finally
                {
                    _tunnelBusy = false;
                    btnTunnel.Enabled = true;
                    UpdateWeb();
                }
            }

            try
            {
                _tunnel.Start(_web.Port);
                SystemMessage("🌍 Internet-Link wird erstellt – das dauert ein paar Sekunden …");
            }
            catch (Exception ex) { SystemMessage("Internet-Link konnte nicht gestartet werden: " + ex.Message); }
            UpdateWeb();
        }

        // ================================================================ Senden / Anzeigen

        /// <summary>Die Eingabe als Morsecode: Ist sie schon Morse, bleibt sie so, sonst wird übersetzt.</summary>
        private string ChatInputAsMorse()
        {
            string input = txtChatInput.Text.Trim();
            string morse = MorseTable.LooksLikeMorse(input) ? MorseTable.Normalize(input) : MorseTable.TextToMorse(input);
            var tokens = morse.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
            while (tokens.Count > 0 && tokens[^1] == "/") tokens.RemoveAt(tokens.Count - 1);
            return string.Join(" ", tokens);
        }

        private void UpdateChatPreview()
        {
            string input = txtChatInput.Text.Trim();
            if (input.Length == 0) { lblChatPreview.Text = ""; return; }
            lblChatPreview.Text = MorseTable.LooksLikeMorse(input)
                ? "→ " + MorseTable.MorseToText(input)
                : "→ " + MorseTable.Pretty(ChatInputAsMorse());
        }

        private void SendChat()
        {
            string morse = ChatInputAsMorse();
            if (morse.Length == 0) return;

            if (_node.Peers.Count == 0 && _web.Clients.Count == 0)
            {
                SystemMessage("Du bist mit niemandem verbunden.");
                return;
            }

            _node.Broadcast(morse);
            _web.Broadcast(_node.Name, morse);
            ChatMessage("Ich", true, morse);
            txtChatInput.Clear();
            _chatKeyTokens.Clear();
            _keyer.Reset();
        }

        // ================================================================ Morsetaste im Chat

        /// <summary>Soll die Leertaste im Chat-Tab gerade morsen? Nicht, wenn man im Namen/Passwort/IP-Feld tippt.</summary>
        private bool ChatKeyActive
        {
            get
            {
                if (tabMain.SelectedTab != tabChat || !chkChatKey.Checked) return false;
                Control? focused = this;
                while (focused is ContainerControl { ActiveControl: not null } cc) focused = cc.ActiveControl;
                return focused != txtChatName && focused != txtChatPassword && focused != txtHost;
            }
        }

        private void SetChatKeyMode(bool on)
        {
            _keyer.Reset();
            _chatKeyTokens.Clear();
            txtChatInput.Clear();
            txtChatInput.ReadOnly = on;   // im Morse-Modus kommt der Text nur von der Leertaste
            txtChatInput.BackColor = on ? Color.FromArgb(255, 250, 225) : SystemColors.Window;
            txtChatInput.PlaceholderText = on
                ? "LEERTASTE: kurz = ·  lang = −   Rücktaste = löschen, Enter = senden"
                : "Text oder Morsecode eingeben, Enter = senden";
            txtChatInput.Focus();
        }

        /// <summary>Ein Buchstabe oder eine Wortpause ("/") wurde gemorst.</summary>
        private void ChatKeyToken(string token)
        {
            if (token == "/" && (_chatKeyTokens.Count == 0 || _chatKeyTokens[^1] == "/")) return;
            _chatKeyTokens.Add(token);
            ShowChatKeyInput("");
        }

        private void ChatKeyBackspace()
        {
            if (_keyer.CurrentCode.Length > 0) { _keyer.Reset(); ShowChatKeyInput(""); return; }

            // Eine Wortpause „/“ am Ende sieht man im Text nicht. Damit die Rücktaste nicht
            // „nichts“ zu tun scheint, wird die Pause zusammen mit dem Buchstaben davor gelöscht.
            while (_chatKeyTokens.Count > 0 && _chatKeyTokens[^1] == "/") _chatKeyTokens.RemoveAt(_chatKeyTokens.Count - 1);
            if (_chatKeyTokens.Count > 0) _chatKeyTokens.RemoveAt(_chatKeyTokens.Count - 1);
            ShowChatKeyInput("");
        }

        /// <summary>Schreibt die gemorsten Buchstaben ins Eingabefeld; der gerade entstehende Code steht in der Vorschau.</summary>
        private void ShowChatKeyInput(string current)
        {
            if (!txtChatInput.Focused) txtChatInput.Focus();   // damit Enter wirklich sendet
            txtChatInput.Text = string.Join(" ", _chatKeyTokens);
            txtChatInput.SelectionStart = txtChatInput.TextLength;
            if (current.Length > 0) lblChatPreview.Text = $"⌨ {MorseTable.Pretty(current)}   → {MorseTable.MorseToText(txtChatInput.Text)}";
        }

        private void RefreshPeers()
        {
            listPeers.Items.Clear();
            foreach (var p in _node.Peers.Where(p => p.Name != null)) listPeers.Items.Add(p);
            foreach (var c in _web.Clients) listPeers.Items.Add(c);
            tabChat.Text = listPeers.Items.Count > 0 ? $"Chat ({listPeers.Items.Count})" : "Chat";
        }

        private void RefreshFound()
        {
            var selected = (listFound.SelectedItem as FoundPeer)?.Key;
            listFound.BeginUpdate();
            listFound.Items.Clear();
            foreach (var fp in _found.Values.OrderBy(f => f.Name)) listFound.Items.Add(fp);
            foreach (FoundPeer fp in listFound.Items) if (fp.Key == selected) listFound.SelectedItem = fp;
            listFound.EndUpdate();
        }

        private static IEnumerable<string> LocalAddresses()
        {
            try
            {
                return Dns.GetHostAddresses(Dns.GetHostName())
                    .Where(a => a.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(a))
                    .Select(a => a.ToString());
            }
            catch (SocketException) { return ["?"]; }
        }

        // ================================================================ Verlauf

        private void ChatMessage(string who, bool mine, string morse) =>
            AddChatEntry(new ChatHistory.Entry(DateTime.Now, who, mine, morse));

        private void SystemMessage(string text) =>
            AddChatEntry(new ChatHistory.Entry(DateTime.Now, null, false, text));

        private void AddChatEntry(ChatHistory.Entry entry)
        {
            if (entry.Who != null) _history.Add(entry);   // nur echte Nachrichten speichern, keine Hinweise
            WriteChatLine(entry);
            if (entry.Who != null && tabMain.SelectedTab != tabChat)
                SetStatus($"Neue Chat-Nachricht von {entry.Who}.");
        }

        private void WriteChatLine(ChatHistory.Entry e)
        {
            rtbChat.SelectionStart = rtbChat.TextLength;
            if (e.Who == null)
            {
                rtbChat.SelectionColor = Color.DarkGoldenrod;
                rtbChat.SelectionFont = new Font(rtbChat.Font, FontStyle.Italic);
                rtbChat.AppendText($"{e.Time:HH:mm}  {e.Text}\n");
            }
            else
            {
                rtbChat.SelectionColor = e.Mine ? Color.RoyalBlue : Color.ForestGreen;
                rtbChat.SelectionFont = new Font(rtbChat.Font, FontStyle.Bold);
                rtbChat.AppendText($"{e.Time:HH:mm}  {e.Who}:  ");
                rtbChat.SelectionColor = Color.Black;
                rtbChat.AppendText(MorseTable.MorseToText(e.Text) + "\n");
                rtbChat.SelectionColor = Color.Gray;
                rtbChat.SelectionFont = new Font("Consolas", 9.5F);
                rtbChat.AppendText("           " + MorseTable.Pretty(e.Text) + "\n");
            }
            rtbChat.ScrollToCaret();
        }

        private void WriteSeparator(string text)
        {
            rtbChat.SelectionStart = rtbChat.TextLength;
            rtbChat.SelectionColor = Color.Silver;
            rtbChat.SelectionFont = new Font(rtbChat.Font, FontStyle.Regular);
            rtbChat.AppendText(text + "\n");
        }

        private void ExportHistory()
        {
            if (_history.Entries.Count == 0) { SystemMessage("Der Verlauf ist leer."); return; }
            using var dialog = new SaveFileDialog
            {
                Filter = "Textdatei (*.txt)|*.txt",
                FileName = $"Morse-Chat {DateTime.Now:yyyy-MM-dd}.txt",
            };
            if (dialog.ShowDialog(this) != DialogResult.OK) return;
            File.WriteAllText(dialog.FileName, _history.ToText());
            SetStatus("Verlauf gespeichert: " + dialog.FileName);
        }

        private void ClearHistory()
        {
            var answer = MessageBox.Show(this, "Den ganzen Chatverlauf löschen? Das kann man nicht rückgängig machen.",
                "Verlauf löschen", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (answer != DialogResult.Yes) return;
            _history.Clear();
            rtbChat.Clear();
            SetStatus("Chatverlauf gelöscht.");
        }
    }
}
