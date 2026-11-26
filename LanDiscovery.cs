using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Morse
{
    /// <summary>
    /// Findet andere MorseCode-Programme im gleichen WLAN/Netzwerk per UDP-Broadcast.
    /// Die Broadcasts verlassen das lokale Netz nie (Router leiten sie nicht weiter).
    /// </summary>
    public sealed class LanDiscovery : IDisposable
    {
        public const int DiscoveryPort = 47801;
        private const string Magic = "MORSECODE1";

        private readonly string _instanceId = Guid.NewGuid().ToString("N");
        private UdpClient? _udp;
        private CancellationTokenSource? _cts;

        public bool IsRunning => _udp != null;

        /// <summary>Name, Adresse, TCP-Port eines gefundenen Freundes.</summary>
        public event Action<string, IPAddress, int>? PeerFound;

        public void Start(Func<string> getName, int tcpPort)
        {
            Stop();
            _cts = new CancellationTokenSource();
            _udp = new UdpClient();
            _udp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _udp.Client.Bind(new IPEndPoint(IPAddress.Any, DiscoveryPort));
            _udp.EnableBroadcast = true;
            _ = ReceiveLoop(_udp, _cts.Token);
            _ = AnnounceLoop(_udp, getName, tcpPort, _cts.Token);
        }

        public void Stop()
        {
            _cts?.Cancel();
            _udp?.Close();
            _udp = null;
        }

        private async Task AnnounceLoop(UdpClient udp, Func<string> getName, int tcpPort, CancellationToken ct)
        {
            var target = new IPEndPoint(IPAddress.Broadcast, DiscoveryPort);
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    var name = getName().Replace("|", "");
                    var data = Encoding.UTF8.GetBytes($"{Magic}|{_instanceId}|{tcpPort}|{name}");
                    await udp.SendAsync(data, target, ct);
                    await Task.Delay(2000, ct);
                }
                catch (Exception) { return; }
            }
        }

        private async Task ReceiveLoop(UdpClient udp, CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    var result = await udp.ReceiveAsync(ct);
                    var parts = Encoding.UTF8.GetString(result.Buffer).Split('|', 4);
                    if (parts.Length == 4 && parts[0] == Magic && parts[1] != _instanceId
                        && int.TryParse(parts[2], out var port))
                    {
                        PeerFound?.Invoke(parts[3], result.RemoteEndPoint.Address, port);
                    }
                }
                catch (Exception) { return; }
            }
        }

        public void Dispose() => Stop();
    }
}
