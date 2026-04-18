using re_sound_performance.Core.Backup;
using re_sound_performance.Core.Power;

namespace re_sound_performance.Core.Tweaks.Implementations;

public sealed class DisableHibernationTweak : ITweak
{
    private readonly IPowerCfgRunner _powerCfg;

    public DisableHibernationTweak(IPowerCfgRunner powerCfg)
    {
        _powerCfg = powerCfg ?? throw new ArgumentNullException(nameof(powerCfg));
    }

    public TweakMetadata Metadata { get; } = new(
        Id: "power.disable_hibernation",
        Name: "Disable hibernation and Fast Startup",
        ShortDescription: "Deletes the hibernation file and disables hybrid shutdown.",
        DetailedDescription: "Turning hibernation off via powercfg deletes hiberfil.sys, which typically consumes between 40 and 70 percent of installed RAM on disk. As a side effect, Fast Startup (hybrid shutdown) is disabled, so shutting down the PC actually stops the kernel instead of caching it. Many gaming issues caused by stale driver state survive across Fast Startup boots. On laptops, evaluate the trade-off before applying.",
        Modifies: "Runs powercfg -h off. Deletes C:\\hiberfil.sys. Disables Fast Startup as a side effect.",
        ExpectedImpact: "Frees 40 to 70 percent of installed RAM on disk. Shutdown behaves as a true cold shutdown. Cold boot is a few seconds slower but ensures clean driver state.",
        Category: TweakCategory.Power,
        Risk: TweakRisk.Medium,
        Evidence: TweakEvidenceLevel.Confirmed,
        Sources: new[]
        {
            "Microsoft Learn: Disable or re-enable hibernation",
            "Microsoft Learn: Fast Startup issues and kernel state"
        },
        IncompatibleWith: new[] { "battery-powered-laptop-requires-hibernate" },
        RequiresRestart: false,
        BlockedWhenVanguardInstalled: false,
        BlockedWhenFaceitInstalled: false);

    public async Task<TweakStatus> ProbeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var output = await _powerCfg.ExecuteAsync("/availablesleepstates", cancellationToken).ConfigureAwait(false);
            return output.Contains("Hibernate", StringComparison.OrdinalIgnoreCase)
                ? TweakStatus.NotApplied
                : TweakStatus.Applied;
        }
        catch
        {
            return TweakStatus.Unknown;
        }
    }

    public async Task<TweakResult> ApplyAsync(IBackupStore backupStore, CancellationToken cancellationToken = default)
    {
        var marker = System.Text.Encoding.UTF8.GetBytes("hibernation_was_enabled");
        var backupReference = await backupStore.SaveAsync(Metadata.Id, "state.txt", marker, cancellationToken).ConfigureAwait(false);
        await _powerCfg.ExecuteAsync("-h off", cancellationToken).ConfigureAwait(false);
        return TweakResult.Ok(Metadata.Id, TweakStatus.Applied, new[] { backupReference });
    }

    public async Task<TweakResult> RevertAsync(IBackupStore backupStore, CancellationToken cancellationToken = default)
    {
        await _powerCfg.ExecuteAsync("-h on", cancellationToken).ConfigureAwait(false);
        var references = await backupStore.ListForTweakAsync(Metadata.Id, cancellationToken).ConfigureAwait(false);
        return TweakResult.Ok(Metadata.Id, TweakStatus.NotApplied, references);
    }
}
