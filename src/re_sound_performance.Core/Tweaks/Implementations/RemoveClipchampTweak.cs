using re_sound_performance.Core.Appx;
using re_sound_performance.Core.Backup;

namespace re_sound_performance.Core.Tweaks.Implementations;

public sealed class RemoveClipchampTweak : ITweak
{
    private static readonly IReadOnlyList<string> PackageNames = new[]
    {
        "Clipchamp.Clipchamp"
    };

    private readonly IAppxManager _appx;

    public RemoveClipchampTweak(IAppxManager appx)
    {
        _appx = appx ?? throw new ArgumentNullException(nameof(appx));
    }

    public TweakMetadata Metadata { get; } = new(
        Id: "debloat.remove_clipchamp",
        Name: "Remove Clipchamp",
        ShortDescription: "Uninstalls the pre-installed Clipchamp video editor.",
        DetailedDescription: "Clipchamp ships pre-installed on Windows 11 but most users never open it. Removing it reclaims disk space and eliminates its background update checks.",
        Modifies: "Removes the Clipchamp.Clipchamp Appx package for all users and its provisioning.",
        ExpectedImpact: "Recovers roughly 300 MB of disk and removes background update checks.",
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
