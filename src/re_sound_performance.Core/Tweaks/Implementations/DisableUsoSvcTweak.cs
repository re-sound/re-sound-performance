using re_sound_performance.Core.Backup;
using re_sound_performance.Core.Services;

namespace re_sound_performance.Core.Tweaks.Implementations;

public sealed class DisableUsoSvcTweak : ITweak
{
    private static readonly IReadOnlyList<ServiceChange> AppliedChanges = new[]
    {
        new ServiceChange("UsoSvc", ServiceStartupType.Disabled),
        new ServiceChange("WaaSMedicSvc", ServiceStartupType.Disabled)
    };

    private readonly IServiceManager _services;

    public DisableUsoSvcTweak(IServiceManager services)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
    }

    public TweakMetadata Metadata { get; } = new(
        Id: "system.disable_uso_service",
        Name: "Disable Update Orchestrator service",
        ShortDescription: "Disables UsoSvc so Windows does not spin up the Update Orchestrator in the background.",
        DetailedDescription: "The Update Orchestrator Service (UsoSvc) launches the scheduled-scan and wake tasks that cannot be disabled via schtasks because those tasks are protected by TrustedInstaller. Disabling the service itself achieves the same goal with normal administrator permissions: the tasks are still present, but they no longer have an orchestrator to run. WaaSMedicSvc is also disabled because it re-enables UsoSvc on Microsoft's own schedule. Manual update checks via Settings still work because they start the service on demand.",
        Modifies: "Service startup type for UsoSvc (Update Orchestrator) and WaaSMedicSvc (Windows Update Medic)",
        ExpectedImpact: "Removes background update scans and wake-for-update behavior without blocking the ability to check for updates manually.",
        Category: TweakCategory.System,
        Risk: TweakRisk.Medium,
        Evidence: TweakEvidenceLevel.Confirmed,
        Sources: new[]
        {
            "Microsoft Learn: Update Orchestrator service",
            "Sysinternals Process Explorer: UsoSvc process tree analysis"
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
