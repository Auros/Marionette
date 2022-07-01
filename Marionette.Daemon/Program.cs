using Microsoft.Extensions.Hosting;

var host = Host.CreateDefaultBuilder(args);

await host.RunConsoleAsync();