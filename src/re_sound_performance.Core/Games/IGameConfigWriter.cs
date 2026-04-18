namespace re_sound_performance.Core.Games;

public enum GameConfigWriteOutcome
{
    Written,
    Skipped,
    MissingInstall,
    Failed
}

public sealed record GameConfigWriteResult(GameConfigWriteOutcome Outcome, string? BackupPath, string? Message);

public interface IGameConfigWriter
{
    Task<GameConfigWriteResult> WriteRecommendedAsync(GameInstallation installation, RecommendedConfig recommendation, CancellationToken cancellationToken = default);
}
