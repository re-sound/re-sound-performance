using Microsoft.Extensions.Logging;
using re_sound_performance.Core.Backup;

namespace re_sound_performance.Core.Tweaks;

public sealed class TweakEngine
{
    private readonly IBackupStore _backupStore;
    private readonly ILogger<TweakEngine> _logger;
    private readonly IReadOnlyDictionary<string, ITweak> _tweaksById;

    public TweakEngine(IEnumerable<ITweak> tweaks, IBackupStore backupStore, ILogger<TweakEngine> logger)
    {
        ArgumentNullException.ThrowIfNull(tweaks);
        _backupStore = backupStore ?? throw new ArgumentNullException(nameof(backupStore));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _tweaksById = tweaks.ToDictionary(t => t.Metadata.Id, StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyCollection<ITweak> AvailableTweaks => _tweaksById.Values.ToList();

    public ITweak? Resolve(string tweakId) =>
        _tweaksById.TryGetValue(tweakId, out var tweak) ? tweak : null;

    public async Task<TweakResult> ApplyAsync(string tweakId, CancellationToken cancellationToken = default)
    {
        var tweak = Resolve(tweakId) ?? throw new KeyNotFoundException($"Unknown tweak id: {tweakId}");
        _logger.LogInformation("Applying tweak {TweakId}", tweakId);

        try
        {
            var result = await tweak.ApplyAsync(_backupStore, cancellationToken).ConfigureAwait(false);
            _logger.LogInformation("Apply {TweakId} result: Success={Success} Status={Status}", tweakId, result.Success, result.ResultingStatus);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Apply {TweakId} threw an exception", tweakId);
            return TweakResult.Fail(tweakId, ex.Message);
        }
    }

    public async Task<TweakResult> RevertAsync(string tweakId, CancellationToken cancellationToken = default)
    {
        var tweak = Resolve(tweakId) ?? throw new KeyNotFoundException($"Unknown tweak id: {tweakId}");
        _logger.LogInformation("Reverting tweak {TweakId}", tweakId);

        try
        {
            var result = await tweak.RevertAsync(_backupStore, cancellationToken).ConfigureAwait(false);
            _logger.LogInformation("Revert {TweakId} result: Success={Success} Status={Status}", tweakId, result.Success, result.ResultingStatus);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Revert {TweakId} threw an exception", tweakId);
            return TweakResult.Fail(tweakId, ex.Message);
        }
    }

    public async Task<TweakStatus> ProbeAsync(string tweakId, CancellationToken cancellationToken = default)
    {
        var tweak = Resolve(tweakId) ?? throw new KeyNotFoundException($"Unknown tweak id: {tweakId}");
        try
        {
            return await tweak.ProbeAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Probe {TweakId} threw an exception", tweakId);
            return TweakStatus.Unknown;
        }
    }
}
