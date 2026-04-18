using System.Text.Json;
using re_sound_performance.Core.Backup;
using re_sound_performance.Core.Tweaks;

namespace re_sound_performance.Core.Appx;

public static class AppxTweakHelper
{
    private const string BackupPayloadName = "appx.json";

    public static Task<TweakStatus> ProbeAsync(IAppxManager manager, IReadOnlyList<string> packageNames)
    {
        ArgumentNullException.ThrowIfNull(manager);
        ArgumentNullException.ThrowIfNull(packageNames);

        var anyPresent = false;
        var anyAbsent = false;

        foreach (var name in packageNames)
        {
            var installed = manager.FindInstalled(name).Count > 0 || manager.IsProvisioned(name);
            if (installed)
            {
                anyPresent = true;
            }
            else
            {
                anyAbsent = true;
            }
        }

        if (!anyPresent)
        {
            return Task.FromResult(TweakStatus.Applied);
        }

        return Task.FromResult(anyAbsent ? TweakStatus.PartiallyApplied : TweakStatus.NotApplied);
    }

    public static async Task<TweakResult> ApplyAsync(
        IAppxManager manager,
        IBackupStore backupStore,
        string tweakId,
        IReadOnlyList<string> packageNames,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(manager);
        ArgumentNullException.ThrowIfNull(backupStore);
        ArgumentException.ThrowIfNullOrWhiteSpace(tweakId);
        ArgumentNullException.ThrowIfNull(packageNames);

        var snapshots = new List<AppxStateSnapshot>(packageNames.Count);
        foreach (var name in packageNames)
        {
            var installed = manager.FindInstalled(name);
            var provisioned = manager.IsProvisioned(name);
            snapshots.Add(new AppxStateSnapshot(
                name,
                installed.Count > 0,
                provisioned,
                installed.Select(p => p.PackageFullName).ToArray()));
        }

        var payload = JsonSerializer.SerializeToUtf8Bytes(snapshots);
        var backupReference = await backupStore.SaveAsync(tweakId, BackupPayloadName, payload, cancellationToken).ConfigureAwait(false);

        var removed = 0;
        var notFound = 0;
        foreach (var name in packageNames)
        {
            var installed = manager.FindInstalled(name);
            var provisioned = manager.IsProvisioned(name);

            if (installed.Count == 0 && !provisioned)
            {
                notFound++;
                continue;
            }

            if (installed.Count > 0)
            {
                manager.RemoveForAllUsers(name);
            }

            if (provisioned)
            {
                manager.RemoveProvisioned(name);
            }

            removed++;
        }

        if (removed == 0)
        {
            return TweakResult.Ok(tweakId, TweakStatus.Applied, new[] { backupReference }, "No matching packages were present; nothing to remove.");
        }

        var message = notFound > 0
            ? $"Removed {removed} package(s); {notFound} were already absent."
            : $"Removed {removed} package(s).";
        return TweakResult.Ok(tweakId, TweakStatus.Applied, new[] { backupReference }, message);
    }

    public static async Task<TweakResult> RevertAsync(
        IAppxManager manager,
        IBackupStore backupStore,
        string tweakId,
        IReadOnlyList<string> packageNames,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(manager);
        ArgumentNullException.ThrowIfNull(backupStore);
        ArgumentException.ThrowIfNullOrWhiteSpace(tweakId);

        var references = await backupStore.ListForTweakAsync(tweakId, cancellationToken).ConfigureAwait(false);
        if (references.Count == 0)
        {
            return TweakResult.Ok(tweakId, TweakStatus.NotApplied, message: "No backup found. Appx packages must be reinstalled manually from the Microsoft Store.");
        }

        var stillAbsent = 0;
        foreach (var name in packageNames)
        {
            if (manager.FindInstalled(name).Count == 0 && !manager.IsProvisioned(name))
            {
                stillAbsent++;
            }
        }

        var message = stillAbsent > 0
            ? $"Backup located but {stillAbsent} package(s) are still missing. Reinstall them from the Microsoft Store to restore."
            : "All packages are already back in place.";

        return TweakResult.Ok(tweakId, stillAbsent == 0 ? TweakStatus.NotApplied : TweakStatus.PartiallyApplied, new[] { references[0] }, message);
    }
}
