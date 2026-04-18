using Microsoft.Win32;

namespace re_sound_performance.Core.Registry;

public sealed record RegistryValueSnapshot(
    RegistryHive Hive,
    string SubKey,
    string ValueName,
    bool Existed,
    object? OriginalValue,
    RegistryValueKind OriginalKind);
