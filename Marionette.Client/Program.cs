using Avalonia;
using Marionette.Services;
using Marionette.Services.Hosted;
using Marionette.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;

namespace Marionette;

internal class Program
{
    public static IServiceProvider Container { get; set; } = null!;

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static async Task Main(string[] args)
    {
        var builder = Host.CreateDefaultBuilder(args);
        builder
            .ConfigureServices(ConfigureServices)
        ;

        var host = builder.Build();
        Container = host.Services;

        _ = Task.Run(host.Run); // Run it on a separate thread, as host.Run is blocking

        // Run this after we initialize the host, the Start methods for avalonia apps block the current thread.
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);

        // Stop the host after the avalonia app finishes executing.
        await host.StopAsync();
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();

    private static void ConfigureServices(HostBuilderContext ctx, IServiceCollection services)
    {
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<SettingsViewModel>();
        services.AddSingleton<StatusViewModel>();
        services.AddSingleton<InfoViewModel>();

        services.AddSingleton<LocalStateService>();

        services.AddHostedService<StateManager>();
    }
}