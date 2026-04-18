using Microsoft.Win32;
using re_sound_performance.Core.Backup;
using re_sound_performance.Core.Registry;

namespace re_sound_performance.Core.Tweaks.Implementations;

public sealed class DisableLocationTrackingTweak : ITweak
{
    private static readonly IReadOnlyList<RegistryChange> AppliedChanges = new[]
    {
        new RegistryChange(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\location", "Value", "Deny", RegistryValueKind.String),
        new RegistryChange(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\LocationAndSensors", "DisableLocation", 1, RegistryValueKind.DWord),
        new RegistryChange(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\LocationAndSensors", "DisableLocationScripting", 1, RegistryValueKind.DWord),
        new RegistryChange(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Services\lfsvc\Service\Configuration", "Status", 0, RegistryValueKind.DWord)
    };

    private readonly IRegistryAccess _registry;

    public DisableLocationTrackingTweak(IRegistryAccess registry)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    }

    public TweakMetadata Metadata { get; } = new(
        Id: "privacy.disable_location_tracking",
        Name: "Disable location tracking",
        ShortDescription: "Denies access to the Location API for all apps and stops the Location Service.",
        DetailedDescription: "Windows Location Service exposes GPS and IP-based location to any UWP or Win32 app that requests it. Disabling it via policy and consent store is a stronger switch than the Settings toggle because it survives OS resets and reinstalls.",
        Modifies: "HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\CapabilityAccessManager\\ConsentStore\\location (Value), HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\LocationAndSensors, HKLM\\SYSTEM\\CurrentControlSet\\Services\\lfsvc\\Service\\Configuration (Status)",
        ExpectedImpact: "Removes a background service and prevents apps from reading the location. No functional loss for gaming.",
        Category: TweakCategory.Privacy,
        Risk: TweakRisk.Safe,
        Evidence: TweakEvidenceLevel.Confirmed,
        Sources: new[]
        {
            "Microsoft Learn: Disable Location Service Group Policy",
            "Microsoft Learn: Capability Access Manager consent store"
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
