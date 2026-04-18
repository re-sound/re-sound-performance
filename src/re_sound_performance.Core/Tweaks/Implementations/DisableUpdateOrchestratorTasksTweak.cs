using re_sound_performance.Core.Backup;
using re_sound_performance.Core.Tasks;

namespace re_sound_performance.Core.Tweaks.Implementations;

public sealed class DisableUpdateOrchestratorTasksTweak : ITweak
{
    private static readonly IReadOnlyList<ScheduledTaskChange> AppliedChanges = new[]
    {
        new ScheduledTaskChange(@"\Microsoft\Windows\UpdateOrchestrator\Schedule Scan", ScheduledTaskState.Disabled),
        new ScheduledTaskChange(@"\Microsoft\Windows\UpdateOrchestrator\Schedule Scan Static Task", ScheduledTaskState.Disabled),
        new ScheduledTaskChange(@"\Microsoft\Windows\UpdateOrchestrator\Schedule Wake To Work", ScheduledTaskState.Disabled),
        new ScheduledTaskChange(@"\Microsoft\Windows\UpdateOrchestrator\USO_UxBroker", ScheduledTaskState.Disabled)
    };

    private readonly IScheduledTaskManager _tasks;

    public DisableUpdateOrchestratorTasksTweak(IScheduledTaskManager tasks)
    {
        _tasks = tasks ?? throw new ArgumentNullException(nameof(tasks));
    }

    public TweakMetadata Metadata { get; } = new(
        Id: "system.disable_update_orchestrator_tasks",
        Name: "Disable Update Orchestrator scheduled tasks",
        ShortDescription: "Prevents Windows Update from waking the machine and running scan tasks outside of the manual check flow.",
        DetailedDescription: "The Update Orchestrator runs scan, wake and UX broker tasks that can spin up the disk and network even when Windows Update is set to manual checking. Disabling them does not disable Windows Update itself; manual checks via Settings still work and security updates can still be installed. The tradeoff is that scheduled automatic scans are paused until the user opens Settings.",
        Modifies: "Scheduled tasks under Microsoft\\Windows\\UpdateOrchestrator (Schedule Scan, Schedule Scan Static Task, Schedule Wake To Work, USO_UxBroker)",
        ExpectedImpact: "Stops unexpected CPU, disk and network activity caused by background update scans. Requires manual update checks to keep the system patched.",
        Category: TweakCategory.System,
        Risk: TweakRisk.Medium,
        Evidence: TweakEvidenceLevel.Confirmed,
        Sources: new[]
        {
            "Microsoft Learn: Manage Windows Update for Business",
            "Sysinternals: update orchestrator task analysis"
        },
        IncompatibleWith: Array.Empty<string>(),
        RequiresRestart: false,
        BlockedWhenVanguardInstalled: false,
        BlockedWhenFaceitInstalled: false);

    public Task<TweakStatus> ProbeAsync(CancellationToken cancellationToken = default) =>
        ScheduledTaskHelper.ProbeAsync(_tasks, AppliedChanges);

    public Task<TweakResult> ApplyAsync(IBackupStore backupStore, CancellationToken cancellationToken = default) =>
        ScheduledTaskHelper.ApplyAsync(_tasks, backupStore, Metadata.Id, AppliedChanges, cancellationToken);

    public Task<TweakResult> RevertAsync(IBackupStore backupStore, CancellationToken cancellationToken = default) =>
        ScheduledTaskHelper.RevertAsync(_tasks, backupStore, Metadata.Id, AppliedChanges, cancellationToken);
}
