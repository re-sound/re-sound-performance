using Microsoft.Extensions.Logging;
using re_sound_performance.Core.Detection;
using re_sound_performance.Core.Tweaks;

namespace re_sound_performance.Core.Presets;

public sealed class PresetRunner
{
    private readonly TweakEngine _engine;
    private readonly DetectionContext _detection;
    private readonly ILogger<PresetRunner> _logger;

    public PresetRunner(TweakEngine engine, DetectionContext detection, ILogger<PresetRunner> logger)
    {
        _engine = engine ?? throw new ArgumentNullException(nameof(engine));
        _detection = detection ?? throw new ArgumentNullException(nameof(detection));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PresetRunSummary> RunAsync(
        Preset preset,
        IProgress<PresetProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(preset);

        var steps = new List<PresetProgress>(preset.TweakIds.Count);
        var applied = 0;
        var skipped = 0;
        var blocked = 0;
        var failed = 0;
        var unavailable = 0;
        var cancelled = false;
        var total = preset.TweakIds.Count;

        for (var i = 0; i < preset.TweakIds.Count; i++)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                cancelled = true;
                break;
            }

            var tweakId = preset.TweakIds[i];
            var tweak = _engine.Resolve(tweakId);
            if (tweak is null)
            {
                var step = new PresetProgress(i + 1, total, tweakId, tweakId, PresetStepOutcome.Unavailable, "Tweak not registered in this build.");
                unavailable++;
                steps.Add(step);
                progress?.Report(step);
                continue;
            }

            var currentStatus = _engine.StateCache.GetOrUnknown(tweakId);
            if (currentStatus == TweakStatus.Applied)
            {
                var step = new PresetProgress(i + 1, total, tweakId, tweak.Metadata.Name, PresetStepOutcome.Skipped, "Already applied.");
                skipped++;
                steps.Add(step);
                progress?.Report(step);
                continue;
            }

            var gate = TweakGate.Evaluate(tweak.Metadata, _detection.AntiCheat);
            if (!gate.Allowed)
            {
                var step = new PresetProgress(i + 1, total, tweakId, tweak.Metadata.Name, PresetStepOutcome.Blocked, gate.Reason);
                blocked++;
                steps.Add(step);
                progress?.Report(step);
                continue;
            }

            PresetProgress stepResult;
            try
            {
                var result = await _engine.ApplyAsync(tweakId, cancellationToken).ConfigureAwait(false);
                var verified = _engine.StateCache.GetOrUnknown(tweakId);
                var outcome = (result.Success, verified) switch
                {
                    (true, TweakStatus.Applied) => PresetStepOutcome.Applied,
                    (true, TweakStatus.PartiallyApplied) => PresetStepOutcome.Failed,
                    (true, TweakStatus.Unavailable) => PresetStepOutcome.Unavailable,
                    (true, TweakStatus.NotApplied) => PresetStepOutcome.Failed,
                    _ => PresetStepOutcome.Failed
                };

                stepResult = new PresetProgress(i + 1, total, tweakId, tweak.Metadata.Name, outcome, result.Message);

                switch (outcome)
                {
                    case PresetStepOutcome.Applied: applied++; break;
                    case PresetStepOutcome.Unavailable: unavailable++; break;
                    default: failed++; break;
                }
            }
            catch (OperationCanceledException)
            {
                cancelled = true;
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Preset {Preset} step {TweakId} threw", preset.Kind, tweakId);
                stepResult = new PresetProgress(i + 1, total, tweakId, tweak.Metadata.Name, PresetStepOutcome.Failed, ex.Message);
                failed++;
            }

            steps.Add(stepResult);
            progress?.Report(stepResult);
        }

        return new PresetRunSummary(preset.Kind, applied, skipped, blocked, failed, unavailable, cancelled, steps);
    }
}
