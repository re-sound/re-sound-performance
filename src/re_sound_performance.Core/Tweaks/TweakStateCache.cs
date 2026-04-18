using System.Collections.Concurrent;

namespace re_sound_performance.Core.Tweaks;

public sealed class TweakStateCache
{
    private readonly ConcurrentDictionary<string, TweakStatus> _states = new(StringComparer.OrdinalIgnoreCase);

    public event EventHandler<TweakStateChangedEventArgs>? StateChanged;

    public event EventHandler<TweakProbingProgressEventArgs>? ProbingProgress;

    public bool TryGet(string tweakId, out TweakStatus status) =>
        _states.TryGetValue(tweakId, out status);

    public TweakStatus GetOrUnknown(string tweakId) =>
        _states.TryGetValue(tweakId, out var status) ? status : TweakStatus.Unknown;

    public IReadOnlyDictionary<string, TweakStatus> Snapshot() =>
        _states.ToDictionary(k => k.Key, v => v.Value, StringComparer.OrdinalIgnoreCase);

    public void Set(string tweakId, TweakStatus status)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tweakId);

        var changed = true;
        _states.AddOrUpdate(tweakId, status, (_, old) =>
        {
            changed = old != status;
            return status;
        });

        if (changed)
        {
            StateChanged?.Invoke(this, new TweakStateChangedEventArgs(tweakId, status));
        }
    }

    internal void RaiseProbingProgress(int completed, int total) =>
        ProbingProgress?.Invoke(this, new TweakProbingProgressEventArgs(completed, total));
}

public sealed class TweakStateChangedEventArgs : EventArgs
{
    public TweakStateChangedEventArgs(string tweakId, TweakStatus status)
    {
        TweakId = tweakId;
        Status = status;
    }

    public string TweakId { get; }

    public TweakStatus Status { get; }
}

public sealed class TweakProbingProgressEventArgs : EventArgs
{
    public TweakProbingProgressEventArgs(int completed, int total)
    {
        Completed = completed;
        Total = total;
    }

    public int Completed { get; }

    public int Total { get; }

    public bool IsComplete => Total > 0 && Completed >= Total;
}
