using re_sound_performance.Core.Backup;
using re_sound_performance.Core.Services;

namespace re_sound_performance.Core.Tweaks.Implementations;

public sealed class DisableXboxServicesTweak : ITweak
{
    private static readonly IReadOnlyList<ServiceChange> AppliedChanges = new[]
    {
        new ServiceChange("XblAuthManager", ServiceStartupType.Disabled),
        new ServiceChange("XblGameSave", ServiceStartupType.Disabled),
        new ServiceChange("XboxNetApiSvc", ServiceStartupType.Disabled),
        new ServiceChange("XboxGipSvc", ServiceStartupType.Disabled)
    };

    private readonly IServiceManager _services;

    public DisableXboxServicesTweak(IServiceManager services)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
    }

    public TweakMetadata Metadata { get; } = new(
        Id: "system.disable_xbox_services",
        Name: "Disable Xbox background services",
        ShortDescription: "Disables the four Xbox Live and controller helper services. Recommended only if you do not use Xbox Game Pass, Xbox titles on PC, or the Xbox Wireless Adapter.",
        DetailedDescription: "XblAuthManager and XboxNetApiSvc handle Xbox Live sign-in and networking. XblGameSave syncs cloud saves for UWP titles. XboxGipSvc powers the Xbox Wireless Adapter and the Xbox Accessories app. Players who use only Steam, Epic, GOG or standalone launchers can disable all four. Users of Xbox Game Pass titles, Microsoft Store games or Xbox controllers on Windows should leave this tweak off.",
        Modifies: "Service startup type for XblAuthManager, XblGameSave, XboxNetApiSvc, XboxGipSvc",
        ExpectedImpact: "Removes four resident services. Saves 40 to 80 MB of RAM and reduces boot-time service initialization.",
        Category: TweakCategory.System,
        Risk: TweakRisk.Medium,
        Evidence: TweakEvidenceLevel.Confirmed,
        Sources: new[]
        {
            "Microsoft Learn: Xbox Live services and game save",
            "Windows Digitals: Xbox services disable guide"
        },
        IncompatibleWith: new[] { "game-pass", "xbox-controller-via-dongle" },
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
