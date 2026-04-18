using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using re_sound_performance.Core.Backup;
using re_sound_performance.Core.Detection;
using re_sound_performance.Core.Presets;
using re_sound_performance.Core.Tweaks;
using re_sound_performance.Tests.Tweaks;

namespace re_sound_performance.Tests.Presets;

public sealed class PresetRunnerTests
{
    [Fact]
    public async Task Run_ApplyAllTweaks_ReportsApplied()
    {
        var engine = BuildEngine(("a", TweakStatus.NotApplied), ("b", TweakStatus.NotApplied));
        var context = new DetectionContext();
        var runner = new PresetRunner(engine.Engine, context, NullLogger<PresetRunner>.Instance);
        var preset = new Preset(PresetKind.Safe, "Test", "tag", "desc", new[] { "a", "b" });

        var summary = await runner.RunAsync(preset);

        summary.Applied.Should().Be(2);
        summary.Failed.Should().Be(0);
        summary.Blocked.Should().Be(0);
        summary.Cancelled.Should().BeFalse();
        summary.Steps.Should().HaveCount(2);
    }

    [Fact]
    public async Task Run_SkipsAlreadyApplied()
    {
        var engine = BuildEngine(("a", TweakStatus.Applied), ("b", TweakStatus.NotApplied));
        var runner = new PresetRunner(engine.Engine, new DetectionContext(), NullLogger<PresetRunner>.Instance);
        var preset = new Preset(PresetKind.Safe, "Test", "tag", "desc", new[] { "a", "b" });

        var summary = await runner.RunAsync(preset);

        summary.Applied.Should().Be(1);
        summary.Skipped.Should().Be(1);
        summary.Steps.Should().Contain(s => s.Outcome == PresetStepOutcome.Skipped);
    }

    [Fact]
    public async Task Run_BlocksWhenVanguardInstalledAndTweakBlocked()
    {
        var engine = BuildEngineWithFlags(("a", TweakStatus.NotApplied, true, false));
        var context = new DetectionContext();
        context.SetAntiCheat(new AntiCheatInfo(AntiCheat.Vanguard, new[] { "vgc" }));
        var runner = new PresetRunner(engine.Engine, context, NullLogger<PresetRunner>.Instance);
        var preset = new Preset(PresetKind.Safe, "Test", "tag", "desc", new[] { "a" });

        var summary = await runner.RunAsync(preset);

        summary.Blocked.Should().Be(1);
        summary.Applied.Should().Be(0);
        summary.Steps[0].Outcome.Should().Be(PresetStepOutcome.Blocked);
    }

    [Fact]
    public async Task Run_UnknownTweakIdIsUnavailable()
    {
        var engine = BuildEngine(("a", TweakStatus.NotApplied));
        var runner = new PresetRunner(engine.Engine, new DetectionContext(), NullLogger<PresetRunner>.Instance);
        var preset = new Preset(PresetKind.Safe, "Test", "tag", "desc", new[] { "missing", "a" });

        var summary = await runner.RunAsync(preset);

        summary.Unavailable.Should().Be(1);
        summary.Applied.Should().Be(1);
    }

    [Fact]
    public async Task Run_CancellationStopsRun()
    {
        var engine = BuildEngine(("a", TweakStatus.NotApplied), ("b", TweakStatus.NotApplied));
        var runner = new PresetRunner(engine.Engine, new DetectionContext(), NullLogger<PresetRunner>.Instance);
        var preset = new Preset(PresetKind.Safe, "Test", "tag", "desc", new[] { "a", "b" });
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var summary = await runner.RunAsync(preset, cancellationToken: cts.Token);

        summary.Cancelled.Should().BeTrue();
        summary.Applied.Should().Be(0);
    }

    [Fact]
    public async Task Run_ReportsProgressForEachStep()
    {
        var engine = BuildEngine(("a", TweakStatus.NotApplied), ("b", TweakStatus.NotApplied));
        var runner = new PresetRunner(engine.Engine, new DetectionContext(), NullLogger<PresetRunner>.Instance);
        var preset = new Preset(PresetKind.Safe, "Test", "tag", "desc", new[] { "a", "b" });
        var reported = new List<PresetProgress>();
        var progress = new Progress<PresetProgress>(p => reported.Add(p));

        await runner.RunAsync(preset, progress);

        await Task.Delay(30);

        reported.Should().HaveCount(2);
        reported.Select(r => r.Completed).Should().Equal(1, 2);
    }

    private static EngineFixture BuildEngine(params (string TweakId, TweakStatus InitialStatus)[] tweaks)
    {
        var withFlags = tweaks.Select(t => (t.TweakId, t.InitialStatus, false, false)).ToArray();
        return BuildEngineWithFlags(withFlags);
    }

    private static EngineFixture BuildEngineWithFlags(params (string TweakId, TweakStatus InitialStatus, bool BlockedVanguard, bool BlockedFaceit)[] tweaks)
    {
        var items = tweaks
            .Select(t => (ITweak)new FakeTweak(t.TweakId, t.InitialStatus, t.BlockedVanguard, t.BlockedFaceit))
            .ToList();
        var cache = new TweakStateCache();
        var engine = new TweakEngine(items, new InMemoryBackupStore(), NullLogger<TweakEngine>.Instance, cache);
        foreach (var tweak in items)
        {
            cache.Set(tweak.Metadata.Id, tweak.ProbeAsync().GetAwaiter().GetResult());
        }
        return new EngineFixture(engine, items);
    }

    private sealed record EngineFixture(TweakEngine Engine, List<ITweak> Tweaks);

    private sealed class FakeTweak : ITweak
    {
        public FakeTweak(string id, TweakStatus initialStatus, bool blockedVanguard, bool blockedFaceit)
        {
            _status = initialStatus;
            Metadata = new TweakMetadata(
                id,
                id,
                "short",
                "detailed",
                "modifies",
                "impact",
                TweakCategory.System,
                TweakRisk.Safe,
                TweakEvidenceLevel.Confirmed,
                Array.Empty<string>(),
                Array.Empty<string>(),
                RequiresRestart: false,
                BlockedWhenVanguardInstalled: blockedVanguard,
                BlockedWhenFaceitInstalled: blockedFaceit);
        }

        private TweakStatus _status;

        public TweakMetadata Metadata { get; }

        public Task<TweakStatus> ProbeAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(_status);

        public Task<TweakResult> ApplyAsync(IBackupStore backupStore, CancellationToken cancellationToken = default)
        {
            _status = TweakStatus.Applied;
            return Task.FromResult(TweakResult.Ok(Metadata.Id, TweakStatus.Applied));
        }

        public Task<TweakResult> RevertAsync(IBackupStore backupStore, CancellationToken cancellationToken = default)
        {
            _status = TweakStatus.NotApplied;
            return Task.FromResult(TweakResult.Ok(Metadata.Id, TweakStatus.NotApplied));
        }
    }
}
