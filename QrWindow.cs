using QRCoder;

namespace Morse
{
    /// <summary>
    /// Zeigt einen Link als großen QR-Code – der Freund scannt ihn mit der Handy-Kamera
    /// und muss die Adresse nicht abtippen.
    /// </summary>
    public sealed class QrWindow : Form
    {
        private readonly PictureBox _picture = new() { Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.White };
        private readonly Label _url = new()
        {
            Dock = DockStyle.Fill, AutoSize = true, TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Consolas", 9.5F), ForeColor = Color.RoyalBlue, Padding = new Padding(0, 6, 0, 6),
            MaximumSize = new Size(400, 0),   // lange Links (Internet) umbrechen statt das Fenster zu sprengen
            Cursor = Cursors.Hand,
        };

        /// <param name="links">(Beschriftung, URL) – z. B. ("🌍 Internet", "https://…")</param>
        public QrWindow(IReadOnlyList<(string Label, string Url)> links)
        {
            Text = "QR-Code – mit dem Handy scannen";
            Font = new Font("Segoe UI", 10F);
            BackColor = Color.White;
            ClientSize = new Size(440, 560);
            MinimumSize = new Size(320, 420);
            StartPosition = FormStartPosition.CenterParent;
            ShowInTaskbar = false;
            MinimizeBox = false;

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, Padding = new Padding(16) };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var hint = new Label
            {
                AutoSize = true, Dock = DockStyle.Fill, ForeColor = Color.DimGray, MaximumSize = new Size(400, 0),
                Text = "Dein Freund öffnet die Kamera am Handy, scannt den Code und gibt dann dasselbe Passwort ein.",
            };

            // Bei mehreren Links (Internet + WLAN) kann man umschalten
            var choices = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, Margin = new Padding(0, 8, 0, 4) };
            var buttons = new List<Button>();
            foreach (var (label, url) in links)
            {
                var b = new Button { Text = label, AutoSize = true, Padding = new Padding(6, 2, 6, 2), FlatStyle = FlatStyle.Flat };
                b.Click += (_, _) => ShowLink(url, b, buttons);
                buttons.Add(b);
                choices.Controls.Add(b);
            }
            choices.Visible = links.Count > 1;

            var close = new Button { Text = "Schließen", AutoSize = true, Anchor = AnchorStyles.Right, Padding = new Padding(10, 3, 10, 3) };
            close.Click += (_, _) => Close();

            // Klick auf den Link kopiert ihn, dann kann man ihn z. B. per WhatsApp schicken
            var tip = new ToolTip();
            tip.SetToolTip(_url, "Klicken = Link kopieren");
            _url.Click += (_, _) =>
            {
                Clipboard.SetText(_url.Text);
                tip.Show("✔ Link kopiert", _url, 0, -24, 1500);
            };
            CancelButton = close;

            layout.Controls.Add(hint, 0, 0);
            layout.Controls.Add(choices, 0, 1);
            layout.Controls.Add(_picture, 0, 2);
            layout.Controls.Add(_url, 0, 3);
            layout.Controls.Add(close, 0, 4);
            Controls.Add(layout);

            if (links.Count > 0) ShowLink(links[0].Url, buttons[0], buttons);
        }

        private void ShowLink(string url, Button active, List<Button> all)
        {
            _url.Text = url;
            var old = _picture.Image;
            _picture.Image = Render(url);
            old?.Dispose();
            foreach (var b in all)
            {
                b.BackColor = b == active ? Color.LightSteelBlue : Color.White;
                b.FlatAppearance.BorderColor = b == active ? Color.RoyalBlue : Color.Silver;
            }
        }

        public static Image Render(string text)
        {
            using var generator = new QRCodeGenerator();
            using var data = generator.CreateQrCode(text, QRCodeGenerator.ECCLevel.M);
            var png = new PngByteQRCode(data).GetGraphic(12);
            using var stream = new MemoryStream(png);
            using var decoded = Image.FromStream(stream);
            return new Bitmap(decoded);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _picture.Image?.Dispose();
            base.Dispose(disposing);
        }
    }
}
