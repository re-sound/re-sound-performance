namespace re_sound_performance.Core.Appx;

public sealed record AppxStateSnapshot(
    string Name,
    bool WasInstalled,
    bool WasProvisioned,
    IReadOnlyList<string> InstalledFullNames);
