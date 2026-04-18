using FluentAssertions;
using re_sound_performance.Core.Games;
using re_sound_performance.Tests.Detection;
using re_sound_performance.Tests.Tweaks;

namespace re_sound_performance.Tests.Games;

public sealed class WindowsGameDetectorTests
{
    [Fact]
    public async Task Detect_Cs2NotInstalled_ReturnsNotFound()
    {
        var detector = new WindowsGameDetector(new InMemoryRegistryAccess(), new InMemoryFileSystemProbe());

        var result = await detector.DetectAsync(GameId.Cs2);

        result.Installed.Should().BeFalse();
        result.Launcher.Should().Be(GameLauncher.Unknown);
    }

    [Fact]
    public async Task Detect_Cs2SteamInstalled_ReportsSteam()
    {
        var fs = new InMemoryFileSystemProbe();
        fs.AddDirectory(@"C:\Program Files (x86)\Steam\steamapps\common\Counter-Strike Global Offensive");
        var detector = new WindowsGameDetector(new InMemoryRegistryAccess(), fs);

        var result = await detector.DetectAsync(GameId.Cs2);

        result.Installed.Should().BeTrue();
        result.Launcher.Should().Be(GameLauncher.Steam);
        result.InstallPath.Should().EndWith("Counter-Strike Global Offensive");
    }

    [Fact]
    public async Task Detect_ValorantFallbackPath_ReportsRiotClient()
    {
        var fs = new InMemoryFileSystemProbe();
        fs.AddDirectory(@"C:\Riot Games\VALORANT\live");
        var detector = new WindowsGameDetector(new InMemoryRegistryAccess(), fs);

        var result = await detector.DetectAsync(GameId.Valorant);

        result.Installed.Should().BeTrue();
        result.Launcher.Should().Be(GameLauncher.RiotClient);
    }

    [Fact]
    public async Task Detect_ApexSteam_ReportsSteam()
    {
        var fs = new InMemoryFileSystemProbe();
        fs.AddDirectory(@"C:\Program Files (x86)\Steam\steamapps\common\Apex Legends");
        var detector = new WindowsGameDetector(new InMemoryRegistryAccess(), fs);

        var result = await detector.DetectAsync(GameId.Apex);

        result.Installed.Should().BeTrue();
        result.Launcher.Should().Be(GameLauncher.Steam);
    }

    [Fact]
    public async Task Detect_ApexEaApp_ReportsEaAppWithWarning()
    {
        var fs = new InMemoryFileSystemProbe();
        fs.AddDirectory(@"C:\Program Files\EA Games\Apex");
        var detector = new WindowsGameDetector(new InMemoryRegistryAccess(), fs);

        var result = await detector.DetectAsync(GameId.Apex);

        result.Installed.Should().BeTrue();
        result.Launcher.Should().Be(GameLauncher.EaApp);
        result.Notes.Should().Contain("EA App");
    }
}
