using Marionette.Daemon.Interfaces;
using Marionette.Daemon.Networking.OSC;
using Marionette.Daemon.Services.Hosted;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OscCore;
using Serilog;
using Serilog.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

HostBuilder builder = new();

var logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3} {SourceContext}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

Log.Logger = logger;

SerilogLoggerFactory logFactory = new(Log.Logger);
var programLogger = logFactory.CreateLogger<Program>();

builder.ConfigureLogging((ctx, logging) =>
{
    // Not a fan of Microsoft's default logging, so we clear the providers and replace it with Serilog
    logging
        .ClearProviders()
        .AddSerilog()
    ;
});

builder.ConfigureServices(services =>
{
    services
        .AddSingleton<OSCReceiver>()
        .AddSingleton<IPollable>(c => c.GetRequiredService<OSCReceiver>()) // We do the second registration to make sure it gets registered as an IPollable
        .AddSingleton<IOSCReceiver>(c => c.GetRequiredService<OSCReceiver>()) 
    ;

    services.AddSingleton<Test>();

    // Register our background services
    services.AddHostedService<PollingService>();
});

var host = builder.UseConsoleLifetime().Build();

host.Services.GetRequiredService<Test>();

await host.RunAsync();


class Test
{
    private readonly ILogger _logger;
    private readonly IOSCReceiver _receiver;

    public Test(ILogger<Test> logger, IOSCReceiver receiver)
    {
        _logger = logger;
        _receiver = receiver;
        _receiver.Subscribe("*", MessageReceived);
    }

    ~Test()
    {
        _receiver.Unsubscribe("*", MessageReceived);
    }

    private void MessageReceived(OscMessage msg)
    {
        _logger.LogInformation("Received message from subscription!");
    }
}