using re_sound_performance.Core.Appx;
using re_sound_performance.Core.Backup;

namespace re_sound_performance.Core.Tweaks.Implementations;

public sealed class RemoveBingAppsTweak : ITweak
{
    private static readonly IReadOnlyList<string> PackageNames = new[]
    {
        "Microsoft.BingNews",
        "Microsoft.BingWeather",
        "Microsoft.BingSearch"
    };

    private readonly IAppxManager _appx;

    public RemoveBingAppsTweak(IAppxManager appx)
    {
        _appx = appx ?? throw new ArgumentNullException(nameof(appx));
    }

    public TweakMetadata Metadata { get; } = new(
        Id: "debloat.remove_bing_apps",
        Name: "Remove Bing News, Weather and Search apps",
        ShortDescription: "Uninstalls the Bing News, Weather and Web Search tiles and their background agents.",
        DetailedDescription: "The Bing apps ship as part of the default Start menu and run background agents that fetch news, weather and search suggestions even when unused. Removing them stops the agents and reclaims the Start menu tiles.",
        Modifies: "Removes Microsoft.BingNews, Microsoft.BingWeather and Microsoft.BingSearch Appx packages and provisioning.",
        ExpectedImpact: "Stops recurring Bing background fetches and frees Start menu space.",
        Category: TweakCategory.Debloat,
        Risk: TweakRisk.Safe,
        Evidence: TweakEvidenceLevel.Confirmed,
        Sources: new[] { "Microsoft Learn: Bing web search Appx components" },
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
