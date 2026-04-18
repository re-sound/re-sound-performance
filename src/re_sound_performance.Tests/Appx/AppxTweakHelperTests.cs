using FluentAssertions;
using re_sound_performance.Core.Appx;
using re_sound_performance.Core.Tweaks;
using re_sound_performance.Tests.Tweaks;

namespace re_sound_performance.Tests.Appx;

public sealed class AppxTweakHelperTests
{
    [Fact]
    public async Task ApplyAsync_RemovesUserAndProvisionedPackages()
    {
        var manager = new InMemoryAppxManager();
        manager.SeedUserInstall("Microsoft.BingNews", "Microsoft.BingNews_1.0.0.0_x64__8wekyb3d8bbwe");
        manager.SeedProvisioned("Microsoft.BingNews");
        var backup = new InMemoryBackupStore();

        var result = await AppxTweakHelper.ApplyAsync(manager, backup, "debloat.test", new[] { "Microsoft.BingNews" });

        result.Success.Should().BeTrue();
        result.ResultingStatus.Should().Be(TweakStatus.Applied);
        manager.FindInstalled("Microsoft.BingNews").Should().BeEmpty();
        manager.IsProvisioned("Microsoft.BingNews").Should().BeFalse();
    }

    [Fact]
    public async Task ApplyAsync_WhenAlreadyRemoved_ReportsAppliedWithMessage()
    {
        var manager = new InMemoryAppxManager();
        var backup = new InMemoryBackupStore();

        var result = await AppxTweakHelper.ApplyAsync(manager, backup, "debloat.test", new[] { "Microsoft.BingNews" });

        result.Success.Should().BeTrue();
        result.ResultingStatus.Should().Be(TweakStatus.Applied);
        result.Message.Should().Contain("nothing to remove");
    }

    [Fact]
    public async Task ProbeAsync_AfterApply_ReportsApplied()
    {
        var manager = new InMemoryAppxManager();
        manager.SeedUserInstall("Microsoft.Clipchamp", "Microsoft.Clipchamp_1.0.0.0_x64__ewe");
        var backup = new InMemoryBackupStore();
        var names = new[] { "Microsoft.Clipchamp" };

        await AppxTweakHelper.ApplyAsync(manager, backup, "debloat.test", names);
        var status = await AppxTweakHelper.ProbeAsync(manager, names);

        status.Should().Be(TweakStatus.Applied);
    }

    [Fact]
    public async Task ProbeAsync_WhenSomeStillInstalled_ReportsPartial()
    {
        var manager = new InMemoryAppxManager();
        manager.SeedUserInstall("A", "A_1.0_x64__xyz");
        manager.SeedUserInstall("B", "B_1.0_x64__xyz");
        var backup = new InMemoryBackupStore();
        var names = new[] { "A", "B" };

        await AppxTweakHelper.ApplyAsync(manager, backup, "debloat.test", new[] { "A" });
        var status = await AppxTweakHelper.ProbeAsync(manager, names);

        status.Should().Be(TweakStatus.PartiallyApplied);
    }

    [Fact]
    public async Task RevertAsync_WithoutBackup_InstructsUserToReinstall()
    {
        var manager = new InMemoryAppxManager();
        var backup = new InMemoryBackupStore();

        var result = await AppxTweakHelper.RevertAsync(manager, backup, "debloat.test", new[] { "A" });

        result.Success.Should().BeTrue();
        result.ResultingStatus.Should().Be(TweakStatus.NotApplied);
        result.Message.Should().Contain("Microsoft Store");
    }
}
