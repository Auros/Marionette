using Marionette.Daemon.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Marionette.Daemon.Services.Hosted;

internal class PollingService : BackgroundService
{
    private readonly ILogger _logger;
    private readonly IPollable[] _pollables;

    public PollingService(ILogger<PollingService> logger, IEnumerable<IPollable> pollables)
    {
        _logger = logger;
        _pollables = pollables.ToArray(); // We convert it to an array, because we're going to use foreach later. When an IEnumerable is iterated over, it allocates a tiny bit, which doesn't happen with arrays and List<T>
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting polling service");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Run every pollable in the container.
                foreach (var pollable in _pollables)
                    pollable.Poll(stoppingToken);

                await Task.Delay(25, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                // When we get this exception, it's almost always going to be the Task.Delay in the try block
                // This is normal when the background service is being destroyed, hence we don't want to throw
                // further, and we don't want to log anything critically 
                _logger.LogDebug("Shutting down");
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "An error occured while running the pollables");
                // We're not rethrowing this exception as it's at the top of the application.
            }
        }
        _logger.LogInformation("Finalizing polling service");
    }
}