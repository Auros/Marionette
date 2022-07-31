using Marionette.Daemon.Interfaces;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Marionette.Daemon.Networking.OSC;

internal class OSCReceiver : IPollable, IDisposable
{
    private IPEndPoint _listenOn;
    private readonly ILogger _logger;
    private readonly UdpClient _udpClient;

    private bool _disposed;
    private CancellationToken? _pollingToken;

    public OSCReceiver(ILogger<OSCReceiver> logger)
    {
        _logger = logger;

        // TODO: Load port from a config.
        // Assign the UDP Client with the listening port
        _udpClient = new(9001);
        _listenOn = new IPEndPoint(IPAddress.Any, 9001);

        // We start the receiver thread here
        Task.Run(Process);
    }

    public void Poll(CancellationToken cancellationToken)
    {
        _pollingToken = cancellationToken;
    }

    private void Process()
    {
        while (!_disposed)
        {
            if (!_pollingToken.HasValue)
                continue;

            if (_pollingToken.Value.IsCancellationRequested)
                return;

            _pollingToken = null;
            try
            {
                var buffer = _udpClient.Receive(ref _listenOn);
                DataReceived(buffer);
            }
            catch (SocketException socketException)
            {
                /* If the socket was cancelled, we don't care. If the socket gets disconnected then we're either just gonna not process the packet, or we'll reconnect later (internal daemon restart). */
                if (socketException.SocketErrorCode is SocketError.Interrupted)
                    continue;

                _logger.LogError(socketException, "An error occured while trying to reecive data");
            }
        }
    }

    private void DataReceived(byte[] buffer)
    {
        _logger.LogInformation("Received Packet, Count = {PacketCount}", buffer.Length);
        _logger.LogInformation("Data: {Data}", Encoding.UTF8.GetString(buffer));
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _udpClient.Dispose();
        _disposed = true;
    }
}