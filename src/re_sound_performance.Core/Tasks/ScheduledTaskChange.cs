namespace re_sound_performance.Core.Tasks;

public sealed record ScheduledTaskChange(string TaskPath, ScheduledTaskState TargetState);
