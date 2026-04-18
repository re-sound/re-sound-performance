namespace re_sound_performance.Core.Services;

public interface IServiceManager
{
    bool ServiceExists(string serviceName);

    ServiceStartupType? GetStartupType(string serviceName);

    void SetStartupType(string serviceName, ServiceStartupType startupType);
}
