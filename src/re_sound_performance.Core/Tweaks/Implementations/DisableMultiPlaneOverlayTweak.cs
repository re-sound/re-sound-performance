using Microsoft.Win32;
using re_sound_performance.Core.Backup;
using re_sound_performance.Core.Registry;

namespace re_sound_performance.Core.Tweaks.Implementations;

public sealed class DisableMultiPlaneOverlayTweak : ITweak
{
    private static readonly IReadOnlyList<RegistryChange> AppliedChanges = new[]
    {
        new RegistryChange(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\Dwm", "OverlayTestMode", 5, RegistryValueKind.DWord)
    };

    private readonly IRegistryAccess _registry;

    public DisableMultiPlaneOverlayTweak(IRegistryAccess registry)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    }

    public TweakMetadata Metadata { get; } = new(
        Id: "system.disable_mpo",
        Name: "Disable Multi-Plane Overlay (MPO)",
        ShortDescription: "Turns off the DWM multi-plane overlay feature that causes flickering and stutter on many NVIDIA and AMD setups.",
        DetailedDescription: "Multi-Plane Overlay is a WDDM feature that lets DWM compose separate surfaces in hardware. Combined with certain drivers, monitors and browsers it produces flicker, black flashes and stutter in fullscreen video and some games. NVIDIA has officially acknowledged the issue. Setting OverlayTestMode to 5 disables MPO system-wide.",
        Modifies: "HKLM\\SOFTWARE\\Microsoft\\Windows\\Dwm\\OverlayTestMode (DWORD 5)",
        ExpectedImpact: "Removes multi-monitor stutter, browser flicker and some fullscreen video artifacts. Small GPU overhead trade-off is negligible on modern cards.",
        Category: TweakCategory.System,
        Risk: TweakRisk.Safe,
        Evidence: TweakEvidenceLevel.Confirmed,
        Sources: new[]
        {
            "NVIDIA Support: MPO display issues",
            "Guru3D forum thread: Disabling MPO"
        },
        IncompatibleWith: Array.Empty<string>(),
        RequiresRestart: true,
        BlockedWhenVanguardInstalled: false,
        BlockedWhenFaceitInstalled: false);

    public Task<TweakStatus> ProbeAsync(CancellationToken cancellationToken = default) =>
        RegistryTweakHelper.ProbeAsync(_registry, AppliedChanges);

    public Task<TweakResult> ApplyAsync(IBackupStore backupStore, CancellationToken cancellationToken = default) =>
        RegistryTweakHelper.ApplyAsync(_registry, backupStore, Metadata.Id, AppliedChanges, cancellationToken);

    public Task<TweakResult> RevertAsync(IBackupStore backupStore, CancellationToken cancellationToken = default) =>
        RegistryTweakHelper.RevertAsync(_registry, backupStore, Metadata.Id, AppliedChanges, cancellationToken);
}
