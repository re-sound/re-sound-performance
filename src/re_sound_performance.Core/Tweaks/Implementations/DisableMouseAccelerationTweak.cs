using Microsoft.Win32;
using re_sound_performance.Core.Backup;
using re_sound_performance.Core.Registry;

namespace re_sound_performance.Core.Tweaks.Implementations;

public sealed class DisableMouseAccelerationTweak : ITweak
{
    private static readonly IReadOnlyList<RegistryChange> AppliedChanges = new[]
    {
        new RegistryChange(RegistryHive.CurrentUser, @"Control Panel\Mouse", "MouseSpeed", "0", RegistryValueKind.String),
        new RegistryChange(RegistryHive.CurrentUser, @"Control Panel\Mouse", "MouseThreshold1", "0", RegistryValueKind.String),
        new RegistryChange(RegistryHive.CurrentUser, @"Control Panel\Mouse", "MouseThreshold2", "0", RegistryValueKind.String)
    };

    private readonly IRegistryAccess _registry;

    public DisableMouseAccelerationTweak(IRegistryAccess registry)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    }

    public TweakMetadata Metadata { get; } = new(
        Id: "input.disable_mouse_acceleration",
        Name: "Disable Enhance Pointer Precision",
        ShortDescription: "Removes the non-linear mouse acceleration curve Windows applies by default.",
        DetailedDescription: "Enhance Pointer Precision (mouse acceleration) multiplies cursor movement based on speed, breaking 1:1 tracking. Disabling it is the universal choice for competitive aim in every FPS since 2010 and is required for muscle memory consistency across games.",
        Modifies: "HKCU\\Control Panel\\Mouse (MouseSpeed, MouseThreshold1, MouseThreshold2)",
        ExpectedImpact: "Consistent 1:1 mouse tracking. Matches how raw input games handle the mouse already, so cross-title sensitivity feels identical.",
        Category: TweakCategory.Input,
        Risk: TweakRisk.Safe,
        Evidence: TweakEvidenceLevel.Confirmed,
        Sources: new[]
        {
            "Microsoft Docs: Pointer precision option",
            "Blur Busters: Input Lag and Mouse Acceleration"
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
