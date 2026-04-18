using Microsoft.Win32;
using re_sound_performance.Core.Registry;

namespace re_sound_performance.Core.Services;

public sealed class WindowsServiceManager : IServiceManager
{
    private const string ServicesRoot = @"SYSTEM\CurrentControlSet\Services";
    private const string StartValueName = "Start";

    private readonly IRegistryAccess _registry;

    public WindowsServiceManager(IRegistryAccess registry)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    }

    public bool ServiceExists(string serviceName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceName);
        return _registry.SubKeyExists(RegistryHive.LocalMachine, BuildServicePath(serviceName));
    }

    public ServiceStartupType? GetStartupType(string serviceName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceName);
        var path = BuildServicePath(serviceName);
        if (!_registry.SubKeyExists(RegistryHive.LocalMachine, path))
        {
            return null;
        }

        var raw = _registry.GetValue(RegistryHive.LocalMachine, path, StartValueName);
        if (raw is null)
        {
            return null;
        }

        var value = Convert.ToInt32(raw);
        if (!Enum.IsDefined(typeof(ServiceStartupType), value))
        {
            return null;
        }

        return (ServiceStartupType)value;
    }

    public void SetStartupType(string serviceName, ServiceStartupType startupType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceName);
        var path = BuildServicePath(serviceName);
        if (!_registry.SubKeyExists(RegistryHive.LocalMachine, path))
        {
            throw new InvalidOperationException($"Service {serviceName} does not exist on this system.");
        }

        _registry.SetValue(RegistryHive.LocalMachine, path, StartValueName, (int)startupType, RegistryValueKind.DWord);
    }

    private static string BuildServicePath(string serviceName) => $"{ServicesRoot}\\{serviceName}";
}
