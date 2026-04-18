using FluentAssertions;
using re_sound_performance.Core.Detection;

namespace re_sound_performance.Tests.Detection;

public sealed class WindowsAntiCheatDetectorTests
{
    [Fact]
    public async Task Detect_NoAntiCheat_ReportsNone()
    {
        var detector = new WindowsAntiCheatDetector(new FakeServiceManager(), new InMemoryFileSystemProbe());

        var result = await detector.DetectAsync();

        result.HasAny.Should().BeFalse();
        result.Installed.Should().Be(AntiCheat.None);
        result.Details.Should().BeEmpty();
    }

    [Fact]
    public async Task Detect_VanguardServicePresent_FlagsVanguard()
    {
        var services = new FakeServiceManager().WithService("vgc");
        var fs = new InMemoryFileSystemProbe();
        var detector = new WindowsAntiCheatDetector(services, fs);

        var result = await detector.DetectAsync();

        result.Has(AntiCheat.Vanguard).Should().BeTrue();
        result.Details.Should().Contain(d => d.Contains("Vanguard", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Detect_VanguardFolderPresent_FlagsVanguard()
    {
        var services = new FakeServiceManager();
        var fs = new InMemoryFileSystemProbe();
        fs.AddDirectory(@"C:\Program Files\Riot Vanguard");
        var detector = new WindowsAntiCheatDetector(services, fs);

        var result = await detector.DetectAsync();

        result.Has(AntiCheat.Vanguard).Should().BeTrue();
    }

    [Fact]
    public async Task Detect_FaceitInstalled_FlagsFaceit()
    {
        var fs = new InMemoryFileSystemProbe();
        fs.AddDirectory(@"C:\Program Files\FACEIT AC");
        var detector = new WindowsAntiCheatDetector(new FakeServiceManager(), fs);

        var result = await detector.DetectAsync();

        result.Has(AntiCheat.FaceitAc).Should().BeTrue();
    }

    [Fact]
    public async Task Detect_EasyAntiCheatInSteamLibrary_FlagsEac()
    {
        var fs = new InMemoryFileSystemProbe();
        fs.AddDirectory(@"C:\Program Files (x86)\Steam\steamapps\common");
        fs.AddDirectory(@"C:\Program Files (x86)\Steam\steamapps\common\Apex");
        fs.AddDirectory(@"C:\Program Files (x86)\Steam\steamapps\common\Apex\EasyAntiCheat");
        var detector = new WindowsAntiCheatDetector(new FakeServiceManager(), fs);

        var result = await detector.DetectAsync();

        result.Has(AntiCheat.EasyAntiCheat).Should().BeTrue();
    }

    [Fact]
    public async Task Detect_BattlEyeService_FlagsBattlEye()
    {
        var services = new FakeServiceManager().WithService("BEService");
        var detector = new WindowsAntiCheatDetector(services, new InMemoryFileSystemProbe());

        var result = await detector.DetectAsync();

        result.Has(AntiCheat.BattlEye).Should().BeTrue();
    }

    [Fact]
    public async Task Detect_MultipleProducts_SetsAllFlags()
    {
        var services = new FakeServiceManager().WithService("vgc").WithService("BEService");
        var fs = new InMemoryFileSystemProbe();
        fs.AddDirectory(@"C:\Program Files\FACEIT AC");
        var detector = new WindowsAntiCheatDetector(services, fs);

        var result = await detector.DetectAsync();

        result.Has(AntiCheat.Vanguard).Should().BeTrue();
        result.Has(AntiCheat.FaceitAc).Should().BeTrue();
        result.Has(AntiCheat.BattlEye).Should().BeTrue();
        result.Details.Should().HaveCount(3);
    }
}
