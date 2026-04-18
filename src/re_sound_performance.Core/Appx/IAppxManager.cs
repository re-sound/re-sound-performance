namespace re_sound_performance.Core.Appx;

public interface IAppxManager
{
    IReadOnlyList<AppxPackageInfo> FindInstalled(string packageName);

    bool IsProvisioned(string packageName);

    void RemoveForAllUsers(string packageName);

    void RemoveProvisioned(string packageName);
}
