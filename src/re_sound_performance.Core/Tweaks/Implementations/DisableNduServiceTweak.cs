using re_sound_performance.Core.Backup;
using re_sound_performance.Core.Services;

namespace re_sound_performance.Core.Tweaks.Implementations;

public sealed class DisableNduServiceTweak : ITweak
{
    private static readonly IReadOnlyList<ServiceChange> AppliedChanges = new[]
    {
        new ServiceChange("Ndu", ServiceStartupType.Disabled)
    };

    private readonly IServiceManager _services;

    public DisableNduServiceTweak(IServiceManager services)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
    }

    public TweakMetadata Metadata { get; } = new(
        Id: "network.disable_ndu",
        Name: "Disable Windows Network Data Usage (Ndu)",
        ShortDescription: "Disables the Network Data Usage monitoring driver known to cause memory leaks on some systems.",
        DetailedDescription: "The Ndu driver is responsible for the per-app network usage graph in Task Manager. On some Windows 11 builds it has been linked to progressive non-paged pool memory leaks during long uptime sessions. Disabling it removes the graph but frees the leak.",
        Modifies: "Service startup type for Ndu",
        ExpectedImpact: "Eliminates a known non-paged pool memory leak affecting long uptime sessions. No effect on actual network connectivity.",
        Category: TweakCategory.Network,
        Risk: TweakRisk.Safe,
        Evidence: TweakEvidenceLevel.Confirmed,
        Sources: new[]
        {
            "Microsoft Community: non-paged pool memory leak caused by Ndu.sys",
            "Superuser thread on Ndu memory usage"
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
