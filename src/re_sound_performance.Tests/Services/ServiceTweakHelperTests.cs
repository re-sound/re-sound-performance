using FluentAssertions;
using Microsoft.Win32;
using re_sound_performance.Core.Services;
using re_sound_performance.Core.Tweaks;
using re_sound_performance.Tests.Tweaks;

namespace re_sound_performance.Tests.Services;

public sealed class ServiceTweakHelperTests
{
    [Fact]
    public async Task ApplyAsync_DisablesServiceByWritingRegistryValue()
    {
        var registry = new InMemoryRegistryAccess();
        registry.SetValue(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Services\DiagTrack", "Start", (int)ServiceStartupType.Automatic, RegistryValueKind.DWord);
        var manager = new WindowsServiceManager(registry);
        var backup = new InMemoryBackupStore();
        var changes = new[] { new ServiceChange("DiagTrack", ServiceStartupType.Disabled) };

        var result = await ServiceTweakHelper.ApplyAsync(manager, backup, "test.tweak", changes);

        result.Success.Should().BeTrue();
        result.ResultingStatus.Should().Be(TweakStatus.Applied);
        manager.GetStartupType("DiagTrack").Should().Be(ServiceStartupType.Disabled);
    }

    [Fact]
    public async Task ApplyAsync_WhenServiceMissing_ReportsUnavailable()
    {
        var registry = new InMemoryRegistryAccess();
        var manager = new WindowsServiceManager(registry);
        var backup = new InMemoryBackupStore();
        var changes = new[] { new ServiceChange("NonExistentService", ServiceStartupType.Disabled) };

        var result = await ServiceTweakHelper.ApplyAsync(manager, backup, "test.tweak", changes);

        result.Success.Should().BeTrue();
        result.ResultingStatus.Should().Be(TweakStatus.Unavailable);
    }

    [Fact]
    public async Task ProbeAsync_ReportsAppliedAfterApply()
    {
        var registry = new InMemoryRegistryAccess();
        registry.SetValue(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Services\DiagTrack", "Start", (int)ServiceStartupType.Automatic, RegistryValueKind.DWord);
        var manager = new WindowsServiceManager(registry);
        var backup = new InMemoryBackupStore();
        var changes = new[] { new ServiceChange("DiagTrack", ServiceStartupType.Disabled) };

        await ServiceTweakHelper.ApplyAsync(manager, backup, "test.tweak", changes);
        var status = await ServiceTweakHelper.ProbeAsync(manager, changes);

        status.Should().Be(TweakStatus.Applied);
    }

    [Fact]
    public async Task RevertAsync_RestoresOriginalStartupType()
    {
        var registry = new InMemoryRegistryAccess();
        registry.SetValue(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Services\DiagTrack", "Start", (int)ServiceStartupType.Automatic, RegistryValueKind.DWord);
        var manager = new WindowsServiceManager(registry);
        var backup = new InMemoryBackupStore();
        var changes = new[] { new ServiceChange("DiagTrack", ServiceStartupType.Disabled) };

        await ServiceTweakHelper.ApplyAsync(manager, backup, "test.tweak", changes);
        var revert = await ServiceTweakHelper.RevertAsync(manager, backup, "test.tweak", changes);

        revert.Success.Should().BeTrue();
        manager.GetStartupType("DiagTrack").Should().Be(ServiceStartupType.Automatic);
    }

    [Fact]
    public async Task ProbeAsync_AllServicesMissing_ReportsUnavailable()
    {
        var registry = new InMemoryRegistryAccess();
        var manager = new WindowsServiceManager(registry);
        var changes = new[]
        {
            new ServiceChange("Missing1", ServiceStartupType.Disabled),
            new ServiceChange("Missing2", ServiceStartupType.Disabled)
        };

        var status = await ServiceTweakHelper.ProbeAsync(manager, changes);

        status.Should().Be(TweakStatus.Unavailable);
    }
}
