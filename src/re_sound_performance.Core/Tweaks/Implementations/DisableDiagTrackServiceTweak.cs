using re_sound_performance.Core.Backup;
using re_sound_performance.Core.Services;

namespace re_sound_performance.Core.Tweaks.Implementations;

public sealed class DisableDiagTrackServiceTweak : ITweak
{
    private static readonly IReadOnlyList<ServiceChange> AppliedChanges = new[]
    {
        new ServiceChange("DiagTrack", ServiceStartupType.Disabled),
        new ServiceChange("dmwappushservice", ServiceStartupType.Disabled),
        new ServiceChange("diagnosticshub.standardcollector.service", ServiceStartupType.Disabled)
    };

    private readonly IServiceManager _services;

    public DisableDiagTrackServiceTweak(IServiceManager services)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
    }

    public TweakMetadata Metadata { get; } = new(
        Id: "privacy.disable_diagtrack_services",
        Name: "Disable DiagTrack and WAP Push services",
        ShortDescription: "Stops the Connected User Experiences, Telemetry and WAP Push background services from starting at boot.",
        DetailedDescription: "DiagTrack is the main telemetry upload service on Windows 11. dmwappushservice handles WAP push messages used by diagnostic channels. diagnosticshub.standardcollector.service runs in parallel for additional diagnostic data. Disabling the three stops the optional telemetry pipeline at the service layer on top of the policy-based telemetry tweak.",
        Modifies: "Service startup type for DiagTrack, dmwappushservice, diagnosticshub.standardcollector.service",
        ExpectedImpact: "Removes a persistent background process and its network uploads. Measurable decrease in idle CPU and packet count.",
        Category: TweakCategory.Privacy,
        Risk: TweakRisk.Safe,
        Evidence: TweakEvidenceLevel.Confirmed,
        Sources: new[]
        {
            "Microsoft Learn: Connected User Experiences and Telemetry service",
            "Black Viper service configurations: DiagTrack recommendation"
        },
        IncompatibleWith: Array.Empty<string>(),
        RequiresRestart: true,
        BlockedWhenVanguardInstalled: false,
        BlockedWhenFaceitInstalled: false);

    public Task<TweakStatus> ProbeAsync(CancellationToken cancellationToken = default) =>
        ServiceTweakHelper.ProbeAsync(_services, AppliedChanges);

    public Task<TweakResult> ApplyAsync(IBackupStore backupStore, CancellationToken cancellationToken = default) =>
        ServiceTweakHelper.ApplyAsync(_services, backupStore, Metadata.Id, AppliedChanges, cancellationToken);

    public Task<TweakResult> RevertAsync(IBackupStore backupStore, CancellationToken cancellationToken = default) =>
        ServiceTweakHelper.RevertAsync(_services, backupStore, Metadata.Id, AppliedChanges, cancellationToken);
}
