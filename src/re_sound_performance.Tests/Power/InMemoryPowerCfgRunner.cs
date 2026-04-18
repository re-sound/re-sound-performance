using re_sound_performance.Core.Power;

namespace re_sound_performance.Tests.Power;

internal sealed class InMemoryPowerCfgRunner : IPowerCfgRunner
{
    private readonly Queue<string> _responses = new();

    public List<string> InvocationLog { get; } = new();

    public void QueueResponse(string output)
    {
        _responses.Enqueue(output);
    }

    public Task<string> ExecuteAsync(string arguments, CancellationToken cancellationToken = default)
    {
        InvocationLog.Add(arguments);
        var response = _responses.Count > 0 ? _responses.Dequeue() : string.Empty;
        return Task.FromResult(response);
    }
}
