using re_sound_performance.Core.Backup;
using re_sound_performance.Core.Services;

namespace re_sound_performance.Core.Tweaks.Implementations;

public sealed class DisableFaxAndRetailDemoTweak : ITweak
{
    private static readonly IReadOnlyList<ServiceChange> AppliedChanges = new[]
    {
        new ServiceChange("Fax", ServiceStartupType.Disabled),
        new ServiceChange("RetailDemo", ServiceStartupType.Disabled),
        new ServiceChange("WMPNetworkSvc", ServiceStartupType.Disabled),
        new ServiceChange("WbioSrvc", ServiceStartupType.Manual)
    };

    private readonly IServiceManager _services;

    public DisableFaxAndRetailDemoTweak(IServiceManager services)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
    }

    public TweakMetadata Metadata { get; } = new(
        Id: "system.disable_legacy_services",
        Name: "Disable Fax, Retail Demo, WMP Network and Biometric services",
        ShortDescription: "Disables four services that desktop gamers almost never use.",
        DetailedDescription: "Fax is the legacy fax sending service. RetailDemo runs in-store display mode and should never start on a personal PC. WMPNetworkSvc shares Windows Media Player libraries. WbioSrvc is the biometric service and is safe to switch to Manual on systems without Windows Hello. Each service removed saves memory and reduces the attack surface.",
        Modifies: "Service startup type for Fax, RetailDemo, WMPNetworkSvc (Disabled) and WbioSrvc (Manual)",
        ExpectedImpact: "Small RAM savings. Faster boot by a few hundred milliseconds on HDD systems. On SSD, negligible.",
        Category: TweakCategory.System,
        Risk: TweakRisk.Safe,
        Evidence: TweakEvidenceLevel.Confirmed,
        Sources: new[]
        {
            "Black Viper service configurations: safe-to-disable list",
            "Kartones: Windows 11 services for gaming"
        },
        IncompatibleWith: Array.Empty<string>(),
        RequiresRestart: true,
        BlockedWhenVanguardInstalled: false,
        BlockedWhenFaceitInstalled: false);

    public Task<TweakStatus> ProbeAsync(CancellationToken cancellationToken = default) =>
        ServiceTweakHelper.ProbeAsync(_services, AppliedChanges);

    public Task<TweakResult> ApplyAsync(IBackupStore backupStore, CancellationToken cancellationToken = default) =>
        ServiceTweakHelper.ApplyAsync(_services, backupStore, Metadata.Id, AppliedChanges, cancellationToken);

    public Task<TweakResult> RevertAsync(IBackupStore backupStore, CancellationToken cancellationToken = default) =>
        ServiceTweakHelper.RevertAsync(_services, backupStore, Metadata.Id, AppliedChanges, cancellationToken);
}
