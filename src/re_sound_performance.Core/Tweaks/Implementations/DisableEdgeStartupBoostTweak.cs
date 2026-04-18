using Microsoft.Win32;
using re_sound_performance.Core.Backup;
using re_sound_performance.Core.Registry;

namespace re_sound_performance.Core.Tweaks.Implementations;

public sealed class DisableEdgeStartupBoostTweak : ITweak
{
    private static readonly IReadOnlyList<RegistryChange> AppliedChanges = new[]
    {
        new RegistryChange(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Edge", "StartupBoostEnabled", 0, RegistryValueKind.DWord),
        new RegistryChange(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Edge", "BackgroundModeEnabled", 0, RegistryValueKind.DWord)
    };

    private readonly IRegistryAccess _registry;

    public DisableEdgeStartupBoostTweak(IRegistryAccess registry)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    }

    public TweakMetadata Metadata { get; } = new(
        Id: "system.disable_edge_startup_boost",
        Name: "Disable Edge Startup Boost and Background Mode",
        ShortDescription: "Stops Microsoft Edge from preloading into RAM at logon and running after the last window is closed.",
        DetailedDescription: "Startup Boost keeps an Edge process resident at logon to make the browser launch faster. Background Mode keeps Edge running after you close every window. Both features consume RAM and can spike CPU during game startup. Disabling them has no downside for users who do not rely on Edge.",
        Modifies: "HKLM\\SOFTWARE\\Policies\\Microsoft\\Edge (StartupBoostEnabled, BackgroundModeEnabled)",
        ExpectedImpact: "Recovers 100 to 300 MB of RAM at logon. Edge starts a fraction of a second slower on manual launch.",
        Category: TweakCategory.System,
        Risk: TweakRisk.Safe,
        Evidence: TweakEvidenceLevel.Confirmed,
        Sources: new[]
        {
            "Microsoft Learn: Edge StartupBoostEnabled policy",
            "Microsoft Learn: Edge BackgroundModeEnabled policy"
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
