using re_sound_performance.Core.Tasks;

namespace re_sound_performance.Tests.Tasks;

internal sealed class InMemoryScheduledTaskManager : IScheduledTaskManager
{
    private readonly Dictionary<string, ScheduledTaskState> _tasks = new(StringComparer.OrdinalIgnoreCase);

    public void Seed(string taskPath, ScheduledTaskState state) => _tasks[taskPath] = state;

    public bool TaskExists(string taskPath) => _tasks.ContainsKey(taskPath);

    public ScheduledTaskState? GetState(string taskPath) =>
        _tasks.TryGetValue(taskPath, out var state) ? state : null;

    public void SetState(string taskPath, ScheduledTaskState state)
    {
        if (!_tasks.ContainsKey(taskPath))
        {
            throw new InvalidOperationException($"Scheduled task {taskPath} does not exist.");
        }

        _tasks[taskPath] = state;
    }

    public SetStateOutcome TrySetState(string taskPath, ScheduledTaskState state)
    {
        if (_accessDenied.Contains(taskPath))
        {
            return new SetStateOutcome(SetStateResult.AccessDenied, "simulated access denied");
        }

        if (!_tasks.ContainsKey(taskPath))
        {
            return new SetStateOutcome(SetStateResult.Failed, $"Task {taskPath} does not exist.");
        }

        _tasks[taskPath] = state;
        return new SetStateOutcome(SetStateResult.Success, null);
    }

    public void SimulateAccessDenied(string taskPath) => _accessDenied.Add(taskPath);

    private readonly HashSet<string> _accessDenied = new(StringComparer.OrdinalIgnoreCase);
}
