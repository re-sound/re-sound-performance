namespace re_sound_performance.Core.Games;

public sealed class FileSystemGameConfigWriter : IGameConfigWriter
{
    public async Task<GameConfigWriteResult> WriteRecommendedAsync(
        GameInstallation installation,
        RecommendedConfig recommendation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(installation);
        ArgumentNullException.ThrowIfNull(recommendation);

        if (!installation.Installed || string.IsNullOrWhiteSpace(installation.InstallPath))
        {
            return new GameConfigWriteResult(GameConfigWriteOutcome.MissingInstall, null, "Game not detected.");
        }

        if (string.IsNullOrWhiteSpace(recommendation.ConfigFileRelativePath) || recommendation.ConfigFileContent is null)
        {
            return new GameConfigWriteResult(GameConfigWriteOutcome.Skipped, null, "No on-disk config to write for this game.");
        }

        var target = Path.Combine(installation.InstallPath, recommendation.ConfigFileRelativePath);
        var directory = Path.GetDirectoryName(target);
        if (string.IsNullOrWhiteSpace(directory))
        {
            return new GameConfigWriteResult(GameConfigWriteOutcome.Failed, null, "Target path is invalid.");
        }

        try
        {
            Directory.CreateDirectory(directory);

            string? backupPath = null;
            if (File.Exists(target))
            {
                backupPath = target + ".resound.bak";
                File.Copy(target, backupPath, overwrite: true);
            }

            await File.WriteAllTextAsync(target, recommendation.ConfigFileContent, cancellationToken).ConfigureAwait(false);
            return new GameConfigWriteResult(GameConfigWriteOutcome.Written, backupPath, $"Wrote {target}");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return new GameConfigWriteResult(GameConfigWriteOutcome.Failed, null, ex.Message);
        }
    }
}
