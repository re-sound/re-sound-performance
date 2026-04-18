namespace re_sound_performance.Core.Backup;

public interface IBackupStore
{
    Task<string> SaveAsync(string tweakId, string payloadName, byte[] payload, CancellationToken cancellationToken = default);

    Task<byte[]?> LoadAsync(string backupReference, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> ListForTweakAsync(string tweakId, CancellationToken cancellationToken = default);

    Task DeleteAsync(string backupReference, CancellationToken cancellationToken = default);
}
