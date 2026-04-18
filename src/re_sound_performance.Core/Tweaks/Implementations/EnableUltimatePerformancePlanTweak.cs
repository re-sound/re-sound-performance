using re_sound_performance.Core.Backup;
using re_sound_performance.Core.Power;

namespace re_sound_performance.Core.Tweaks.Implementations;

public sealed class EnableUltimatePerformancePlanTweak : ITweak
{
    private readonly IPowerCfgRunner _powerCfg;

    public EnableUltimatePerformancePlanTweak(IPowerCfgRunner powerCfg)
    {
        _powerCfg = powerCfg ?? throw new ArgumentNullException(nameof(powerCfg));
    }

    public TweakMetadata Metadata { get; } = new(
        Id: "power.enable_ultimate_performance",
        Name: "Enable Ultimate Performance power plan",
        ShortDescription: "Imports the hidden Ultimate Performance scheme and activates it.",
        DetailedDescription: "Ultimate Performance is an official Microsoft power scheme hidden on Home and Pro editions. It disables core parking, keeps minimum processor state at 100 percent and removes PCI Express link state power management. On desktops it delivers the most consistent frame pacing at the cost of higher idle power draw. On laptops it hurts battery life and is not recommended on battery power.",
        Modifies: "Creates a power scheme from the well-known Ultimate Performance GUID (e9a42b02-d5df-448d-aa00-03f14749eb61) and sets it active.",
        ExpectedImpact: "Eliminates core parking wake-up latency spikes. Improves 1 percent low FPS consistency on CPU-bound workloads. Increases idle power draw.",
        Category: TweakCategory.Power,
        Risk: TweakRisk.Safe,
        Evidence: TweakEvidenceLevel.Confirmed,
        Sources: new[]
        {
            "Microsoft Learn: Power plan management with powercfg",
            "Microsoft Community: Ultimate Performance plan availability on Windows 11"
        },
        IncompatibleWith: new[] { "battery-powered-laptop" },
        RequiresRestart: false,
        BlockedWhenVanguardInstalled: false,
        BlockedWhenFaceitInstalled: false);

    public async Task<TweakStatus> ProbeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var output = await _powerCfg.ExecuteAsync("/getactivescheme", cancellationToken).ConfigureAwait(false);
            return output.Contains(PowerPlanGuids.UltimatePerformance.ToString(), StringComparison.OrdinalIgnoreCase)
                ? TweakStatus.Applied
                : TweakStatus.NotApplied;
        }
        catch
        {
            return TweakStatus.Unknown;
        }
    }

    public async Task<TweakResult> ApplyAsync(IBackupStore backupStore, CancellationToken cancellationToken = default)
    {
        var previousScheme = await ReadActiveSchemeAsync(cancellationToken).ConfigureAwait(false);
        var backupPayload = System.Text.Encoding.UTF8.GetBytes(previousScheme?.ToString() ?? string.Empty);
        var backupReference = await backupStore.SaveAsync(Metadata.Id, "active_scheme.txt", backupPayload, cancellationToken).ConfigureAwait(false);

        try
        {
            await _powerCfg.ExecuteAsync($"/duplicatescheme {PowerPlanGuids.UltimatePerformance}", cancellationToken).ConfigureAwait(false);
        }
        catch
        {
        }

        await _powerCfg.ExecuteAsync($"/setactive {PowerPlanGuids.UltimatePerformance}", cancellationToken).ConfigureAwait(false);
        return TweakResult.Ok(Metadata.Id, TweakStatus.Applied, new[] { backupReference });
    }

    public async Task<TweakResult> RevertAsync(IBackupStore backupStore, CancellationToken cancellationToken = default)
    {
        var references = await backupStore.ListForTweakAsync(Metadata.Id, cancellationToken).ConfigureAwait(false);
        if (references.Count == 0)
        {
            await _powerCfg.ExecuteAsync($"/setactive {PowerPlanGuids.Balanced}", cancellationToken).ConfigureAwait(false);
            return TweakResult.Ok(Metadata.Id, TweakStatus.NotApplied, message: "No backup found, restored Balanced plan.");
        }

        var latestReference = references[0];
        var payload = await backupStore.LoadAsync(latestReference, cancellationToken).ConfigureAwait(false);
        var text = payload is null ? string.Empty : System.Text.Encoding.UTF8.GetString(payload);

        if (!Guid.TryParse(text, out var previousGuid))
        {
            previousGuid = PowerPlanGuids.Balanced;
        }

        await _powerCfg.ExecuteAsync($"/setactive {previousGuid}", cancellationToken).ConfigureAwait(false);
        return TweakResult.Ok(Metadata.Id, TweakStatus.NotApplied, new[] { latestReference });
    }

    private async Task<Guid?> ReadActiveSchemeAsync(CancellationToken cancellationToken)
    {
        try
        {
            var output = await _powerCfg.ExecuteAsync("/getactivescheme", cancellationToken).ConfigureAwait(false);
            var tokens = output.Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var token in tokens)
            {
                if (Guid.TryParse(token, out var parsed))
                {
                    return parsed;
                }
            }
        }
        catch
        {
        }

        return null;
    }
}
