using FluentAssertions;
using re_sound_performance.Core.Tasks;
using re_sound_performance.Core.Tweaks;
using re_sound_performance.Tests.Tweaks;

namespace re_sound_performance.Tests.Tasks;

public sealed class ScheduledTaskHelperTests
{
    [Fact]
    public async Task ApplyAsync_DisablesExistingTasks()
    {
        var manager = new InMemoryScheduledTaskManager();
        manager.Seed(@"\Microsoft\Windows\App\Task1", ScheduledTaskState.Ready);
        manager.Seed(@"\Microsoft\Windows\App\Task2", ScheduledTaskState.Ready);
        var backup = new InMemoryBackupStore();
        var changes = new[]
        {
            new ScheduledTaskChange(@"\Microsoft\Windows\App\Task1", ScheduledTaskState.Disabled),
            new ScheduledTaskChange(@"\Microsoft\Windows\App\Task2", ScheduledTaskState.Disabled)
        };

        var result = await ScheduledTaskHelper.ApplyAsync(manager, backup, "test.tweak", changes);

        result.Success.Should().BeTrue();
        result.ResultingStatus.Should().Be(TweakStatus.Applied);
        manager.GetState(@"\Microsoft\Windows\App\Task1").Should().Be(ScheduledTaskState.Disabled);
        manager.GetState(@"\Microsoft\Windows\App\Task2").Should().Be(ScheduledTaskState.Disabled);
    }

    [Fact]
    public async Task ApplyAsync_WhenTasksMissing_ReportsUnavailable()
    {
        var manager = new InMemoryScheduledTaskManager();
        var backup = new InMemoryBackupStore();
        var changes = new[]
        {
            new ScheduledTaskChange(@"\Missing\Task1", ScheduledTaskState.Disabled)
        };

        var result = await ScheduledTaskHelper.ApplyAsync(manager, backup, "test.tweak", changes);

        result.Success.Should().BeTrue();
        result.ResultingStatus.Should().Be(TweakStatus.Unavailable);
    }

    [Fact]
    public async Task ApplyAsync_PartialPresenceStillReportsAppliedWithMessage()
    {
        var manager = new InMemoryScheduledTaskManager();
        manager.Seed(@"\Present", ScheduledTaskState.Ready);
        var backup = new InMemoryBackupStore();
        var changes = new[]
        {
            new ScheduledTaskChange(@"\Present", ScheduledTaskState.Disabled),
            new ScheduledTaskChange(@"\Missing", ScheduledTaskState.Disabled)
        };

        var result = await ScheduledTaskHelper.ApplyAsync(manager, backup, "test.tweak", changes);

        result.Success.Should().BeTrue();
        result.ResultingStatus.Should().Be(TweakStatus.Applied);
        result.Message.Should().Contain("1 of 2");
    }

    [Fact]
    public async Task ProbeAsync_AfterApply_ReportsApplied()
    {
        var manager = new InMemoryScheduledTaskManager();
        manager.Seed(@"\Task", ScheduledTaskState.Ready);
        var backup = new InMemoryBackupStore();
        var changes = new[] { new ScheduledTaskChange(@"\Task", ScheduledTaskState.Disabled) };

        await ScheduledTaskHelper.ApplyAsync(manager, backup, "test.tweak", changes);
        var status = await ScheduledTaskHelper.ProbeAsync(manager, changes);

        status.Should().Be(TweakStatus.Applied);
    }

    [Fact]
    public async Task RevertAsync_RestoresOriginalState()
    {
        var manager = new InMemoryScheduledTaskManager();
        manager.Seed(@"\Task", ScheduledTaskState.Ready);
        var backup = new InMemoryBackupStore();
        var changes = new[] { new ScheduledTaskChange(@"\Task", ScheduledTaskState.Disabled) };

        await ScheduledTaskHelper.ApplyAsync(manager, backup, "test.tweak", changes);
        await ScheduledTaskHelper.RevertAsync(manager, backup, "test.tweak", changes);

        manager.GetState(@"\Task").Should().Be(ScheduledTaskState.Ready);
    }

    [Fact]
    public async Task RevertAsync_WithoutBackup_ReportsNotApplied()
    {
        var manager = new InMemoryScheduledTaskManager();
        var backup = new InMemoryBackupStore();
        var changes = new[] { new ScheduledTaskChange(@"\Task", ScheduledTaskState.Disabled) };

        var result = await ScheduledTaskHelper.RevertAsync(manager, backup, "test.tweak", changes);

        result.Success.Should().BeTrue();
        result.ResultingStatus.Should().Be(TweakStatus.NotApplied);
        result.Message.Should().Contain("No backup");
    }
}
