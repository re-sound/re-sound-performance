using re_sound_performance.Core.Appx;
using re_sound_performance.Core.Backup;

namespace re_sound_performance.Core.Tweaks.Implementations;

public sealed class RemoveTeamsConsumerTweak : ITweak
{
    private static readonly IReadOnlyList<string> PackageNames = new[]
    {
        "MicrosoftTeams"
    };

    private readonly IAppxManager _appx;

    public RemoveTeamsConsumerTweak(IAppxManager appx)
    {
        _appx = appx ?? throw new ArgumentNullException(nameof(appx));
    }

    public TweakMetadata Metadata { get; } = new(
        Id: "debloat.remove_teams_consumer",
        Name: "Remove Microsoft Teams (consumer)",
        ShortDescription: "Uninstalls the pre-installed consumer version of Microsoft Teams.",
        DetailedDescription: "The consumer Teams app is provisioned by default on Windows 11 and launches a resident MsTeams.exe process that consumes memory even when nobody is signed in. Uninstalling it does not affect the work or school Teams installations, which are separate Win32 apps.",
        Modifies: "Removes the MicrosoftTeams Appx package for all users and removes its provisioning for new users.",
        ExpectedImpact: "Recovers 150-300 MB of RAM used by the resident Teams process. No impact on any workplace Teams deployment.",
        Category: TweakCategory.Debloat,
        Risk: TweakRisk.Safe,
        Evidence: TweakEvidenceLevel.Confirmed,
        Sources: new[]
        {
            "Microsoft Learn: Appx package management",
            "Windows 11 provisioning baseline audit"
        },
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
