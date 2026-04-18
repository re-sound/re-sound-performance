namespace re_sound_performance.Core.Tasks;

public interface IScheduledTaskManager
{
    bool TaskExists(string taskPath);

    ScheduledTaskState? GetState(string taskPath);

    void SetState(string taskPath, ScheduledTaskState state);
}
