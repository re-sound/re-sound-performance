using re_sound_performance.Core.Backup;
using re_sound_performance.Core.Services;

namespace re_sound_performance.Core.Tweaks.Implementations;

public sealed class DisableWerAndSysMainTweak : ITweak
{
    private static readonly IReadOnlyList<ServiceChange> AppliedChanges = new[]
    {
        new ServiceChange("WerSvc", ServiceStartupType.Disabled),
        new ServiceChange("WerReportingForScenarioHealthy", ServiceStartupType.Disabled),
        new ServiceChange("SysMain", ServiceStartupType.Disabled)
    };

    private readonly IServiceManager _services;

    public DisableWerAndSysMainTweak(IServiceManager services)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
    }

    public TweakMetadata Metadata { get; } = new(
        Id: "system.disable_wer_and_sysmain",
        Name: "Disable Windows Error Reporting and SysMain (Superfetch)",
        ShortDescription: "Stops crash reporting uploads and the SuperFetch prefetcher that is optional on modern NVMe systems.",
        DetailedDescription: "WerSvc uploads crash dumps to Microsoft and has no benefit for end users. SysMain (the successor to Superfetch) preloads application data based on usage patterns. On SATA HDDs SysMain helped, but on NVMe SSDs it creates unnecessary disk I/O competing with games. Disabling both is a standard gaming optimization for SSD-only systems.",
        Modifies: "Service startup type for WerSvc, WerReportingForScenarioHealthy, SysMain",
        ExpectedImpact: "Reduces idle disk activity on NVMe systems. Eliminates unsolicited crash uploads. On HDD-only setups, leave SysMain enabled.",
        Category: TweakCategory.System,
        Risk: TweakRisk.Medium,
        Evidence: TweakEvidenceLevel.Controversial,
        Sources: new[]
        {
            "Microsoft Learn: Windows Error Reporting overview",
            "Microsoft Q&A: SysMain behavior on NVMe SSDs",
            "XDA developers: Most Windows optimization guides are nonsense"
        },
        IncompatibleWith: new[] { "hdd-primary-drive" },
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
