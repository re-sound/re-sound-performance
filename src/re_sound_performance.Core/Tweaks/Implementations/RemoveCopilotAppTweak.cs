using re_sound_performance.Core.Appx;
using re_sound_performance.Core.Backup;

namespace re_sound_performance.Core.Tweaks.Implementations;

public sealed class RemoveCopilotAppTweak : ITweak
{
    private static readonly IReadOnlyList<string> PackageNames = new[]
    {
        "Microsoft.Copilot",
        "Microsoft.Windows.Copilot"
    };

    private readonly IAppxManager _appx;

    public RemoveCopilotAppTweak(IAppxManager appx)
    {
        _appx = appx ?? throw new ArgumentNullException(nameof(appx));
    }

    public TweakMetadata Metadata { get; } = new(
        Id: "debloat.remove_copilot_app",
        Name: "Remove Copilot app",
        ShortDescription: "Uninstalls the standalone Copilot Appx package.",
        DetailedDescription: "Separate from the taskbar Copilot icon, the Microsoft.Copilot Appx package installs a full desktop Copilot assistant. Removing it ensures there is no background Copilot process running.",
        Modifies: "Removes Microsoft.Copilot and Microsoft.Windows.Copilot Appx packages for all users.",
        ExpectedImpact: "Removes the Copilot assistant from installed apps. Pair with the Disable Copilot and Recall tweak under Privacy for full coverage.",
        Category: TweakCategory.Debloat,
        Risk: TweakRisk.Safe,
        Evidence: TweakEvidenceLevel.Confirmed,
        Sources: new[] { "Microsoft Learn: Manage Copilot" },
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
