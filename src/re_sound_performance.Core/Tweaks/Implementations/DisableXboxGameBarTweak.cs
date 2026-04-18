using System.Text.Json;
using Microsoft.Win32;
using re_sound_performance.Core.Backup;
using re_sound_performance.Core.Registry;

namespace re_sound_performance.Core.Tweaks.Implementations;

public sealed class DisableXboxGameBarTweak : ITweak
{
    private const string SubKey = @"Software\Microsoft\GameBar";
    private const string ValueName = "UseNexusForGameBarEnabled";

    private readonly IRegistryAccess _registry;

    public DisableXboxGameBarTweak(IRegistryAccess registry)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    }

    public TweakMetadata Metadata { get; } = new(
        Id: "system.disable_xbox_game_bar",
        Name: "Disable Xbox Game Bar",
        ShortDescription: "Turns off the Xbox Game Bar overlay and hotkey handler.",
        DetailedDescription: "Xbox Game Bar adds background overhead, registers global hotkeys that can interfere with games, and keeps DVR recording services resident. Disabling it frees a small amount of RAM and removes input latency caused by the overlay hook.",
        Modifies: "HKCU\\Software\\Microsoft\\GameBar\\UseNexusForGameBarEnabled (DWORD)",
        ExpectedImpact: "Reduced background overhead. No measurable FPS change but eliminates overlay-related hitching on some setups.",
        Category: TweakCategory.System,
        Risk: TweakRisk.Safe,
        Evidence: TweakEvidenceLevel.Confirmed,
        Sources: new[]
        {
            "Microsoft Support: options-to-optimize-gaming-performance-in-windows-11",
            "Blur Busters forum thread on Game Bar input lag"
        },
        IncompatibleWith: Array.Empty<string>(),
        RequiresRestart: false,
        BlockedWhenVanguardInstalled: false,
        BlockedWhenFaceitInstalled: false);

    public Task<TweakStatus> ProbeAsync(CancellationToken cancellationToken = default)
    {
        var currentValue = _registry.GetValue(RegistryHive.CurrentUser, SubKey, ValueName);
        if (currentValue is null)
        {
            return Task.FromResult(TweakStatus.NotApplied);
        }

        if (currentValue is int intValue)
        {
            return Task.FromResult(intValue == 0 ? TweakStatus.Applied : TweakStatus.NotApplied);
        }

        return Task.FromResult(TweakStatus.Unknown);
    }

    public async Task<TweakResult> ApplyAsync(IBackupStore backupStore, CancellationToken cancellationToken = default)
    {
        var snapshot = CaptureSnapshot();
        var backupReference = await SaveSnapshotAsync(backupStore, snapshot, cancellationToken).ConfigureAwait(false);

        _registry.SetValue(RegistryHive.CurrentUser, SubKey, ValueName, 0, RegistryValueKind.DWord);

        return TweakResult.Ok(Metadata.Id, TweakStatus.Applied, new[] { backupReference });
    }

    public async Task<TweakResult> RevertAsync(IBackupStore backupStore, CancellationToken cancellationToken = default)
    {
        var references = await backupStore.ListForTweakAsync(Metadata.Id, cancellationToken).ConfigureAwait(false);
        if (references.Count == 0)
        {
            _registry.DeleteValue(RegistryHive.CurrentUser, SubKey, ValueName);
            return TweakResult.Ok(Metadata.Id, TweakStatus.NotApplied, message: "No backup found, default state restored.");
        }

        var latestReference = references[0];
        var payload = await backupStore.LoadAsync(latestReference, cancellationToken).ConfigureAwait(false);
        if (payload is null || payload.Length == 0)
        {
            _registry.DeleteValue(RegistryHive.CurrentUser, SubKey, ValueName);
            return TweakResult.Ok(Metadata.Id, TweakStatus.NotApplied, message: "Backup payload missing, default state restored.");
        }

        var snapshot = JsonSerializer.Deserialize<RegistryValueSnapshot>(payload)
            ?? throw new InvalidOperationException("Backup payload could not be deserialized.");

        RestoreSnapshot(snapshot);
        return TweakResult.Ok(Metadata.Id, TweakStatus.NotApplied, new[] { latestReference });
    }

    private RegistryValueSnapshot CaptureSnapshot()
    {
        var existing = _registry.GetValue(RegistryHive.CurrentUser, SubKey, ValueName);
        if (existing is null)
        {
            return new RegistryValueSnapshot(RegistryHive.CurrentUser, SubKey, ValueName, Existed: false, OriginalValue: null, OriginalKind: RegistryValueKind.Unknown);
        }

        var kind = _registry.GetValueKind(RegistryHive.CurrentUser, SubKey, ValueName);
        return new RegistryValueSnapshot(RegistryHive.CurrentUser, SubKey, ValueName, Existed: true, OriginalValue: existing, OriginalKind: kind);
    }

    private static Task<string> SaveSnapshotAsync(IBackupStore backupStore, RegistryValueSnapshot snapshot, CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.SerializeToUtf8Bytes(snapshot, new JsonSerializerOptions { WriteIndented = false });
        return backupStore.SaveAsync("system.disable_xbox_game_bar", "registry.json", payload, cancellationToken);
    }

    private void RestoreSnapshot(RegistryValueSnapshot snapshot)
    {
        if (!snapshot.Existed || snapshot.OriginalValue is null)
        {
            _registry.DeleteValue(snapshot.Hive, snapshot.SubKey, snapshot.ValueName);
            return;
        }

        var value = NormalizeValue(snapshot.OriginalValue, snapshot.OriginalKind);
        _registry.SetValue(snapshot.Hive, snapshot.SubKey, snapshot.ValueName, value, snapshot.OriginalKind);
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
            _ => element.ToString()
        };
    }
}
