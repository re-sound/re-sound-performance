namespace re_sound_performance.Core.Games;

public interface IGameDetector
{
    Task<GameInstallation> DetectAsync(GameId game, CancellationToken cancellationToken = default);
}
