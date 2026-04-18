using FluentAssertions;
using Microsoft.Win32;
using re_sound_performance.Core.Registry;
using re_sound_performance.Core.Tweaks;

namespace re_sound_performance.Tests.Tweaks;

public sealed class RegistryTweakHelperTests
{
    [Fact]
    public async Task ApplyAsync_WritesAllChanges()
    {
        var registry = new InMemoryRegistryAccess();
        var backup = new InMemoryBackupStore();
        var changes = new[]
        {
            new RegistryChange(RegistryHive.CurrentUser, @"Test\Path", "ValueA", 42, RegistryValueKind.DWord),
            new RegistryChange(RegistryHive.CurrentUser, @"Test\Path", "ValueB", "hello", RegistryValueKind.String)
        };

        var result = await RegistryTweakHelper.ApplyAsync(registry, backup, "test.tweak", changes);

        result.Success.Should().BeTrue();
        result.ResultingStatus.Should().Be(TweakStatus.Applied);
        registry.GetValue(RegistryHive.CurrentUser, @"Test\Path", "ValueA").Should().Be(42);
        registry.GetValue(RegistryHive.CurrentUser, @"Test\Path", "ValueB").Should().Be("hello");
    }

    [Fact]
    public async Task ProbeAsync_AfterApply_ReportsApplied()
    {
        var registry = new InMemoryRegistryAccess();
        var backup = new InMemoryBackupStore();
        var changes = new[]
        {
            new RegistryChange(RegistryHive.CurrentUser, @"Test\Path", "ValueA", 42, RegistryValueKind.DWord),
            new RegistryChange(RegistryHive.CurrentUser, @"Test\Path", "ValueB", "hello", RegistryValueKind.String)
        };

        await RegistryTweakHelper.ApplyAsync(registry, backup, "test.tweak", changes);
        var status = await RegistryTweakHelper.ProbeAsync(registry, changes);

        status.Should().Be(TweakStatus.Applied);
    }

    [Fact]
    public async Task ProbeAsync_PartialApplied_ReportsPartial()
    {
        var registry = new InMemoryRegistryAccess();
        registry.SetValue(RegistryHive.CurrentUser, @"Test\Path", "ValueA", 42, RegistryValueKind.DWord);
        var expected = new[]
        {
            new RegistryChange(RegistryHive.CurrentUser, @"Test\Path", "ValueA", 42, RegistryValueKind.DWord),
            new RegistryChange(RegistryHive.CurrentUser, @"Test\Path", "ValueB", "hello", RegistryValueKind.String)
        };

        var status = await RegistryTweakHelper.ProbeAsync(registry, expected);

        status.Should().Be(TweakStatus.PartiallyApplied);
    }

    [Fact]
    public async Task RevertAsync_RestoresOriginalMissingValues()
    {
        var registry = new InMemoryRegistryAccess();
        var backup = new InMemoryBackupStore();
        var changes = new[]
        {
            new RegistryChange(RegistryHive.CurrentUser, @"Test\Path", "ValueA", 42, RegistryValueKind.DWord),
            new RegistryChange(RegistryHive.CurrentUser, @"Test\Path", "ValueB", "hello", RegistryValueKind.String)
        };

        await RegistryTweakHelper.ApplyAsync(registry, backup, "test.tweak", changes);
        var revert = await RegistryTweakHelper.RevertAsync(registry, backup, "test.tweak", changes);

        revert.Success.Should().BeTrue();
        revert.ResultingStatus.Should().Be(TweakStatus.NotApplied);
        registry.GetValue(RegistryHive.CurrentUser, @"Test\Path", "ValueA").Should().BeNull();
        registry.GetValue(RegistryHive.CurrentUser, @"Test\Path", "ValueB").Should().BeNull();
    }

    [Fact]
    public async Task RevertAsync_RestoresOriginalValuesWhenTheyExisted()
    {
        var registry = new InMemoryRegistryAccess();
        registry.SetValue(RegistryHive.CurrentUser, @"Test\Path", "ValueA", 7, RegistryValueKind.DWord);
        registry.SetValue(RegistryHive.CurrentUser, @"Test\Path", "ValueB", "previous", RegistryValueKind.String);
        var backup = new InMemoryBackupStore();
        var changes = new[]
        {
            new RegistryChange(RegistryHive.CurrentUser, @"Test\Path", "ValueA", 42, RegistryValueKind.DWord),
            new RegistryChange(RegistryHive.CurrentUser, @"Test\Path", "ValueB", "new", RegistryValueKind.String)
        };

        await RegistryTweakHelper.ApplyAsync(registry, backup, "test.tweak", changes);
        var revert = await RegistryTweakHelper.RevertAsync(registry, backup, "test.tweak", changes);

        revert.Success.Should().BeTrue();
        registry.GetValue(RegistryHive.CurrentUser, @"Test\Path", "ValueA").Should().Be(7);
        registry.GetValue(RegistryHive.CurrentUser, @"Test\Path", "ValueB").Should().Be("previous");
    }
}
