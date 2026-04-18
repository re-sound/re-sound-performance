using Microsoft.Win32;
using re_sound_performance.Core.Backup;
using re_sound_performance.Core.Registry;

namespace re_sound_performance.Core.Tweaks.Implementations;

public sealed class DisableGameDvrTweak : ITweak
{
    private static readonly IReadOnlyList<RegistryChange> AppliedChanges = new[]
    {
        new RegistryChange(RegistryHive.CurrentUser, @"System\GameConfigStore", "GameDVR_Enabled", 0, RegistryValueKind.DWord),
        new RegistryChange(RegistryHive.CurrentUser, @"System\GameConfigStore", "GameDVR_FSEBehaviorMode", 2, RegistryValueKind.DWord),
        new RegistryChange(RegistryHive.CurrentUser, @"System\GameConfigStore", "GameDVR_HonorUserFSEBehaviorMode", 1, RegistryValueKind.DWord),
        new RegistryChange(RegistryHive.CurrentUser, @"System\GameConfigStore", "GameDVR_DXGIHonorFSEWindowsCompatible", 1, RegistryValueKind.DWord),
        new RegistryChange(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\GameDVR", "AllowGameDVR", 0, RegistryValueKind.DWord)
    };

    private readonly IRegistryAccess _registry;

    public DisableGameDvrTweak(IRegistryAccess registry)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    }

    public TweakMetadata Metadata { get; } = new(
        Id: "system.disable_game_dvr",
        Name: "Disable Game DVR and FSE hook",
        ShortDescription: "Stops the background Game DVR recorder and removes the Windows fullscreen hook from DirectX games.",
        DetailedDescription: "Game DVR keeps an active recording buffer for every foreground game and wraps exclusive fullscreen into the DWM fullscreen optimization layer. Disabling it releases a recording process, avoids an input-latency hook on DX10/DX11 titles, and lets games run in true exclusive fullscreen.",
        Modifies: "HKCU\\System\\GameConfigStore (GameDVR_Enabled, GameDVR_FSEBehaviorMode, GameDVR_HonorUserFSEBehaviorMode, GameDVR_DXGIHonorFSEWindowsCompatible), HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\GameDVR\\AllowGameDVR",
        ExpectedImpact: "Eliminates Game DVR background CPU and disk activity. Can reduce input latency on DX10/DX11 titles that fall back to exclusive fullscreen.",
        Category: TweakCategory.System,
        Risk: TweakRisk.Safe,
        Evidence: TweakEvidenceLevel.Confirmed,
        Sources: new[]
        {
            "Microsoft Support: options-to-optimize-gaming-performance-in-windows-11",
            "Blur Busters: Optimizations for Windowed Games and FSE behavior"
        },
        IncompatibleWith: Array.Empty<string>(),
        RequiresRestart: false,
        BlockedWhenVanguardInstalled: false,
        BlockedWhenFaceitInstalled: false);

    public Task<TweakStatus> ProbeAsync(CancellationToken cancellationToken = default) =>
        RegistryTweakHelper.ProbeAsync(_registry, AppliedChanges);

    public Task<TweakResult> ApplyAsync(IBackupStore backupStore, CancellationToken cancellationToken = default) =>
        RegistryTweakHelper.ApplyAsync(_registry, backupStore, Metadata.Id, AppliedChanges, cancellationToken);

    public Task<TweakResult> RevertAsync(IBackupStore backupStore, CancellationToken cancellationToken = default) =>
        RegistryTweakHelper.RevertAsync(_registry, backupStore, Metadata.Id, AppliedChanges, cancellationToken);
}
