namespace re_sound_performance.Core.Games;

public sealed record GameInstallation(
    GameId Game,
    bool Installed,
    string? InstallPath,
    GameLauncher Launcher,
    string? Notes)
{
    public static GameInstallation NotFound(GameId game) =>
        new(game, Installed: false, InstallPath: null, Launcher: GameLauncher.Unknown, Notes: null);
}
