using re_sound_performance.Core.Appx;
using re_sound_performance.Core.Backup;

namespace re_sound_performance.Core.Tweaks.Implementations;

public sealed class RemoveRecallAppTweak : ITweak
{
    private static readonly IReadOnlyList<string> PackageNames = new[]
    {
        "Microsoft.Windows.AI.Copilot",
        "MicrosoftWindows.Client.AIX"
    };

    private readonly IAppxManager _appx;

    public RemoveRecallAppTweak(IAppxManager appx)
    {
        _appx = appx ?? throw new ArgumentNullException(nameof(appx));
    }

    public TweakMetadata Metadata { get; } = new(
        Id: "debloat.remove_recall_app",
        Name: "Remove Recall / Windows AI",
        ShortDescription: "Uninstalls the Windows AI Copilot runtime and Recall surface packages.",
        DetailedDescription: "Microsoft.Windows.AI.Copilot and the MicrosoftWindows.Client.AIX packages host the Recall snapshot pipeline. Removing them prevents Recall from running even if its policy toggle is re-enabled later.",
        Modifies: "Removes Microsoft.Windows.AI.Copilot and MicrosoftWindows.Client.AIX Appx packages and provisioning.",
        ExpectedImpact: "Prevents Recall from capturing or indexing screenshots. Pair with the Disable Copilot and Recall tweak for full coverage.",
        Category: TweakCategory.Debloat,
        Risk: TweakRisk.Medium,
        Evidence: TweakEvidenceLevel.Controversial,
        Sources: new[] { "Microsoft Learn: Recall manageability" },
        IncompatibleWith: Array.Empty<string>(),
        RequiresRestart: true,
        BlockedWhenVanguardInstalled: false,
        BlockedWhenFaceitInstalled: false);

    public Task<TweakStatus> ProbeAsync(CancellationToken cancellationToken = default) =>
        AppxTweakHelper.ProbeAsync(_appx, PackageNames);

    public Task<TweakResult> ApplyAsync(IBackupStore backupStore, CancellationToken cancellationToken = default) =>
        AppxTweakHelper.ApplyAsync(_appx, backupStore, Metadata.Id, PackageNames, cancellationToken);

    public Task<TweakResult> RevertAsync(IBackupStore backupStore, CancellationToken cancellationToken = default) =>
        AppxTweakHelper.RevertAsync(_appx, backupStore, Metadata.Id, PackageNames, cancellationToken);
}
