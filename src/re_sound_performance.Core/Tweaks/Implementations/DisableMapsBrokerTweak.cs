using re_sound_performance.Core.Backup;
using re_sound_performance.Core.Services;

namespace re_sound_performance.Core.Tweaks.Implementations;

public sealed class DisableMapsBrokerTweak : ITweak
{
    private static readonly IReadOnlyList<ServiceChange> AppliedChanges = new[]
    {
        new ServiceChange("MapsBroker", ServiceStartupType.Disabled)
    };

    private readonly IServiceManager _services;

    public DisableMapsBrokerTweak(IServiceManager services)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
    }

    public TweakMetadata Metadata { get; } = new(
        Id: "system.disable_maps_broker",
        Name: "Disable Downloaded Maps Manager",
        ShortDescription: "Stops the background service that keeps offline Maps data in sync.",
        DetailedDescription: "MapsBroker is only useful if you use the Windows Maps app offline. Disabling the service removes a background process that runs independently of whether Maps is ever launched.",
        Modifies: "Service startup type for MapsBroker",
        ExpectedImpact: "Negligible RAM and CPU savings. No functional loss for users who do not use Windows Maps offline.",
        Category: TweakCategory.System,
        Risk: TweakRisk.Safe,
        Evidence: TweakEvidenceLevel.Confirmed,
        Sources: new[]
        {
            "Microsoft Learn: Downloaded Maps Manager service",
            "Kartones blog: Windows 11 services to disable for gaming"
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
