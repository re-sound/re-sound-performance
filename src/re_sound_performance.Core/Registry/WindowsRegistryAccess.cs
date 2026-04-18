using Microsoft.Win32;

namespace re_sound_performance.Core.Registry;

public sealed class WindowsRegistryAccess : IRegistryAccess
{
    public object? GetValue(RegistryHive hive, string subKey, string valueName)
    {
        using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64);
        using var key = baseKey.OpenSubKey(subKey);
        return key?.GetValue(valueName);
    }

    public RegistryValueKind GetValueKind(RegistryHive hive, string subKey, string valueName)
    {
        using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64);
        using var key = baseKey.OpenSubKey(subKey);
        if (key is null)
        {
            return RegistryValueKind.Unknown;
        }

        return key.GetValueKind(valueName);
    }

    public void SetValue(RegistryHive hive, string subKey, string valueName, object value, RegistryValueKind kind)
    {
        using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64);
        using var key = baseKey.CreateSubKey(subKey, writable: true);
        if (key is null)
        {
            throw new InvalidOperationException($"Could not open or create subkey {subKey}.");
        }

        key.SetValue(valueName, value, kind);
    }

    public void DeleteValue(RegistryHive hive, string subKey, string valueName)
    {
        using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64);
        using var key = baseKey.OpenSubKey(subKey, writable: true);
        key?.DeleteValue(valueName, throwOnMissingValue: false);
    }

    public bool SubKeyExists(RegistryHive hive, string subKey)
    {
        using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64);
        using var key = baseKey.OpenSubKey(subKey);
        return key is not null;
    }
}
