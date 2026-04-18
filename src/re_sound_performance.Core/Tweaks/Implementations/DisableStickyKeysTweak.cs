using Microsoft.Win32;
using re_sound_performance.Core.Backup;
using re_sound_performance.Core.Registry;

namespace re_sound_performance.Core.Tweaks.Implementations;

public sealed class DisableStickyKeysTweak : ITweak
{
    private static readonly IReadOnlyList<RegistryChange> AppliedChanges = new[]
    {
        new RegistryChange(RegistryHive.CurrentUser, @"Control Panel\Accessibility\StickyKeys", "Flags", "506", RegistryValueKind.String),
        new RegistryChange(RegistryHive.CurrentUser, @"Control Panel\Accessibility\Keyboard Response", "Flags", "122", RegistryValueKind.String),
        new RegistryChange(RegistryHive.CurrentUser, @"Control Panel\Accessibility\ToggleKeys", "Flags", "58", RegistryValueKind.String)
    };

    private readonly IRegistryAccess _registry;

    public DisableStickyKeysTweak(IRegistryAccess registry)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    }

    public TweakMetadata Metadata { get; } = new(
        Id: "input.disable_accessibility_shortcuts",
        Name: "Disable Sticky, Filter and Toggle Keys",
        ShortDescription: "Turns off the three accessibility shortcuts triggered by holding Shift, Num Lock or Shift five times.",
        DetailedDescription: "Sticky Keys, Filter Keys and Toggle Keys show confirmation popups that steal focus from the active game. Pressing Shift rapidly or holding it during a match opens a dialog that forces the game into windowed mode or pauses input. Disabling the shortcut is safe and universally recommended.",
        Modifies: "HKCU\\Control Panel\\Accessibility\\StickyKeys, KeyboardResponse, ToggleKeys (Flags values set to disabled states)",
        ExpectedImpact: "Eliminates interrupt popups in game sessions. Does not affect users who rely on these accessibility features through Settings.",
        Category: TweakCategory.Input,
        Risk: TweakRisk.Safe,
        Evidence: TweakEvidenceLevel.Confirmed,
        Sources: new[]
        {
            "Microsoft Docs: Accessibility keyboard shortcuts",
            "Windows 11 Central: Disable accessibility shortcuts for gaming"
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
