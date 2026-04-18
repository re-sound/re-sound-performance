using FluentAssertions;
using re_sound_performance.Core.Power;
using re_sound_performance.Core.Tweaks;
using re_sound_performance.Core.Tweaks.Implementations;
using re_sound_performance.Tests.Tweaks;

namespace re_sound_performance.Tests.Power;

public sealed class EnableUltimatePerformancePlanTweakTests
{
    [Fact]
    public async Task ApplyAsync_DuplicatesAndActivatesUltimateScheme()
    {
        var runner = new InMemoryPowerCfgRunner();
        runner.QueueResponse($"Power Scheme GUID: {PowerPlanGuids.Balanced}");
        runner.QueueResponse("Power Scheme GUID duplicated.");
        runner.QueueResponse(string.Empty);

        var tweak = new EnableUltimatePerformancePlanTweak(runner);
        var backup = new InMemoryBackupStore();

        var result = await tweak.ApplyAsync(backup);

        result.Success.Should().BeTrue();
        result.ResultingStatus.Should().Be(TweakStatus.Applied);
        runner.InvocationLog.Should().Contain(args => args.Contains("/duplicatescheme", StringComparison.OrdinalIgnoreCase));
        runner.InvocationLog.Should().Contain(args => args.Contains($"/setactive {PowerPlanGuids.UltimatePerformance}", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task ProbeAsync_ReportsAppliedWhenActiveSchemeIsUltimate()
    {
        var runner = new InMemoryPowerCfgRunner();
        runner.QueueResponse($"Power Scheme GUID: {PowerPlanGuids.UltimatePerformance}");
        var tweak = new EnableUltimatePerformancePlanTweak(runner);

        var status = await tweak.ProbeAsync();

        status.Should().Be(TweakStatus.Applied);
    }

    [Fact]
    public async Task ProbeAsync_ReportsNotAppliedWhenActiveSchemeIsBalanced()
    {
        var runner = new InMemoryPowerCfgRunner();
        runner.QueueResponse($"Power Scheme GUID: {PowerPlanGuids.Balanced}");
        var tweak = new EnableUltimatePerformancePlanTweak(runner);

        var status = await tweak.ProbeAsync();

        status.Should().Be(TweakStatus.NotApplied);
    }
}
