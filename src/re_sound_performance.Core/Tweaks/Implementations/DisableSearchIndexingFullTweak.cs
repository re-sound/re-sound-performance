using Microsoft.Win32;
using re_sound_performance.Core.Backup;
using re_sound_performance.Core.Registry;

namespace re_sound_performance.Core.Tweaks.Implementations;

public sealed class DisableSearchIndexingFullTweak : ITweak
{
    private static readonly IReadOnlyList<RegistryChange> AppliedChanges = new[]
    {
        new RegistryChange(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Windows Search", "PreventIndexingOutlook", 1, RegistryValueKind.DWord),
        new RegistryChange(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Windows Search", "DisableSearchBoxSuggestions", 1, RegistryValueKind.DWord),
        new RegistryChange(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Windows Search", "AllowSearchToUseLocation", 0, RegistryValueKind.DWord),
        new RegistryChange(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Windows Search", "ConnectedSearchUseWeb", 0, RegistryValueKind.DWord),
        new RegistryChange(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Search", "CortanaConsent", 0, RegistryValueKind.DWord),
        new RegistryChange(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Search", "BingSearchEnabled", 0, RegistryValueKind.DWord)
    };

    private readonly IRegistryAccess _registry;

    public DisableSearchIndexingFullTweak(IRegistryAccess registry)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    }

    public TweakMetadata Metadata { get; } = new(
        Id: "privacy.disable_bing_and_web_search",
        Name: "Disable Bing, web search and Cortana in Start",
        ShortDescription: "Stops the Start Menu from sending search queries to Bing and loading web results.",
        DetailedDescription: "Every search typed into Start Menu is forwarded to Bing by default. The feature triggers network activity, loads Edge processes as fallback renderers and leaks the query to Microsoft. Disabling web search keeps Start Menu search strictly local, faster and quieter.",
        Modifies: "HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\Windows Search, HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Search",
        ExpectedImpact: "Start Menu search responds faster and does not call out to the internet. No functional loss for local file and app search.",
        Category: TweakCategory.Privacy,
        Risk: TweakRisk.Safe,
        Evidence: TweakEvidenceLevel.Confirmed,
        Sources: new[]
        {
            "Microsoft Learn: ConnectedSearchUseWeb policy",
            "Microsoft Learn: Disable Bing Search in Start Menu"
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
