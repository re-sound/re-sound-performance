using System.Text.Json;
using Microsoft.Win32;
using re_sound_performance.Core.Backup;
using re_sound_performance.Core.Tweaks;

namespace re_sound_performance.Core.Registry;

public static class RegistryTweakHelper
{
    private const string BackupPayloadName = "registry.json";

    public static Task<TweakStatus> ProbeAsync(IRegistryAccess registry, IReadOnlyList<RegistryChange> expectedChanges)
    {
        var anyApplied = false;
        var allApplied = true;

        foreach (var change in expectedChanges)
        {
            var current = registry.GetValue(change.Hive, change.SubKey, change.ValueName);
            var matches = current is not null && ValuesEqual(current, change.AppliedValue, change.Kind);

            if (matches)
            {
                anyApplied = true;
            }
            else
            {
                allApplied = false;
            }
        }

        if (allApplied)
        {
            return Task.FromResult(TweakStatus.Applied);
        }

        return Task.FromResult(anyApplied ? TweakStatus.PartiallyApplied : TweakStatus.NotApplied);
    }

    public static async Task<TweakResult> ApplyAsync(
        IRegistryAccess registry,
        IBackupStore backupStore,
        string tweakId,
        IReadOnlyList<RegistryChange> changes,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentNullException.ThrowIfNull(backupStore);
        ArgumentException.ThrowIfNullOrWhiteSpace(tweakId);
        ArgumentNullException.ThrowIfNull(changes);

        var snapshots = new List<RegistryValueSnapshot>(changes.Count);
        foreach (var change in changes)
        {
            var existing = registry.GetValue(change.Hive, change.SubKey, change.ValueName);
            if (existing is null)
            {
                snapshots.Add(new RegistryValueSnapshot(
                    change.Hive,
                    change.SubKey,
                    change.ValueName,
                    Existed: false,
                    OriginalValue: null,
                    OriginalKind: RegistryValueKind.Unknown));
            }
            else
            {
                var kind = registry.GetValueKind(change.Hive, change.SubKey, change.ValueName);
                snapshots.Add(new RegistryValueSnapshot(
                    change.Hive,
                    change.SubKey,
                    change.ValueName,
                    Existed: true,
                    OriginalValue: existing,
                    OriginalKind: kind));
            }
        }

        var payload = JsonSerializer.SerializeToUtf8Bytes(snapshots);
        var backupReference = await backupStore.SaveAsync(tweakId, BackupPayloadName, payload, cancellationToken).ConfigureAwait(false);

        foreach (var change in changes)
        {
            registry.SetValue(change.Hive, change.SubKey, change.ValueName, change.AppliedValue, change.Kind);
        }

        return TweakResult.Ok(tweakId, TweakStatus.Applied, new[] { backupReference });
    }

    public static async Task<TweakResult> RevertAsync(
        IRegistryAccess registry,
        IBackupStore backupStore,
        string tweakId,
        IReadOnlyList<RegistryChange> defaultChanges,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentNullException.ThrowIfNull(backupStore);
        ArgumentException.ThrowIfNullOrWhiteSpace(tweakId);

        var references = await backupStore.ListForTweakAsync(tweakId, cancellationToken).ConfigureAwait(false);
        if (references.Count == 0)
        {
            foreach (var change in defaultChanges)
            {
                registry.DeleteValue(change.Hive, change.SubKey, change.ValueName);
            }

            return TweakResult.Ok(tweakId, TweakStatus.NotApplied, message: "No backup found, defaults restored.");
        }

        var latestReference = references[0];
        var payload = await backupStore.LoadAsync(latestReference, cancellationToken).ConfigureAwait(false);
        if (payload is null || payload.Length == 0)
        {
            foreach (var change in defaultChanges)
            {
                registry.DeleteValue(change.Hive, change.SubKey, change.ValueName);
            }

            return TweakResult.Ok(tweakId, TweakStatus.NotApplied, message: "Backup payload missing, defaults restored.");
        }

        var snapshots = JsonSerializer.Deserialize<List<RegistryValueSnapshot>>(payload)
            ?? throw new InvalidOperationException($"Could not deserialize backup for tweak {tweakId}.");

        foreach (var snapshot in snapshots)
        {
            if (!snapshot.Existed || snapshot.OriginalValue is null)
            {
                registry.DeleteValue(snapshot.Hive, snapshot.SubKey, snapshot.ValueName);
            }
            else
            {
                var normalized = NormalizeValue(snapshot.OriginalValue, snapshot.OriginalKind);
                registry.SetValue(snapshot.Hive, snapshot.SubKey, snapshot.ValueName, normalized, snapshot.OriginalKind);
            }
        }

        return TweakResult.Ok(tweakId, TweakStatus.NotApplied, new[] { latestReference });
    }

    private static bool ValuesEqual(object current, object expected, RegistryValueKind kind)
    {
        return kind switch
        {
            RegistryValueKind.DWord => ConvertToInt32(current) == ConvertToInt32(expected),
            RegistryValueKind.QWord => ConvertToInt64(current) == ConvertToInt64(expected),
            RegistryValueKind.String => string.Equals(current.ToString(), expected.ToString(), StringComparison.Ordinal),
            RegistryValueKind.ExpandString => string.Equals(current.ToString(), expected.ToString(), StringComparison.Ordinal),
            RegistryValueKind.Binary => BinaryEquals(current, expected),
            RegistryValueKind.MultiString => MultiStringEquals(current, expected),
            _ => Equals(current, expected)
        };
    }

    private static int ConvertToInt32(object value) => value switch
    {
        int i => i,
        long l => (int)l,
        JsonElement e when e.ValueKind == JsonValueKind.Number => e.GetInt32(),
        _ => Convert.ToInt32(value)
    };

    private static long ConvertToInt64(object value) => value switch
    {
        long l => l,
        int i => i,
        JsonElement e when e.ValueKind == JsonValueKind.Number => e.GetInt64(),
        _ => Convert.ToInt64(value)
    };

    private static bool BinaryEquals(object current, object expected)
    {
        var currentBytes = current as byte[];
        var expectedBytes = expected as byte[];
        if (currentBytes is null || expectedBytes is null)
        {
            return false;
        }

        return currentBytes.SequenceEqual(expectedBytes);
    }

    private static bool MultiStringEquals(object current, object expected)
    {
        var currentStrings = current as string[];
        var expectedStrings = expected as string[];
        if (currentStrings is null || expectedStrings is null)
        {
            return false;
        }

        return currentStrings.SequenceEqual(expectedStrings, StringComparer.Ordinal);
    }

    private static object NormalizeValue(object value, RegistryValueKind kind)
    {
        if (value is not JsonElement element)
        {
            return value;
        }

        return kind switch
        {
            RegistryValueKind.DWord => element.GetInt32(),
            RegistryValueKind.QWord => element.GetInt64(),
            RegistryValueKind.String => element.GetString() ?? string.Empty,
            RegistryValueKind.ExpandString => element.GetString() ?? string.Empty,
            RegistryValueKind.MultiString => element.EnumerateArray()
                .Select(e => e.GetString() ?? string.Empty)
                .ToArray(),
            RegistryValueKind.Binary => element.EnumerateArray()
                .Select(e => e.GetByte())
                .ToArray(),
            _ => element.ToString() ?? string.Empty
        };
    }
}
