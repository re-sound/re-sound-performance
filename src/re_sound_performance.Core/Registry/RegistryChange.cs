using Microsoft.Win32;

namespace re_sound_performance.Core.Registry;

public sealed record RegistryChange(
    RegistryHive Hive,
    string SubKey,
    string ValueName,
    object AppliedValue,
    RegistryValueKind Kind);
