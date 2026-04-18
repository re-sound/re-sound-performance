using FluentAssertions;
using Microsoft.Win32;
using re_sound_performance.Core.Tweaks;
using re_sound_performance.Core.Tweaks.Implementations;

namespace re_sound_performance.Tests.Tweaks;

public sealed class DisableXboxGameBarTweakTests
{
    [Fact]
    public async Task Apply_WhenNoPreviousValue_WritesZeroAndReturnsApplied()
    {
        var registry = new InMemoryRegistryAccess();
        var backup = new InMemoryBackupStore();
        var tweak = new DisableXboxGameBarTweak(registry);

        var result = await tweak.ApplyAsync(backup);

        result.Success.Should().BeTrue();
        result.ResultingStatus.Should().Be(TweakStatus.Applied);
        registry.GetValue(RegistryHive.CurrentUser, @"Software\Microsoft\GameBar", "UseNexusForGameBarEnabled")
            .Should().Be(0);
    }

    [Fact]
    public async Task Probe_AfterApply_ReportsApplied()
    {
        var registry = new InMemoryRegistryAccess();
        var backup = new InMemoryBackupStore();
        var tweak = new DisableXboxGameBarTweak(registry);

        await tweak.ApplyAsync(backup);
        var status = await tweak.ProbeAsync();

        status.Should().Be(TweakStatus.Applied);
    }

    [Fact]
    public async Task Revert_AfterApply_RestoresOriginalMissingValue()
    {
        var registry = new InMemoryRegistryAccess();
        var backup = new InMemoryBackupStore();
        var tweak = new DisableXboxGameBarTweak(registry);

        await tweak.ApplyAsync(backup);
        var revertResult = await tweak.RevertAsync(backup);

        revertResult.Success.Should().BeTrue();
        revertResult.ResultingStatus.Should().Be(TweakStatus.NotApplied);
        registry.GetValue(RegistryHive.CurrentUser, @"Software\Microsoft\GameBar", "UseNexusForGameBarEnabled")
            .Should().BeNull();
    }

    [Fact]
    public async Task Revert_AfterApply_WhenOriginalValueExisted_RestoresIt()
    {
        var registry = new InMemoryRegistryAccess();
        registry.SetValue(RegistryHive.CurrentUser, @"Software\Microsoft\GameBar", "UseNexusForGameBarEnabled", 1, RegistryValueKind.DWord);
        var backup = new InMemoryBackupStore();
        var tweak = new DisableXboxGameBarTweak(registry);

        await tweak.ApplyAsync(backup);
        var revertResult = await tweak.RevertAsync(backup);

        revertResult.Success.Should().BeTrue();
        registry.GetValue(RegistryHive.CurrentUser, @"Software\Microsoft\GameBar", "UseNexusForGameBarEnabled")
            .Should().Be(1);
    }
}
