using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Morse
{
    /// <summary>
    /// Erstellt einen öffentlichen Internet-Link (https://….trycloudflare.com) zum Browser-Zugang,
    /// mit dem kostenlosen Programm „cloudflared“ von Cloudflare. Funktioniert auch in Schul-WLANs,
    /// weil die Verbindung von hier aus nach draußen aufgebaut wird. Kein Konto nötig.
    /// Die Nachrichten bleiben mit eurem Passwort verschlüsselt – Cloudflare kann sie nicht lesen.
    /// </summary>
    public sealed partial class TunnelManager : IDisposable
    {
        public const string DownloadUrl =
            "https://github.com/cloudflare/cloudflared/releases/latest/download/cloudflared-windows-amd64.exe";

        public static readonly string ExePath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MorseCode", "cloudflared.exe");

        private Process? _process;

        public static bool IsInstalled => File.Exists(ExePath);
        public bool IsRunning => _process is { HasExited: false };
        public string? Url { get; private set; }

        /// <summary>Der Link ist bereit.</summary>
        public event Action<string>? UrlReady;
        /// <summary>Der Tunnel wurde beendet (mit Grund, falls es ein Fehler war).</summary>
        public event Action<string?>? Stopped;

        [GeneratedRegex(@"https://[a-z0-9-]+\.trycloudflare\.com")]
        private static partial Regex UrlPattern();

        /// <summary>Lädt cloudflared einmalig herunter. Fortschritt in Prozent.</summary>
        public static async Task DownloadAsync(IProgress<int> progress, CancellationToken ct)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ExePath)!);
            var temp = ExePath + ".download";
            using (var http = new HttpClient { Timeout = TimeSpan.FromMinutes(10) })
            using (var response = await http.GetAsync(DownloadUrl, HttpCompletionOption.ResponseHeadersRead, ct))
            {
                response.EnsureSuccessStatusCode();
                long total = response.Content.Headers.ContentLength ?? -1;
                await using var source = await response.Content.ReadAsStreamAsync(ct);
                await using var target = File.Create(temp);
                var buffer = new byte[81920];
                long done = 0;
                int read;
                while ((read = await source.ReadAsync(buffer, ct)) > 0)
                {
                    await target.WriteAsync(buffer.AsMemory(0, read), ct);
                    done += read;
                    if (total > 0) progress.Report((int)(done * 100 / total));
                }
            }
            File.Move(temp, ExePath, overwrite: true);
        }

        /// <summary>Startet den Tunnel zum lokalen Port. Der Link kommt über <see cref="UrlReady"/>.</summary>
        public void Start(int localPort)
        {
            Stop();
            KillLeftovers();
            Url = null;
            var psi = new ProcessStartInfo(ExePath, $"tunnel --no-autoupdate --url http://localhost:{localPort}")
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
            };
            var process = new Process { StartInfo = psi, EnableRaisingEvents = true };
            var lastLines = new Queue<string>();

            void OnLine(string? line)
            {
                if (line == null) return;
                lock (lastLines)
                {
                    lastLines.Enqueue(line);
                    if (lastLines.Count > 5) lastLines.Dequeue();
                }
                var match = UrlPattern().Match(line);
                if (match.Success && Url == null)
                {
                    Url = match.Value;
                    UrlReady?.Invoke(Url);
                }
            }
            process.ErrorDataReceived += (_, e) => OnLine(e.Data); // cloudflared schreibt sein Protokoll auf stderr
            process.OutputDataReceived += (_, e) => OnLine(e.Data);
            process.Exited += (_, _) =>
            {
                if (_process != process) return; // absichtlich gestoppt
                _process = null;
                string reason;
                lock (lastLines) reason = string.Join(" ", lastLines.TakeLast(2));
                Stopped?.Invoke(Url == null ? "Internet-Link konnte nicht erstellt werden. " + reason : "Internet-Link wurde beendet.");
            };

            _process = process;
            process.Start();
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();
        }

        public void Stop()
        {
            var p = _process;
            _process = null;
            Url = null;
            if (p == null) return;
            try { if (!p.HasExited) p.Kill(entireProcessTree: true); }
            catch (Exception) { }
            p.Dispose();
            Stopped?.Invoke(null);
        }

        /// <summary>Beendet alte Tunnel unseres cloudflared, falls das Programm vorher abgestürzt ist.</summary>
        private static void KillLeftovers()
        {
            foreach (var p in Process.GetProcessesByName("cloudflared"))
            {
                try
                {
                    if (string.Equals(p.MainModule?.FileName, ExePath, StringComparison.OrdinalIgnoreCase)) p.Kill();
                }
                catch (Exception) { } // fremder Prozess ohne Zugriff → in Ruhe lassen
                finally { p.Dispose(); }
            }
        }

        public void Dispose() => Stop();
    }
}
