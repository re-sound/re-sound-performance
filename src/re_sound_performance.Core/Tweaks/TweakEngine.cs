using Microsoft.Extensions.Logging;
using re_sound_performance.Core.Backup;

namespace re_sound_performance.Core.Tweaks;

public sealed class TweakEngine
{
    private readonly IBackupStore _backupStore;
    private readonly ILogger<TweakEngine> _logger;
    private readonly IReadOnlyDictionary<string, ITweak> _tweaksById;
    private readonly TweakStateCache _cache;

    public TweakEngine(IEnumerable<ITweak> tweaks, IBackupStore backupStore, ILogger<TweakEngine> logger, TweakStateCache cache)
    {
        ArgumentNullException.ThrowIfNull(tweaks);
        _backupStore = backupStore ?? throw new ArgumentNullException(nameof(backupStore));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _tweaksById = tweaks.ToDictionary(t => t.Metadata.Id, StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyCollection<ITweak> AvailableTweaks => _tweaksById.Values.ToList();

    public TweakStateCache StateCache => _cache;

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
            await RefreshProbeAsync(tweak, cancellationToken).ConfigureAwait(false);
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
            await RefreshProbeAsync(tweak, cancellationToken).ConfigureAwait(false);
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
        return await RefreshProbeAsync(tweak, cancellationToken).ConfigureAwait(false);
    }

    public async Task ProbeAllAsync(CancellationToken cancellationToken = default)
    {
        var all = _tweaksById.Values.ToList();
        var total = all.Count;
        var completed = 0;

        _cache.RaiseProbingProgress(0, total);

        var tasks = all.Select(async tweak =>
        {
            await RefreshProbeAsync(tweak, cancellationToken).ConfigureAwait(false);
            var done = Interlocked.Increment(ref completed);
            _cache.RaiseProbingProgress(done, total);
        });

        await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    private async Task<TweakStatus> RefreshProbeAsync(ITweak tweak, CancellationToken cancellationToken)
    {
        TweakStatus status;
        try
        {
            status = await tweak.ProbeAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Probe {TweakId} threw an exception", tweak.Metadata.Id);
            status = TweakStatus.Unknown;
        }

        _cache.Set(tweak.Metadata.Id, status);
        return status;
    }
}
