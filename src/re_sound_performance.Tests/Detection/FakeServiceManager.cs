using re_sound_performance.Core.Services;

namespace re_sound_performance.Tests.Detection;

internal sealed class FakeServiceManager : IServiceManager
{
    private readonly Dictionary<string, ServiceStartupType> _services = new(StringComparer.OrdinalIgnoreCase);

    public FakeServiceManager WithService(string name, ServiceStartupType startup = ServiceStartupType.Manual)
    {
        _services[name] = startup;
        return this;
    }

    public bool ServiceExists(string serviceName) => _services.ContainsKey(serviceName);

    public ServiceStartupType? GetStartupType(string serviceName) =>
        _services.TryGetValue(serviceName, out var value) ? value : null;

    public void SetStartupType(string serviceName, ServiceStartupType startupType)
    {
        if (!_services.ContainsKey(serviceName))
        {
            throw new InvalidOperationException($"Unknown service {serviceName}");
        }

        _services[serviceName] = startupType;
    }
}
