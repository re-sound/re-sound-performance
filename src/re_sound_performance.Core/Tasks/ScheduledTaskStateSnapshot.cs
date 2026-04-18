namespace re_sound_performance.Core.Tasks;

public sealed record ScheduledTaskStateSnapshot(
    string TaskPath,
    bool Existed,
    ScheduledTaskState? OriginalState);
