using re_sound_performance.Core.Appx;

namespace re_sound_performance.Tests.Appx;

internal sealed class InMemoryAppxManager : IAppxManager
{
    private readonly Dictionary<string, List<AppxPackageInfo>> _installed = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _provisioned = new(StringComparer.OrdinalIgnoreCase);

    public void SeedUserInstall(string name, string packageFullName)
    {
        if (!_installed.TryGetValue(name, out var list))
        {
            list = new List<AppxPackageInfo>();
            _installed[name] = list;
        }

        list.Add(new AppxPackageInfo(name, packageFullName, AppxPackageKind.User));
    }

    public void SeedProvisioned(string name) => _provisioned.Add(name);

    public IReadOnlyList<AppxPackageInfo> FindInstalled(string packageName) =>
        _installed.TryGetValue(packageName, out var list) ? list.ToArray() : Array.Empty<AppxPackageInfo>();

    public bool IsProvisioned(string packageName) => _provisioned.Contains(packageName);

    public void RemoveForAllUsers(string packageName) => _installed.Remove(packageName);

    public void RemoveProvisioned(string packageName) => _provisioned.Remove(packageName);
}
