using FluentAssertions;
using re_sound_performance.Core.Games;

namespace re_sound_performance.Tests.Games;

public sealed class FileSystemGameConfigWriterTests : IDisposable
{
    private readonly string _root;

    public FileSystemGameConfigWriterTests()
    {
        _root = Path.Combine(Path.GetTempPath(), "resound-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_root);
    }

    [Fact]
    public async Task Write_NoInstall_ReturnsMissingInstall()
    {
        var writer = new FileSystemGameConfigWriter();
        var install = GameInstallation.NotFound(GameId.Cs2);

        var result = await writer.WriteRecommendedAsync(install, GameRecommendations.Cs2);

        result.Outcome.Should().Be(GameConfigWriteOutcome.MissingInstall);
    }

    [Fact]
    public async Task Write_NoConfigFileRecipe_ReturnsSkipped()
    {
        var writer = new FileSystemGameConfigWriter();
        var install = new GameInstallation(GameId.Valorant, true, _root, GameLauncher.RiotClient, null);

        var result = await writer.WriteRecommendedAsync(install, GameRecommendations.Valorant);

        result.Outcome.Should().Be(GameConfigWriteOutcome.Skipped);
    }

    [Fact]
    public async Task Write_Cs2Autoexec_WritesFileUnderInstall()
    {
        var writer = new FileSystemGameConfigWriter();
        var install = new GameInstallation(GameId.Cs2, true, _root, GameLauncher.Steam, null);

        var result = await writer.WriteRecommendedAsync(install, GameRecommendations.Cs2);

        result.Outcome.Should().Be(GameConfigWriteOutcome.Written);
        var written = await File.ReadAllTextAsync(Path.Combine(_root, "game", "csgo", "cfg", "autoexec.cfg"));
        written.Should().Contain("cl_updaterate 128");
    }

    [Fact]
    public async Task Write_Cs2Autoexec_WhenExisting_KeepsBackup()
    {
        var dir = Path.Combine(_root, "game", "csgo", "cfg");
        Directory.CreateDirectory(dir);
        var target = Path.Combine(dir, "autoexec.cfg");
        await File.WriteAllTextAsync(target, "pre-existing content");

        var writer = new FileSystemGameConfigWriter();
        var install = new GameInstallation(GameId.Cs2, true, _root, GameLauncher.Steam, null);

        var result = await writer.WriteRecommendedAsync(install, GameRecommendations.Cs2);

        result.Outcome.Should().Be(GameConfigWriteOutcome.Written);
        result.BackupPath.Should().EndWith(".resound.bak");
        var backup = await File.ReadAllTextAsync(result.BackupPath!);
        backup.Should().Be("pre-existing content");
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_root))
            {
                Directory.Delete(_root, recursive: true);
            }
        }
        catch
        {
        }
    }
}
