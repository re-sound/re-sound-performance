using re_sound_performance.Core.Backup;
using re_sound_performance.Core.Tasks;

namespace re_sound_performance.Core.Tweaks.Implementations;

public sealed class DisableTelemetryScheduledTasksTweak : ITweak
{
    private static readonly IReadOnlyList<ScheduledTaskChange> AppliedChanges = new[]
    {
        new ScheduledTaskChange(@"\Microsoft\Windows\Application Experience\Microsoft Compatibility Appraiser", ScheduledTaskState.Disabled),
        new ScheduledTaskChange(@"\Microsoft\Windows\Application Experience\ProgramDataUpdater", ScheduledTaskState.Disabled),
        new ScheduledTaskChange(@"\Microsoft\Windows\Application Experience\StartupAppTask", ScheduledTaskState.Disabled),
        new ScheduledTaskChange(@"\Microsoft\Windows\Customer Experience Improvement Program\Consolidator", ScheduledTaskState.Disabled),
        new ScheduledTaskChange(@"\Microsoft\Windows\Customer Experience Improvement Program\UsbCeip", ScheduledTaskState.Disabled),
        new ScheduledTaskChange(@"\Microsoft\Windows\Customer Experience Improvement Program\KernelCeipTask", ScheduledTaskState.Disabled),
        new ScheduledTaskChange(@"\Microsoft\Windows\DiskDiagnostic\Microsoft-Windows-DiskDiagnosticDataCollector", ScheduledTaskState.Disabled),
        new ScheduledTaskChange(@"\Microsoft\Windows\Feedback\Siuf\DmClient", ScheduledTaskState.Disabled),
        new ScheduledTaskChange(@"\Microsoft\Windows\Feedback\Siuf\DmClientOnScenarioDownload", ScheduledTaskState.Disabled),
        new ScheduledTaskChange(@"\Microsoft\Windows\Windows Error Reporting\QueueReporting", ScheduledTaskState.Disabled)
    };

    private readonly IScheduledTaskManager _tasks;

    public DisableTelemetryScheduledTasksTweak(IScheduledTaskManager tasks)
    {
        _tasks = tasks ?? throw new ArgumentNullException(nameof(tasks));
    }

    public TweakMetadata Metadata { get; } = new(
        Id: "privacy.disable_telemetry_scheduled_tasks",
        Name: "Disable telemetry scheduled tasks",
        ShortDescription: "Disables the ten built-in Windows tasks that collect and upload diagnostic, CEIP and feedback data.",
        DetailedDescription: "Windows 11 ships with a set of Task Scheduler entries that periodically wake up to run the Compatibility Appraiser, the Customer Experience Improvement Program collectors, disk diagnostic dumps, feedback requests and error-report queueing. None of them are required for the OS to function and they are the main source of background CPU spikes on an idle machine.",
        Modifies: "Scheduled tasks under Microsoft\\Windows\\Application Experience, Customer Experience Improvement Program, DiskDiagnostic, Feedback\\Siuf and Windows Error Reporting",
        ExpectedImpact: "Removes a predictable source of idle CPU spikes and background network activity. Safe to disable on personal machines.",
        Category: TweakCategory.Privacy,
        Risk: TweakRisk.Safe,
        Evidence: TweakEvidenceLevel.Confirmed,
        Sources: new[]
        {
            "Microsoft Learn: Task Scheduler user experience tasks",
            "Windows 11 performance research: background tasks audit"
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
