using System.Text.Json;
using re_sound_performance.Core.Backup;
using re_sound_performance.Core.Tweaks;

namespace re_sound_performance.Core.Tasks;

public static class ScheduledTaskHelper
{
    private const string BackupPayloadName = "scheduled_tasks.json";

    public static Task<TweakStatus> ProbeAsync(IScheduledTaskManager manager, IReadOnlyList<ScheduledTaskChange> expectedChanges)
    {
        ArgumentNullException.ThrowIfNull(manager);
        ArgumentNullException.ThrowIfNull(expectedChanges);

        var anyApplied = false;
        var allApplied = true;
        var anyExists = false;

        foreach (var change in expectedChanges)
        {
            if (!manager.TaskExists(change.TaskPath))
            {
                continue;
            }

            anyExists = true;
            var current = manager.GetState(change.TaskPath);
            if (current == change.TargetState)
            {
                anyApplied = true;
            }
            else
            {
                allApplied = false;
            }
        }

        if (!anyExists)
        {
            return Task.FromResult(TweakStatus.Unavailable);
        }

        if (allApplied)
        {
            return Task.FromResult(TweakStatus.Applied);
        }

        return Task.FromResult(anyApplied ? TweakStatus.PartiallyApplied : TweakStatus.NotApplied);
    }

    public static async Task<TweakResult> ApplyAsync(
        IScheduledTaskManager manager,
        IBackupStore backupStore,
        string tweakId,
        IReadOnlyList<ScheduledTaskChange> changes,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(manager);
        ArgumentNullException.ThrowIfNull(backupStore);
        ArgumentException.ThrowIfNullOrWhiteSpace(tweakId);
        ArgumentNullException.ThrowIfNull(changes);

        var snapshots = new List<ScheduledTaskStateSnapshot>(changes.Count);
        foreach (var change in changes)
        {
            var exists = manager.TaskExists(change.TaskPath);
            var currentState = exists ? manager.GetState(change.TaskPath) : null;
            snapshots.Add(new ScheduledTaskStateSnapshot(change.TaskPath, exists, currentState));
        }

        var payload = JsonSerializer.SerializeToUtf8Bytes(snapshots);
        var backupReference = await backupStore.SaveAsync(tweakId, BackupPayloadName, payload, cancellationToken).ConfigureAwait(false);

        var applied = 0;
        var skipped = 0;
        var accessDenied = 0;
        var failures = new List<string>();

        foreach (var change in changes)
        {
            if (!manager.TaskExists(change.TaskPath))
            {
                skipped++;
                continue;
            }

            var outcome = manager.TrySetState(change.TaskPath, change.TargetState);
            if (outcome.IsSuccess)
            {
                applied++;
                continue;
            }

            if (outcome.IsAccessDenied)
            {
                accessDenied++;
            }
            else
            {
                failures.Add(outcome.ErrorMessage ?? "Unknown error");
            }
        }

        if (applied == 0 && accessDenied > 0 && failures.Count == 0)
        {
            return TweakResult.Ok(
                tweakId,
                TweakStatus.Unavailable,
                new[] { backupReference },
                "These scheduled tasks are protected by TrustedInstaller and cannot be disabled via schtasks. Use a service-based tweak instead.");
        }

        if (applied == 0 && skipped == changes.Count)
        {
            return TweakResult.Ok(tweakId, TweakStatus.Unavailable, new[] { backupReference }, "No matching scheduled tasks exist on this system.");
        }

        if (applied == 0 && failures.Count > 0)
        {
            return TweakResult.Fail(tweakId, failures[0]);
        }

        string? message = null;
        if (skipped > 0 || accessDenied > 0)
        {
            var parts = new List<string> { $"Applied to {applied} of {changes.Count} tasks" };
            if (skipped > 0) parts.Add($"{skipped} not present");
            if (accessDenied > 0) parts.Add($"{accessDenied} protected by TrustedInstaller");
            message = string.Join(", ", parts) + ".";
        }

        var status = accessDenied > 0 ? TweakStatus.PartiallyApplied : TweakStatus.Applied;
        return TweakResult.Ok(tweakId, status, new[] { backupReference }, message);
    }

    public static async Task<TweakResult> RevertAsync(
        IScheduledTaskManager manager,
        IBackupStore backupStore,
        string tweakId,
        IReadOnlyList<ScheduledTaskChange> defaultChanges,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(manager);
        ArgumentNullException.ThrowIfNull(backupStore);
        ArgumentException.ThrowIfNullOrWhiteSpace(tweakId);

        var references = await backupStore.ListForTweakAsync(tweakId, cancellationToken).ConfigureAwait(false);
        if (references.Count == 0)
        {
            return TweakResult.Ok(tweakId, TweakStatus.NotApplied, message: "No backup found to revert from.");
        }

        var latestReference = references[0];
        var payload = await backupStore.LoadAsync(latestReference, cancellationToken).ConfigureAwait(false);
        if (payload is null || payload.Length == 0)
        {
            return TweakResult.Ok(tweakId, TweakStatus.NotApplied, message: "Backup payload missing.");
        }

        var snapshots = JsonSerializer.Deserialize<List<ScheduledTaskStateSnapshot>>(payload)
            ?? throw new InvalidOperationException($"Could not deserialize backup for tweak {tweakId}.");

        foreach (var snapshot in snapshots)
        {
            if (!snapshot.Existed || snapshot.OriginalState is null)
            {
                continue;
            }

            if (!manager.TaskExists(snapshot.TaskPath))
            {
                continue;
            }

            manager.SetState(snapshot.TaskPath, snapshot.OriginalState.Value);
        }

        return TweakResult.Ok(tweakId, TweakStatus.NotApplied, new[] { latestReference });
    }
}
