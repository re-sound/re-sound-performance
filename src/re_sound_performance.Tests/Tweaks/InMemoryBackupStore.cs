using re_sound_performance.Core.Backup;

namespace re_sound_performance.Tests.Tweaks;

internal sealed class InMemoryBackupStore : IBackupStore
{
    private readonly Dictionary<string, List<(string Reference, byte[] Payload)>> _store = new(StringComparer.OrdinalIgnoreCase);
    private int _counter;

    public Task<string> SaveAsync(string tweakId, string payloadName, byte[] payload, CancellationToken cancellationToken = default)
    {
        if (!_store.TryGetValue(tweakId, out var list))
        {
            list = new List<(string, byte[])>();
            _store[tweakId] = list;
        }

        var reference = $"{tweakId}/{++_counter:D4}_{payloadName}";
        list.Insert(0, (reference, payload));
        return Task.FromResult(reference);
    }

    public Task<byte[]?> LoadAsync(string backupReference, CancellationToken cancellationToken = default)
    {
        foreach (var entry in _store.Values.SelectMany(list => list))
        {
            if (string.Equals(entry.Reference, backupReference, StringComparison.Ordinal))
            {
                return Task.FromResult<byte[]?>(entry.Payload);
            }
        }

        return Task.FromResult<byte[]?>(null);
    }

    public Task<IReadOnlyList<string>> ListForTweakAsync(string tweakId, CancellationToken cancellationToken = default)
    {
        if (!_store.TryGetValue(tweakId, out var list))
        {
            return Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());
        }

        return Task.FromResult<IReadOnlyList<string>>(list.Select(e => e.Reference).ToList());
    }

    public Task DeleteAsync(string backupReference, CancellationToken cancellationToken = default)
    {
        foreach (var list in _store.Values)
        {
            list.RemoveAll(e => string.Equals(e.Reference, backupReference, StringComparison.Ordinal));
        }

        return Task.CompletedTask;
    }
}
