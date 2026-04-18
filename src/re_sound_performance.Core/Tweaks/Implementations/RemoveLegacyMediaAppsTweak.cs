using re_sound_performance.Core.Appx;
using re_sound_performance.Core.Backup;

namespace re_sound_performance.Core.Tweaks.Implementations;

public sealed class RemoveLegacyMediaAppsTweak : ITweak
{
    private static readonly IReadOnlyList<string> PackageNames = new[]
    {
        "Microsoft.ZuneMusic",
        "Microsoft.ZuneVideo",
        "Microsoft.MicrosoftSolitaireCollection",
        "Microsoft.Office.OneNote"
    };

    private readonly IAppxManager _appx;

    public RemoveLegacyMediaAppsTweak(IAppxManager appx)
    {
        _appx = appx ?? throw new ArgumentNullException(nameof(appx));
    }

    public TweakMetadata Metadata { get; } = new(
        Id: "debloat.remove_legacy_media_apps",
        Name: "Remove legacy media and Solitaire",
        ShortDescription: "Uninstalls Groove Music, Movies & TV, Microsoft Solitaire Collection and the UWP OneNote app.",
        DetailedDescription: "The legacy Zune-derived media apps, Solitaire Collection and UWP OneNote are provisioned by default but largely superseded by modern alternatives. Each brings its own background agent for update checks. Removing them frees RAM that is otherwise spent on resident entry points.",
        Modifies: "Removes Microsoft.ZuneMusic, Microsoft.ZuneVideo, Microsoft.MicrosoftSolitaireCollection and Microsoft.Office.OneNote Appx packages.",
        ExpectedImpact: "Removes four idle-running Appx entries. If you play Solitaire or open music via the pre-installed app, keep this tweak Off.",
        Category: TweakCategory.Debloat,
        Risk: TweakRisk.Safe,
        Evidence: TweakEvidenceLevel.Confirmed,
        Sources: new[] { "Windows 11 provisioning baseline audit" },
        IncompatibleWith: Array.Empty<string>(),
        RequiresRestart: false,
        BlockedWhenVanguardInstalled: false,
        BlockedWhenFaceitInstalled: false);

    public Task<TweakStatus> ProbeAsync(CancellationToken cancellationToken = default) =>
        AppxTweakHelper.ProbeAsync(_appx, PackageNames);

    public Task<TweakResult> ApplyAsync(IBackupStore backupStore, CancellationToken cancellationToken = default) =>
        AppxTweakHelper.ApplyAsync(_appx, backupStore, Metadata.Id, PackageNames, cancellationToken);

    public Task<TweakResult> RevertAsync(IBackupStore backupStore, CancellationToken cancellationToken = default) =>
        AppxTweakHelper.RevertAsync(_appx, backupStore, Metadata.Id, PackageNames, cancellationToken);
}
