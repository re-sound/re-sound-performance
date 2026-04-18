using System.IO;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using re_sound_performance.Core.Backup;
using re_sound_performance.Core.Registry;
using re_sound_performance.Core.Tweaks;
using re_sound_performance.Core.Tweaks.Implementations;
using re_sound_performance.Views;

namespace re_sound_performance;

public partial class App : Application
{
    private IServiceProvider? _services;

    public IServiceProvider Services => _services ?? throw new InvalidOperationException("Services not initialized.");

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();
        ConfigureServices(services);
        _services = services.BuildServiceProvider();

        var main = _services.GetRequiredService<MainWindow>();
        main.Show();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Information));

        services.AddSingleton<IRegistryAccess, WindowsRegistryAccess>();

        var backupRoot = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "re_sound_performance",
            "backups");
        services.AddSingleton<IBackupStore>(_ => new FileSystemBackupStore(backupRoot));

        services.AddSingleton<RestorePointManager>();

        services.AddSingleton<ITweak, DisableXboxGameBarTweak>();
        services.AddSingleton<ITweak, DisableGameDvrTweak>();
        services.AddSingleton<ITweak, DisableMouseAccelerationTweak>();
        services.AddSingleton<ITweak, DisableStickyKeysTweak>();
        services.AddSingleton<ITweak, DisableTelemetryTweak>();
        services.AddSingleton<ITweak, DisableActivityHistoryTweak>();
        services.AddSingleton<ITweak, DisableAdvertisingIdTweak>();
        services.AddSingleton<ITweak, DisableCopilotTweak>();
        services.AddSingleton<ITweak, DisableMultiPlaneOverlayTweak>();
        services.AddSingleton<ITweak, SystemResponsivenessGamingTweak>();
        services.AddSingleton<ITweak, DisableEdgeStartupBoostTweak>();
        services.AddSingleton<ITweak, DisableStartupTipsTweak>();
        services.AddSingleton<ITweak, EnableShaderCacheUnlimitedTweak>();
        services.AddSingleton<ITweak, DisableSearchIndexingFullTweak>();
        services.AddSingleton<ITweak, DisableLocationTrackingTweak>();

        services.AddSingleton<TweakEngine>();

        services.AddSingleton<MainWindow>();
    }
}
