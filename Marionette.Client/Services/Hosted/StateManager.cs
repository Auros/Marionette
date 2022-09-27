using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Marionette.Services.Hosted;

internal class StateManager : IHostedService
{
    private readonly ILogger _logger;
    private readonly UdpClient _client;
    private readonly LocalStateService _localStateService;
    private readonly CancellationTokenSource _cts = new();

    private static readonly byte[] _ping = new byte[] { 0x50, 0x49, 0x4e, 0x47 };
    private static readonly byte[] _pong = new byte[] { 0x50, 0x4f, 0x4e, 0x47 };

    public StateManager(ILogger<StateManager> logger, LocalStateService localStateService)
    {
        _client = new UdpClient();
        _logger = logger;
        _localStateService = localStateService;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _client.Connect("localhost", 49700);
        _ = Task.Run(PingThread, cancellationToken);
        _ = Task.Run(UpdateThread, cancellationToken);
        return Task.CompletedTask;
    }

    private async Task PingThread()
    {
        while (!_cts.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(1000, _cts.Token);
                await _client.SendAsync(_ping, _cts.Token);
            }
            // These two exceptions are regular and expected if one side gets closed off
            catch (TaskCanceledException) { }
            catch (OperationCanceledException) { }
            catch (SocketException socketException)
            {
                _localStateService.UpdatePing(false);

                /* If the socket was cancelled, we don't care. If the socket gets disconnected then we're either just gonna not process the packet, or we'll reconnect later (internal daemon restart). */
                if (socketException.SocketErrorCode is SocketError.Interrupted)
                    continue;

                _logger.LogError(socketException, "An error occured while trying to recieve data");
            }
            catch
            {
                _logger.LogError("An error occured while polling");
            }
        }
    }

    private async Task UpdateThread()
    {
        while (!_cts.IsCancellationRequested)
        {
            try
            {
                var result = await _client.ReceiveAsync(_cts.Token);
                ReceivedMessage(result);
            }
            // These two exceptions are regular and expected if one side gets closed off
            catch (TaskCanceledException) { }
            catch (OperationCanceledException) { }
            catch (SocketException socketException)
            {
                _localStateService.UpdatePing(false);

                /* If the daemon isn't online, we don't care, it gets handled by the ping service. */
                if (socketException.SocketErrorCode is SocketError.ConnectionReset)
                    continue;

                _logger.LogError(socketException, "An error occured while trying to reecive data... {Code}", socketException.SocketErrorCode);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occured");
            }
        }
    }

    private void ReceivedMessage(UdpReceiveResult result)
    {
        if (result.Buffer.Length == 4 && result.Buffer.SequenceEqual(_pong))
        {
            _logger.LogDebug("PONG!");
            _localStateService.UpdatePing(true);
            return;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _cts.Cancel();
        return Task.CompletedTask;
    }
}