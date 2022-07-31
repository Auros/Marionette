using Marionette.Daemon.Interfaces;
using Marionette.Daemon.Networking.OSC;
using Marionette.Daemon.Services.Hosted;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;

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
        .AddSingleton<OSCReceiver>().AddSingleton<IPollable>(c => c.GetRequiredService<OSCReceiver>()) // We do the second registration to make sure it gets registered as an IPollable
    ;

    // Register our background services
    services.AddHostedService<PollingService>();
});

await builder.RunConsoleAsync();