using System.IO;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using re_sound_performance.Core.Appx;
using re_sound_performance.Core.Backup;
using re_sound_performance.Core.Detection;
using re_sound_performance.Core.Games;
using re_sound_performance.Core.Power;
using re_sound_performance.Core.Presets;
using re_sound_performance.Core.Registry;
using re_sound_performance.Core.Services;
using re_sound_performance.Core.Tasks;
using re_sound_performance.Core.Tweaks;
using re_sound_performance.Core.Tweaks.Implementations;
using re_sound_performance.Views;

namespace re_sound_performance;

public partial class App : Application
{
    private IServiceProvider? _services;
    private Mutex? _instanceMutex;

    public IServiceProvider Services => _services ?? throw new InvalidOperationException("Services not initialized.");

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        if (!SingleInstanceGuard.TryAcquire(out _instanceMutex))
        {
            Shutdown();
            return;
        }

        var services = new ServiceCollection();
        ConfigureServices(services);
        _services = services.BuildServiceProvider();

        var main = _services.GetRequiredService<MainWindow>();
        main.Show();

        var engine = _services.GetRequiredService<TweakEngine>();
        _ = engine.ProbeAllAsync();

        _ = RunDetectionAsync();
    }

    private async Task RunDetectionAsync()
    {
        if (_services is null)
        {
            return;
        }

        var context = _services.GetRequiredService<DetectionContext>();
        var hardware = _services.GetRequiredService<IHardwareDetector>();
        var antiCheat = _services.GetRequiredService<IAntiCheatDetector>();

        try
        {
            var hw = await hardware.DetectAsync().ConfigureAwait(false);
            context.SetHardware(hw);
        }
        catch
        {
            context.SetHardware(HardwareInfo.Unknown);
        }

        try
        {
            var ac = await antiCheat.DetectAsync().ConfigureAwait(false);
            context.SetAntiCheat(ac);
        }
        catch
        {
            context.SetAntiCheat(AntiCheatInfo.None);
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        if (_instanceMutex is not null)
        {
            try { _instanceMutex.ReleaseMutex(); } catch { }
            _instanceMutex.Dispose();
            _instanceMutex = null;
        }

        base.OnExit(e);
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Information));

        services.AddSingleton<IRegistryAccess, WindowsRegistryAccess>();
        services.AddSingleton<IServiceManager, WindowsServiceManager>();
        services.AddSingleton<IPowerCfgRunner, WindowsPowerCfgRunner>();
        services.AddSingleton<IScheduledTaskManager, WindowsScheduledTaskManager>();
        services.AddSingleton<IAppxManager, PowerShellAppxManager>();

        var backupRoot = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "re_sound_performance",
            "backups");
        services.AddSingleton<IBackupStore>(_ => new FileSystemBackupStore(backupRoot));

        services.AddSingleton<RestorePointManager>();

        services.AddSingleton<TweakStateCache>();

        services.AddSingleton<IFileSystemProbe, FileSystemProbe>();
        services.AddSingleton<IHardwareDetector, WmiHardwareDetector>();
        services.AddSingleton<IAntiCheatDetector, WindowsAntiCheatDetector>();
        services.AddSingleton<DetectionContext>();
        services.AddSingleton<PresetRunner>();

        services.AddSingleton<IGameDetector, WindowsGameDetector>();
        services.AddSingleton<IGameConfigWriter, FileSystemGameConfigWriter>();

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
        services.AddSingleton<ITweak, DisableDiagTrackServiceTweak>();
        services.AddSingleton<ITweak, DisableMapsBrokerTweak>();
        services.AddSingleton<ITweak, DisableNduServiceTweak>();
        services.AddSingleton<ITweak, DisableFaxAndRetailDemoTweak>();
        services.AddSingleton<ITweak, DisableXboxServicesTweak>();
        services.AddSingleton<ITweak, DisableWerAndSysMainTweak>();
        services.AddSingleton<ITweak, EnableUltimatePerformancePlanTweak>();
        services.AddSingleton<ITweak, DisableHibernationTweak>();

        services.AddSingleton<ITweak, DisableTelemetryScheduledTasksTweak>();
        services.AddSingleton<ITweak, DisableUpdateOrchestratorTasksTweak>();
        services.AddSingleton<ITweak, DisableUsoSvcTweak>();

        services.AddSingleton<ITweak, RemoveTeamsConsumerTweak>();
        services.AddSingleton<ITweak, RemoveClipchampTweak>();
        services.AddSingleton<ITweak, RemoveBingAppsTweak>();
        services.AddSingleton<ITweak, RemoveXboxAppsConsumerTweak>();
        services.AddSingleton<ITweak, RemoveCopilotAppTweak>();
        services.AddSingleton<ITweak, RemoveRecallAppTweak>();
        services.AddSingleton<ITweak, RemoveStockAnnoyancesTweak>();
        services.AddSingleton<ITweak, RemoveLegacyMediaAppsTweak>();

        services.AddSingleton<TweakEngine>();

        services.AddSingleton<MainWindow>();
    }
}
