namespace re_sound_performance.Core.Services;

public sealed record ServiceStateSnapshot(
    string ServiceName,
    bool Existed,
    ServiceStartupType? OriginalStartupType);
