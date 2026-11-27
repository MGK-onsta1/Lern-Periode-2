using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Morse
{
    /// <summary>
    /// Direkte Verbindung zwischen zwei (oder mehr) Rechnern per TCP – ohne Server dazwischen.
    /// Jede Nachricht wird mit AES-GCM verschlüsselt; der Schlüssel kommt aus dem gemeinsamen Passwort.
    /// </summary>
    public sealed class ChatNode : IDisposable
    {
        public const int DefaultPort = 47800;

        private readonly object _lock = new();
        private readonly List<Peer> _peers = new();
        private TcpListener? _listener;
        private CancellationTokenSource? _cts;
        private byte[] _key = DeriveKey("");

        public string Name { get; set; } = Environment.UserName;
        public int Port { get; private set; }
        public bool IsRunning => _listener != null;

        public event Action<string>? Log;
        public event Action<Peer>? PeerConnected;
        public event Action<Peer>? PeerDisconnected;
        /// <summary>Peer, Name des Absenders (kann ein weitergeleiteter Freund sein), Morsecode.</summary>
        public event Action<Peer, string, string>? MessageReceived;

        public IReadOnlyList<Peer> Peers { get { lock (_lock) return _peers.ToList(); } }

        public void SetPassword(string password) => _key = DeriveKey(password);

        /// <summary>Startet den Empfang auf dem Port, damit Freunde sich verbinden können.</summary>
        public void Start(int port)
        {
            Stop();
            _cts = new CancellationTokenSource();
            _listener = new TcpListener(IPAddress.Any, port);
            _listener.Start();
            Port = port;
            _ = AcceptLoop(_listener, _cts.Token);
        }

        public void Stop()
        {
            _cts?.Cancel();
            _listener?.Stop();
            _listener = null;
            foreach (var p in Peers) p.Close();
        }

        /// <summary>Verbindet zu einem Freund (IP-Adresse oder Hostname).</summary>
        public async Task ConnectAsync(string host, int port)
        {
            var client = new TcpClient();
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(8));
            await client.ConnectAsync(host, port, timeout.Token);
            AddPeer(client, $"{host}:{port}");
        }

        /// <summary>Sendet an alle. Mit <paramref name="from"/> wird eine Nachricht eines Browser-Freundes weitergeleitet.</summary>
        public void Broadcast(string morse, string? from = null)
        {
            foreach (var p in Peers) _ = p.SendAsync(new Packet("msg", from ?? Name, morse));
        }

        private async Task AcceptLoop(TcpListener listener, CancellationToken ct)
        {
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    var client = await listener.AcceptTcpClientAsync(ct);
                    AddPeer(client, client.Client.RemoteEndPoint?.ToString() ?? "?");
                }
            }
            catch (OperationCanceledException) { }
            catch (ObjectDisposedException) { }
            catch (SocketException ex) { Log?.Invoke("Empfang gestoppt: " + ex.Message); }
        }

        private void AddPeer(TcpClient client, string endpoint)
        {
            client.NoDelay = true;
            var peer = new Peer(this, client, endpoint);
            lock (_lock) _peers.Add(peer);
            _ = peer.RunAsync();
            _ = peer.SendAsync(new Packet("hello", Name, ""));
        }

        private void OnPacket(Peer peer, Packet packet)
        {
            if (packet.T == "hello")
            {
                bool first = peer.Name == null;
                peer.Name = CleanName(packet.N);
                if (first) PeerConnected?.Invoke(peer);
            }
            else if (packet.T == "msg" && peer.Name != null) MessageReceived?.Invoke(peer, CleanName(packet.N), packet.M);
        }

        /// <summary>Höchstens so viele Zeichen darf ein Name haben.</summary>
        public const int MaxNameLength = 20;

        /// <summary>
        /// Aufgabe 6 (gelöst von Claude, 27.11.2026): Ein Name darf höchstens 20 Zeichen haben.
        /// Leerzeichen am Anfang/Ende fallen weg, ein leerer Name wird zu „Gast“ – wie in WebBridge.cs mit Math.Min.
        /// </summary>
        public static string CleanName(string? name)
        {
            name = (name ?? "").Trim();
            if (name.Length == 0) return "Gast";
            return name[..Math.Min(name.Length, MaxNameLength)];
        }

        private void OnClosed(Peer peer, string? reason)
        {
            bool removed;
            lock (_lock) removed = _peers.Remove(peer);
            if (!removed) return;
            if (reason != null) Log?.Invoke(reason);
            if (peer.Name != null) PeerDisconnected?.Invoke(peer);
        }

        // ---------- Verschlüsselung ----------

        private static byte[] DeriveKey(string password) =>
            Rfc2898DeriveBytes.Pbkdf2(password, Encoding.UTF8.GetBytes("MorseCode/v1"), 100_000, HashAlgorithmName.SHA256, 32);

        private string Encrypt(Packet packet)
        {
            var plain = JsonSerializer.SerializeToUtf8Bytes(packet);
            var nonce = RandomNumberGenerator.GetBytes(12);
            var tag = new byte[16];
            var cipher = new byte[plain.Length];
            using (var aes = new AesGcm(_key, 16)) aes.Encrypt(nonce, plain, cipher, tag);
            return Convert.ToBase64String([.. nonce, .. tag, .. cipher]);
        }

        private Packet? Decrypt(string line)
        {
            try
            {
                var data = Convert.FromBase64String(line);
                var plain = new byte[data.Length - 28];
                using (var aes = new AesGcm(_key, 16))
                    aes.Decrypt(data.AsSpan(0, 12), data.AsSpan(28), data.AsSpan(12, 16), plain);
                return JsonSerializer.Deserialize<Packet>(plain);
            }
            catch (Exception) { return null; }
        }

        public void Dispose() => Stop();

        public record Packet(string T, string N, string M);

        public sealed class Peer
        {
            private readonly ChatNode _node;
            private readonly TcpClient _client;
            private readonly StreamWriter _writer;
            private readonly SemaphoreSlim _writeLock = new(1, 1);

            public string Endpoint { get; }
            public string? Name { get; internal set; }

            internal Peer(ChatNode node, TcpClient client, string endpoint)
            {
                _node = node;
                _client = client;
                Endpoint = endpoint;
                _writer = new StreamWriter(client.GetStream(), new UTF8Encoding(false)) { AutoFlush = true };
            }

            internal async Task RunAsync()
            {
                string? reason = null;
                try
                {
                    using var reader = new StreamReader(_client.GetStream(), Encoding.UTF8);
                    string? line;
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        var packet = _node.Decrypt(line);
                        if (packet == null)
                        {
                            reason = $"Verbindung zu {Endpoint} abgelehnt: falsches Passwort.";
                            break;
                        }
                        _node.OnPacket(this, packet);
                    }
                }
                catch (Exception) { }

                // Der andere hat die Verbindung zugemacht, bevor er „hallo“ sagen konnte.
                // Das passiert vor allem, wenn sein Passwort nicht zu unserem passt.
                if (reason == null && Name == null)
                    reason = $"Verbindung zu {Endpoint} abgebrochen – habt ihr beide dasselbe Passwort?";
                Close();
                _node.OnClosed(this, reason);
            }

            internal async Task SendAsync(Packet packet)
            {
                await _writeLock.WaitAsync();
                try { await _writer.WriteLineAsync(_node.Encrypt(packet)); }
                catch (Exception) { Close(); }
                finally { _writeLock.Release(); }
            }

            public void Close() => _client.Close();

            public override string ToString() => $"{Name ?? "?"} ({Endpoint})";
        }
    }
}
