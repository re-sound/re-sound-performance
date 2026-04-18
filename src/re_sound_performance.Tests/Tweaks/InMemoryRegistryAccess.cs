using Microsoft.Win32;
using re_sound_performance.Core.Registry;

namespace re_sound_performance.Tests.Tweaks;

internal sealed class InMemoryRegistryAccess : IRegistryAccess
{
    private readonly Dictionary<string, (object Value, RegistryValueKind Kind)> _store = new(StringComparer.OrdinalIgnoreCase);

    public object? GetValue(RegistryHive hive, string subKey, string valueName)
    {
        var key = BuildKey(hive, subKey, valueName);
        return _store.TryGetValue(key, out var entry) ? entry.Value : null;
    }

    public RegistryValueKind GetValueKind(RegistryHive hive, string subKey, string valueName)
    {
        var key = BuildKey(hive, subKey, valueName);
        return _store.TryGetValue(key, out var entry) ? entry.Kind : RegistryValueKind.Unknown;
    }

    public void SetValue(RegistryHive hive, string subKey, string valueName, object value, RegistryValueKind kind)
    {
        var key = BuildKey(hive, subKey, valueName);
        _store[key] = (value, kind);
    }

    public void DeleteValue(RegistryHive hive, string subKey, string valueName)
    {
        var key = BuildKey(hive, subKey, valueName);
        _store.Remove(key);
    }

    public bool SubKeyExists(RegistryHive hive, string subKey) =>
        _store.Keys.Any(k => k.StartsWith($"{hive}|{subKey}|", StringComparison.OrdinalIgnoreCase));

    private static string BuildKey(RegistryHive hive, string subKey, string valueName) =>
        $"{hive}|{subKey}|{valueName}";
}
