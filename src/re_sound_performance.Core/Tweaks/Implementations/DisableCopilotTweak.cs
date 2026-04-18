using Microsoft.Win32;
using re_sound_performance.Core.Backup;
using re_sound_performance.Core.Registry;

namespace re_sound_performance.Core.Tweaks.Implementations;

public sealed class DisableCopilotTweak : ITweak
{
    private static readonly IReadOnlyList<RegistryChange> AppliedChanges = new[]
    {
        new RegistryChange(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\WindowsCopilot", "TurnOffWindowsCopilot", 1, RegistryValueKind.DWord),
        new RegistryChange(RegistryHive.CurrentUser, @"Software\Policies\Microsoft\Windows\WindowsCopilot", "TurnOffWindowsCopilot", 1, RegistryValueKind.DWord),
        new RegistryChange(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\WindowsAI", "DisableAIDataAnalysis", 1, RegistryValueKind.DWord),
        new RegistryChange(RegistryHive.CurrentUser, @"Software\Policies\Microsoft\Windows\WindowsAI", "DisableAIDataAnalysis", 1, RegistryValueKind.DWord),
        new RegistryChange(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\WindowsAI", "TurnOffSavingSnapshots", 1, RegistryValueKind.DWord),
        new RegistryChange(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\WindowsAI", "AllowRecallEnablement", 0, RegistryValueKind.DWord)
    };

    private readonly IRegistryAccess _registry;

    public DisableCopilotTweak(IRegistryAccess registry)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    }

    public TweakMetadata Metadata { get; } = new(
        Id: "privacy.disable_copilot_recall",
        Name: "Disable Copilot and Recall",
        ShortDescription: "Turns off the Copilot side panel, the AI data analysis pipeline and Recall snapshot capture.",
        DetailedDescription: "Copilot keeps a resident service that consumes between 500 MB and 1 GB of RAM while passive. Recall periodically captures screenshots and feeds them through an on-device AI index. Both features run background work that competes with games. Disabling them via policy removes the overhead and stops Recall snapshot storage.",
        Modifies: "HKLM+HKCU WindowsCopilot (TurnOffWindowsCopilot), HKLM+HKCU WindowsAI (DisableAIDataAnalysis, TurnOffSavingSnapshots, AllowRecallEnablement)",
        ExpectedImpact: "Recovers 500 MB to 1.2 GB of RAM and removes background AI capture overhead. No functional loss for gaming.",
        Category: TweakCategory.Privacy,
        Risk: TweakRisk.Safe,
        Evidence: TweakEvidenceLevel.Confirmed,
        Sources: new[]
        {
            "Microsoft Learn: Manage Recall",
            "Microsoft Learn: TurnOffWindowsCopilot Group Policy",
            "Dev.to: Windows 11 Recall performance measurements"
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
