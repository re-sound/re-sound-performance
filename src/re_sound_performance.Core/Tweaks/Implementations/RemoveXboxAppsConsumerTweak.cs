using re_sound_performance.Core.Appx;
using re_sound_performance.Core.Backup;

namespace re_sound_performance.Core.Tweaks.Implementations;

public sealed class RemoveXboxAppsConsumerTweak : ITweak
{
    private static readonly IReadOnlyList<string> PackageNames = new[]
    {
        "Microsoft.XboxApp",
        "Microsoft.GamingApp",
        "Microsoft.Xbox.TCUI",
        "Microsoft.XboxGamingOverlay",
        "Microsoft.XboxIdentityProvider",
        "Microsoft.XboxSpeechToTextOverlay"
    };

    private readonly IAppxManager _appx;

    public RemoveXboxAppsConsumerTweak(IAppxManager appx)
    {
        _appx = appx ?? throw new ArgumentNullException(nameof(appx));
    }

    public TweakMetadata Metadata { get; } = new(
        Id: "debloat.remove_xbox_apps_consumer",
        Name: "Remove Xbox consumer apps",
        ShortDescription: "Uninstalls the Xbox consumer apps and Game Bar helper packages.",
        DetailedDescription: "If the machine is not used with Game Pass or Xbox services, the XboxApp, GamingApp, Game Bar helpers and Xbox Identity Provider can be removed. This also stops the XboxGamingOverlay hook that some users report as a source of stutter. If you use Xbox Live achievements or PC Game Pass, keep this tweak Off.",
        Modifies: "Removes six Microsoft.Xbox* Appx packages including the Gaming App and Game Bar helpers.",
        ExpectedImpact: "Removes residual Xbox overlay and helper processes. Breaks Xbox Live achievements for titles that rely on them.",
        Category: TweakCategory.Debloat,
        Risk: TweakRisk.Medium,
        Evidence: TweakEvidenceLevel.Confirmed,
        Sources: new[] { "Microsoft Learn: Gaming app provisioning" },
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
