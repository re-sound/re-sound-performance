using Microsoft.Win32;
using re_sound_performance.Core.Backup;
using re_sound_performance.Core.Registry;

namespace re_sound_performance.Core.Tweaks.Implementations;

public sealed class DisableStartupTipsTweak : ITweak
{
    private static readonly IReadOnlyList<RegistryChange> AppliedChanges = new[]
    {
        new RegistryChange(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338388Enabled", 0, RegistryValueKind.DWord),
        new RegistryChange(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338389Enabled", 0, RegistryValueKind.DWord),
        new RegistryChange(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-310093Enabled", 0, RegistryValueKind.DWord),
        new RegistryChange(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SystemPaneSuggestionsEnabled", 0, RegistryValueKind.DWord),
        new RegistryChange(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SilentInstalledAppsEnabled", 0, RegistryValueKind.DWord),
        new RegistryChange(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\CloudContent", "DisableWindowsConsumerFeatures", 1, RegistryValueKind.DWord)
    };

    private readonly IRegistryAccess _registry;

    public DisableStartupTipsTweak(IRegistryAccess registry)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    }

    public TweakMetadata Metadata { get; } = new(
        Id: "privacy.disable_tips_and_suggestions",
        Name: "Disable Windows tips, suggestions and consumer features",
        ShortDescription: "Stops Windows from showing tips on the lock screen, Start suggestions and silent app installs.",
        DetailedDescription: "Content Delivery Manager is responsible for the Windows tips that appear on the lock screen, the suggested apps in Start and the silent installation of promoted UWP apps such as TikTok or Spotify. DisableWindowsConsumerFeatures blocks the entire recommendation channel.",
        Modifies: "HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\ContentDeliveryManager (several SubscribedContent keys, SystemPaneSuggestionsEnabled, SilentInstalledAppsEnabled), HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\CloudContent (DisableWindowsConsumerFeatures)",
        ExpectedImpact: "Removes ad-driven Start suggestions and lock screen tips. Prevents unexpected UWP app installs.",
        Category: TweakCategory.Privacy,
        Risk: TweakRisk.Safe,
        Evidence: TweakEvidenceLevel.Confirmed,
        Sources: new[]
        {
            "Microsoft Learn: DisableWindowsConsumerFeatures policy",
            "Microsoft Learn: Content Delivery Manager configuration"
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
