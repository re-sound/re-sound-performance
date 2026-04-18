namespace re_sound_performance.Core.Tasks;

public interface IScheduledTaskManager
{
    bool TaskExists(string taskPath);

    ScheduledTaskState? GetState(string taskPath);

    void SetState(string taskPath, ScheduledTaskState state);

    SetStateOutcome TrySetState(string taskPath, ScheduledTaskState state);
}

public enum SetStateResult
{
    Success,
    AccessDenied,
    Failed
}

public readonly record struct SetStateOutcome(SetStateResult Result, string? ErrorMessage)
{
    public bool IsSuccess => Result == SetStateResult.Success;

    public bool IsAccessDenied => Result == SetStateResult.AccessDenied;
}
