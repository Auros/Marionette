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
    logging
        .ClearProviders()
        .AddSerilog()
    ;
});

builder.ConfigureServices(services =>
{

});

await builder.RunConsoleAsync();