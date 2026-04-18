using re_sound_performance.Core.Appx;
using re_sound_performance.Core.Backup;

namespace re_sound_performance.Core.Tweaks.Implementations;

public sealed class RemoveStockAnnoyancesTweak : ITweak
{
    private static readonly IReadOnlyList<string> PackageNames = new[]
    {
        "Microsoft.GetHelp",
        "Microsoft.Getstarted",
        "Microsoft.WindowsFeedbackHub",
        "Microsoft.MixedReality.Portal"
    };

    private readonly IAppxManager _appx;

    public RemoveStockAnnoyancesTweak(IAppxManager appx)
    {
        _appx = appx ?? throw new ArgumentNullException(nameof(appx));
    }

    public TweakMetadata Metadata { get; } = new(
        Id: "debloat.remove_stock_annoyances",
        Name: "Remove stock annoyances (Get Help, Tips, Feedback Hub, Mixed Reality)",
        ShortDescription: "Uninstalls the four least-used stock Windows 11 apps.",
        DetailedDescription: "Get Help, Tips, Feedback Hub and Mixed Reality Portal are provisioned by default and rarely launched. None of them provide functionality that cannot be accessed through Settings or a browser. Removing them clears the Start menu and stops their auto-update background work.",
        Modifies: "Removes Microsoft.GetHelp, Microsoft.Getstarted, Microsoft.WindowsFeedbackHub and Microsoft.MixedReality.Portal Appx packages.",
        ExpectedImpact: "Removes four Start menu tiles and their periodic update checks. No functional loss.",
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
