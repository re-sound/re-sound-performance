using System.Text;

namespace re_sound_performance.Core.Backup;

public sealed class FileSystemBackupStore : IBackupStore
{
    private readonly string _rootDirectory;

    public FileSystemBackupStore(string rootDirectory)
    {
        _rootDirectory = rootDirectory ?? throw new ArgumentNullException(nameof(rootDirectory));
        Directory.CreateDirectory(_rootDirectory);
    }

    public async Task<string> SaveAsync(string tweakId, string payloadName, byte[] payload, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tweakId);
        ArgumentException.ThrowIfNullOrWhiteSpace(payloadName);
        ArgumentNullException.ThrowIfNull(payload);

        var tweakDirectory = Path.Combine(_rootDirectory, Sanitize(tweakId));
        Directory.CreateDirectory(tweakDirectory);

        var timestamp = DateTimeOffset.UtcNow.ToString("yyyyMMdd_HHmmssfff");
        var fileName = $"{timestamp}_{Sanitize(payloadName)}";
        var filePath = Path.Combine(tweakDirectory, fileName);

        await File.WriteAllBytesAsync(filePath, payload, cancellationToken).ConfigureAwait(false);
        return Path.GetRelativePath(_rootDirectory, filePath);
    }

    public async Task<byte[]?> LoadAsync(string backupReference, CancellationToken cancellationToken = default)
    {
        var filePath = Path.Combine(_rootDirectory, backupReference);
        if (!File.Exists(filePath))
        {
            return null;
        }

        return await File.ReadAllBytesAsync(filePath, cancellationToken).ConfigureAwait(false);
    }

    public Task<IReadOnlyList<string>> ListForTweakAsync(string tweakId, CancellationToken cancellationToken = default)
    {
        var tweakDirectory = Path.Combine(_rootDirectory, Sanitize(tweakId));
        if (!Directory.Exists(tweakDirectory))
        {
            return Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());
        }

        var files = Directory
            .EnumerateFiles(tweakDirectory)
            .Select(path => Path.GetRelativePath(_rootDirectory, path))
            .OrderByDescending(path => path, StringComparer.Ordinal)
            .ToArray();

        return Task.FromResult<IReadOnlyList<string>>(files);
    }

    public Task DeleteAsync(string backupReference, CancellationToken cancellationToken = default)
    {
        var filePath = Path.Combine(_rootDirectory, backupReference);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        return Task.CompletedTask;
    }

    private static string Sanitize(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var builder = new StringBuilder(value.Length);
        foreach (var character in value)
        {
            builder.Append(Array.IndexOf(invalid, character) >= 0 ? '_' : character);
        }

        return builder.ToString();
    }
}
