using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net.Sockets;

namespace Marionette.Daemon.Services.Hosted;

internal class StateManager : IHostedService
{
    private readonly ILogger _logger;
    private readonly UdpClient _server;
    private readonly CancellationTokenSource _cts = new();

    private static readonly byte[] _ping = new byte[] { 0x50, 0x49, 0x4e, 0x47 };
    private static readonly byte[] _pong = new byte[] { 0x50, 0x4f, 0x4e, 0x47 };

    public StateManager(ILogger<StateManager> logger)
    {
        _server = new UdpClient(49700);
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _ = Task.Run(UpdateThread, cancellationToken);
        return Task.CompletedTask;
    }

    private async Task UpdateThread()
    {
        while (!_cts.IsCancellationRequested)
        {
            try
            {
                var result = await _server.ReceiveAsync(_cts.Token);
                ReceivedMessage(result);
            }
            // These two exceptions are regular and expected if one side gets closed off
            catch (TaskCanceledException) { }
            catch (OperationCanceledException) { }
            catch (SocketException socketException)
            {
                /* If the socket was cancelled, we don't care. If the socket gets disconnected then we're either just gonna not process the packet, or we'll reconnect later (internal daemon restart). */
                if (socketException.SocketErrorCode is SocketError.Interrupted)
                    continue;

                _logger.LogError(socketException, "An error occured while trying to reecive data");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occured");
            }
        }
    }

    private void ReceivedMessage(UdpReceiveResult result)
    {
        if (result.Buffer.Length == 4 && result.Buffer.SequenceEqual(_ping))
        {
            // If we received a ping message, send back a pong.
            _server.Send(_pong, result.RemoteEndPoint);
            _logger.LogInformation("PING!");
            return;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _cts.Cancel();
        return Task.CompletedTask;
    }
}