using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Morse
{
    /// <summary>
    /// Kleiner eingebauter Webserver: Freunde öffnen die Seite im Browser (Handy oder PC)
    /// und können ohne Installation mitmorsen. Die Seite redet per WebSocket mit diesem Programm.
    ///
    /// Verschlüsselung: Browser erlauben ihre eingebaute Krypto nur über https, deshalb gibt es hier
    /// ein eigenes Verfahren, das die Webseite in reinem JavaScript genauso nachbaut:
    /// Schlüssel = PBKDF2-SHA256(Passwort), Schlüsselstrom = HMAC-SHA256(encKey, nonce|zähler),
    /// Prüfsumme = HMAC-SHA256(macKey, nonce|chiffrat). Ohne richtiges Passwort kann man nichts lesen.
    /// </summary>
    public sealed class WebBridge : IDisposable
    {
        public const int DefaultPort = 47802;
        private const int Iterations = 20_000;
        private static readonly byte[] Salt = Encoding.UTF8.GetBytes("MorseCode/web1");

        private readonly object _lock = new();
        private readonly List<Client> _clients = new();
        private TcpListener? _listener;
        private CancellationTokenSource? _cts;
        private byte[] _encKey = new byte[32], _macKey = new byte[32];

        public int Port { get; private set; }
        public bool IsRunning => _listener != null;

        /// <summary>Liefert die Namen aller, die gerade im Chat sind (für die Begrüßung).</summary>
        public Func<IEnumerable<string>> OnlineNames { get; set; } = () => [];

        public event Action<Client>? ClientJoined;
        public event Action<Client>? ClientLeft;
        public event Action<Client, string>? MessageReceived;
        public event Action<string>? Log;

        public IReadOnlyList<Client> Clients { get { lock (_lock) return _clients.Where(c => c.Name != null).ToList(); } }

        public void SetPassword(string password)
        {
            var key = Rfc2898DeriveBytes.Pbkdf2(password, Salt, Iterations, HashAlgorithmName.SHA256, 64);
            _encKey = key[..32];
            _macKey = key[32..];
        }

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
            List<Client> all;
            lock (_lock) all = _clients.ToList();
            foreach (var c in all) c.Close();
        }

        /// <summary>Schickt eine Morse-Nachricht an alle Browser-Freunde (außer <paramref name="except"/>).</summary>
        public void Broadcast(string from, string morse, Client? except = null)
        {
            foreach (var c in Clients.Where(c => c != except)) _ = c.SendAsync(new ChatNode.Packet("msg", from, morse));
        }

        /// <summary>Schickt eine Hinweis-Zeile (z. B. „Ben ist beigetreten“) an alle Browser-Freunde.</summary>
        public void Notice(string text, Client? except = null)
        {
            foreach (var c in Clients.Where(c => c != except)) _ = c.SendAsync(new ChatNode.Packet("sys", "", text));
        }

        /// <summary>Schickt jedem Browser-Freund die aktuelle Liste, wer gerade online ist.</summary>
        public void UpdateOnline()
        {
            var names = OnlineNames().Distinct().ToList();
            foreach (var c in Clients)
                _ = c.SendAsync(new ChatNode.Packet("online", "", string.Join(", ", names.Where(n => n != c.Name))));
        }

        private async Task AcceptLoop(TcpListener listener, CancellationToken ct)
        {
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    var tcp = await listener.AcceptTcpClientAsync(ct);
                    _ = HandleConnection(tcp, ct);
                }
            }
            catch (OperationCanceledException) { }
            catch (ObjectDisposedException) { }
            catch (SocketException ex) { Log?.Invoke("Browser-Zugang gestoppt: " + ex.Message); }
        }

        // ---------- HTTP ----------

        private async Task HandleConnection(TcpClient tcp, CancellationToken ct)
        {
            tcp.NoDelay = true;
            var stream = tcp.GetStream();
            try
            {
                Dictionary<string, string> headers;
                string path;
                using (var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct))
                {
                    timeout.CancelAfter(TimeSpan.FromSeconds(10));
                    (path, headers) = await ReadRequest(stream, timeout.Token);
                }

                if (path == "/ws" && headers.TryGetValue("sec-websocket-key", out var wsKey))
                {
                    var accept = Convert.ToBase64String(SHA1.HashData(Encoding.ASCII.GetBytes(wsKey + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11")));
                    await WriteAscii(stream, "HTTP/1.1 101 Switching Protocols\r\nUpgrade: websocket\r\nConnection: Upgrade\r\n" +
                                             $"Sec-WebSocket-Accept: {accept}\r\n\r\n", ct);
                    var client = new Client(this, tcp, stream, tcp.Client.RemoteEndPoint?.ToString() ?? "?");
                    lock (_lock) _clients.Add(client);
                    await client.RunAsync(ct);
                    return;
                }

                if (path is "/" or "/index.html")
                {
                    var body = Encoding.UTF8.GetBytes(WebPage.Render());
                    await WriteAscii(stream, "HTTP/1.1 200 OK\r\nContent-Type: text/html; charset=utf-8\r\n" +
                                             $"Content-Length: {body.Length}\r\nCache-Control: no-store\r\nConnection: close\r\n\r\n", ct);
                    await stream.WriteAsync(body, ct);
                }
                else if (path == "/pixel.ttf")
                {
                    await WriteAscii(stream, "HTTP/1.1 200 OK\r\nContent-Type: font/ttf\r\n" +
                                             $"Content-Length: {WebPage.PixelFont.Length}\r\nCache-Control: max-age=86400\r\nConnection: close\r\n\r\n", ct);
                    await stream.WriteAsync(WebPage.PixelFont, ct);
                }
                else
                {
                    await WriteAscii(stream, "HTTP/1.1 404 Not Found\r\nContent-Length: 0\r\nConnection: close\r\n\r\n", ct);
                }
            }
            catch (Exception) { }
            tcp.Close();
        }

        private static async Task<(string Path, Dictionary<string, string> Headers)> ReadRequest(NetworkStream stream, CancellationToken ct)
        {
            var buffer = new byte[8192];
            int length = 0;
            while (true)
            {
                if (length == buffer.Length) throw new InvalidDataException("Anfrage zu groß");
                int n = await stream.ReadAsync(buffer.AsMemory(length), ct);
                if (n == 0) throw new EndOfStreamException();
                length += n;
                var text = Encoding.ASCII.GetString(buffer, 0, length);
                int end = text.IndexOf("\r\n\r\n", StringComparison.Ordinal);
                if (end < 0) continue;

                var lines = text[..end].Split("\r\n");
                var parts = lines[0].Split(' ');
                var path = parts.Length > 1 ? parts[1].Split('?')[0] : "/";
                var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (var line in lines.Skip(1))
                {
                    int colon = line.IndexOf(':');
                    if (colon > 0) headers[line[..colon].Trim()] = line[(colon + 1)..].Trim();
                }
                return (path, headers);
            }
        }

        private static Task WriteAscii(Stream s, string text, CancellationToken ct) =>
            s.WriteAsync(Encoding.ASCII.GetBytes(text), ct).AsTask();

        // ---------- Verschlüsselung (muss exakt zur Webseite passen) ----------

        private string Encrypt(ChatNode.Packet packet)
        {
            var plain = JsonSerializer.SerializeToUtf8Bytes(packet);
            var nonce = RandomNumberGenerator.GetBytes(16);
            var cipher = XorStream(nonce, plain);
            var tag = HMACSHA256.HashData(_macKey, nonce.Concat(cipher).ToArray());
            return Convert.ToBase64String([.. nonce, .. cipher, .. tag]);
        }

        private ChatNode.Packet? Decrypt(string text)
        {
            try
            {
                var data = Convert.FromBase64String(text);
                if (data.Length < 48) return null;
                var nonce = data[..16];
                var cipher = data[16..^32];
                var tag = data[^32..];
                if (!CryptographicOperations.FixedTimeEquals(tag, HMACSHA256.HashData(_macKey, nonce.Concat(cipher).ToArray()))) return null;
                return JsonSerializer.Deserialize<ChatNode.Packet>(XorStream(nonce, cipher));
            }
            catch (Exception) { return null; }
        }

        private byte[] XorStream(byte[] nonce, byte[] data)
        {
            var output = new byte[data.Length];
            var block = new byte[20];
            nonce.CopyTo(block, 0);
            for (int offset = 0, counter = 0; offset < data.Length; offset += 32, counter++)
            {
                System.Buffers.Binary.BinaryPrimitives.WriteInt32BigEndian(block.AsSpan(16), counter);
                var keystream = HMACSHA256.HashData(_encKey, block);
                for (int i = 0; i < 32 && offset + i < data.Length; i++)
                    output[offset + i] = (byte)(data[offset + i] ^ keystream[i]);
            }
            return output;
        }

        public void Dispose() => Stop();

        // ---------- WebSocket-Verbindung eines Browser-Freundes ----------

        public sealed class Client
        {
            private readonly WebBridge _bridge;
            private readonly TcpClient _tcp;
            private readonly NetworkStream _stream;
            private readonly SemaphoreSlim _writeLock = new(1, 1);

            public string Endpoint { get; }
            public string? Name { get; private set; }

            internal Client(WebBridge bridge, TcpClient tcp, NetworkStream stream, string endpoint)
            {
                _bridge = bridge;
                _tcp = tcp;
                _stream = stream;
                Endpoint = endpoint;
            }

            internal async Task RunAsync(CancellationToken ct)
            {
                try
                {
                    string? text;
                    while ((text = await ReadMessage(ct)) != null)
                    {
                        var packet = _bridge.Decrypt(text);
                        if (packet == null)
                        {
                            if (Name == null)
                            {
                                await SendRawAsync("!Falsches Passwort – frag deinen Freund nach dem richtigen.");
                                _bridge.Log?.Invoke($"Browser {Endpoint} abgelehnt: falsches Passwort.");
                                break;
                            }
                            continue;
                        }

                        if (packet.T == "hello" && Name == null)
                        {
                            Name = string.IsNullOrWhiteSpace(packet.N) ? "Gast" : packet.N.Trim()[..Math.Min(packet.N.Trim().Length, 30)];
                            var others = string.Join(", ", _bridge.OnlineNames().Where(n => n != Name).Distinct());
                            await SendAsync(new ChatNode.Packet("welcome", "", others));
                            _bridge.ClientJoined?.Invoke(this);
                        }
                        else if (packet.T == "msg" && Name != null && packet.M.Length is > 0 and < 2000)
                        {
                            _bridge.MessageReceived?.Invoke(this, packet.M);
                        }
                    }
                }
                catch (Exception) { }

                Close();
                bool removed;
                lock (_bridge._lock) removed = _bridge._clients.Remove(this);
                if (removed && Name != null) _bridge.ClientLeft?.Invoke(this);
            }

            /// <summary>Liest eine komplette Text-Nachricht (null = Verbindung zu).</summary>
            private async Task<string?> ReadMessage(CancellationToken ct)
            {
                var message = new List<byte>();
                while (true)
                {
                    var head = await ReadExactly(2, ct);
                    bool fin = (head[0] & 0x80) != 0;
                    int opcode = head[0] & 0x0F;
                    bool masked = (head[1] & 0x80) != 0;
                    long len = head[1] & 0x7F;
                    if (len == 126) len = System.Buffers.Binary.BinaryPrimitives.ReadUInt16BigEndian(await ReadExactly(2, ct));
                    else if (len == 127) len = System.Buffers.Binary.BinaryPrimitives.ReadInt64BigEndian(await ReadExactly(8, ct));
                    if (len > 65536 || !masked) return null; // Browser maskieren immer; riesige Nachrichten lehnen wir ab
                    var mask = await ReadExactly(4, ct);
                    var payload = await ReadExactly((int)len, ct);
                    for (int i = 0; i < payload.Length; i++) payload[i] ^= mask[i % 4];

                    switch (opcode)
                    {
                        case 0x8: return null; // Schließen
                        case 0x9: await SendFrame(0xA, payload); continue; // Ping → Pong
                        case 0xA: continue; // Pong
                    }
                    message.AddRange(payload);
                    if (message.Count > 65536) return null;
                    if (fin) return Encoding.UTF8.GetString(message.ToArray());
                }
            }

            private async Task<byte[]> ReadExactly(int count, CancellationToken ct)
            {
                var buffer = new byte[count];
                await _stream.ReadExactlyAsync(buffer, ct);
                return buffer;
            }

            internal Task SendAsync(ChatNode.Packet packet) => SendRawAsync(_bridge.Encrypt(packet));

            private Task SendRawAsync(string text) => SendFrame(0x1, Encoding.UTF8.GetBytes(text));

            private async Task SendFrame(int opcode, byte[] payload)
            {
                var header = new List<byte> { (byte)(0x80 | opcode) };
                if (payload.Length < 126) header.Add((byte)payload.Length);
                else if (payload.Length <= ushort.MaxValue) { header.Add(126); header.Add((byte)(payload.Length >> 8)); header.Add((byte)payload.Length); }
                else { header.Add(127); for (int i = 7; i >= 0; i--) header.Add((byte)((long)payload.Length >> (8 * i))); }

                await _writeLock.WaitAsync();
                try
                {
                    await _stream.WriteAsync(header.ToArray());
                    await _stream.WriteAsync(payload);
                }
                catch (Exception) { Close(); }
                finally { _writeLock.Release(); }
            }

            public void Close() => _tcp.Close();

            public override string ToString() => $"🌐 {Name ?? "?"} (Browser)";
        }
    }
}
