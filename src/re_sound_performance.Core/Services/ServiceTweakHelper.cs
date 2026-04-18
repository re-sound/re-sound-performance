using System.Text.Json;
using re_sound_performance.Core.Backup;
using re_sound_performance.Core.Tweaks;

namespace re_sound_performance.Core.Services;

public static class ServiceTweakHelper
{
    private const string BackupPayloadName = "services.json";

    public static Task<TweakStatus> ProbeAsync(IServiceManager services, IReadOnlyList<ServiceChange> expectedChanges)
    {
        var anyApplied = false;
        var allApplied = true;
        var anyExists = false;

        foreach (var change in expectedChanges)
        {
            if (!services.ServiceExists(change.ServiceName))
            {
                continue;
            }

            anyExists = true;
            var current = services.GetStartupType(change.ServiceName);
            if (current == change.TargetStartupType)
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
        IServiceManager services,
        IBackupStore backupStore,
        string tweakId,
        IReadOnlyList<ServiceChange> changes,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(backupStore);
        ArgumentException.ThrowIfNullOrWhiteSpace(tweakId);
        ArgumentNullException.ThrowIfNull(changes);

        var snapshots = new List<ServiceStateSnapshot>(changes.Count);
        foreach (var change in changes)
        {
            var exists = services.ServiceExists(change.ServiceName);
            var currentStartup = exists ? services.GetStartupType(change.ServiceName) : null;
            snapshots.Add(new ServiceStateSnapshot(change.ServiceName, exists, currentStartup));
        }

        var payload = JsonSerializer.SerializeToUtf8Bytes(snapshots);
        var backupReference = await backupStore.SaveAsync(tweakId, BackupPayloadName, payload, cancellationToken).ConfigureAwait(false);

        var applied = 0;
        var skipped = 0;
        foreach (var change in changes)
        {
            if (!services.ServiceExists(change.ServiceName))
            {
                skipped++;
                continue;
            }

            services.SetStartupType(change.ServiceName, change.TargetStartupType);
            applied++;
        }

        if (applied == 0)
        {
            return TweakResult.Ok(tweakId, TweakStatus.Unavailable, new[] { backupReference }, "No matching services exist on this system.");
        }

        var message = skipped > 0 ? $"Applied to {applied} of {changes.Count} services ({skipped} not present)." : null;
        return TweakResult.Ok(tweakId, TweakStatus.Applied, new[] { backupReference }, message);
    }

    public static async Task<TweakResult> RevertAsync(
        IServiceManager services,
        IBackupStore backupStore,
        string tweakId,
        IReadOnlyList<ServiceChange> defaultChanges,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(services);
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

        var snapshots = JsonSerializer.Deserialize<List<ServiceStateSnapshot>>(payload)
            ?? throw new InvalidOperationException($"Could not deserialize backup for tweak {tweakId}.");

        foreach (var snapshot in snapshots)
        {
            if (!snapshot.Existed || snapshot.OriginalStartupType is null)
            {
                continue;
            }

            if (!services.ServiceExists(snapshot.ServiceName))
            {
                continue;
            }

            services.SetStartupType(snapshot.ServiceName, snapshot.OriginalStartupType.Value);
        }

        return TweakResult.Ok(tweakId, TweakStatus.NotApplied, new[] { latestReference });
    }
}
