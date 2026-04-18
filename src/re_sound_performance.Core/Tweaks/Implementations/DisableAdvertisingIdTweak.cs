using Microsoft.Win32;
using re_sound_performance.Core.Backup;
using re_sound_performance.Core.Registry;

namespace re_sound_performance.Core.Tweaks.Implementations;

public sealed class DisableAdvertisingIdTweak : ITweak
{
    private static readonly IReadOnlyList<RegistryChange> AppliedChanges = new[]
    {
        new RegistryChange(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo", "Enabled", 0, RegistryValueKind.DWord),
        new RegistryChange(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\AdvertisingInfo", "DisabledByGroupPolicy", 1, RegistryValueKind.DWord)
    };

    private readonly IRegistryAccess _registry;

    public DisableAdvertisingIdTweak(IRegistryAccess registry)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    }

    public TweakMetadata Metadata { get; } = new(
        Id: "privacy.disable_advertising_id",
        Name: "Disable advertising ID",
        ShortDescription: "Prevents apps from tracking the per-user advertising identifier across the system.",
        DetailedDescription: "The Windows advertising ID is a per-user identifier that UWP apps, Edge and Start Menu ads use to personalize content. Disabling it resets the identifier to all zeros and blocks apps from reading it, silencing most ad-driven telemetry paths.",
        Modifies: "HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\AdvertisingInfo (Enabled), HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\AdvertisingInfo (DisabledByGroupPolicy)",
        ExpectedImpact: "Removes the cross-app tracking identifier. No functional loss in gaming scenarios.",
        Category: TweakCategory.Privacy,
        Risk: TweakRisk.Safe,
        Evidence: TweakEvidenceLevel.Confirmed,
        Sources: new[]
        {
            "Microsoft Learn: Windows advertising ID",
            "Microsoft Privacy Settings documentation"
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
