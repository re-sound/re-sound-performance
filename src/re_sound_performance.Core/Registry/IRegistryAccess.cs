using Microsoft.Win32;

namespace re_sound_performance.Core.Registry;

public interface IRegistryAccess
{
    object? GetValue(RegistryHive hive, string subKey, string valueName);

    RegistryValueKind GetValueKind(RegistryHive hive, string subKey, string valueName);

    void SetValue(RegistryHive hive, string subKey, string valueName, object value, RegistryValueKind kind);

    void DeleteValue(RegistryHive hive, string subKey, string valueName);

    bool SubKeyExists(RegistryHive hive, string subKey);
}
