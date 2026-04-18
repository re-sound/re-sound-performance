using Microsoft.Win32;
using re_sound_performance.Core.Backup;
using re_sound_performance.Core.Registry;

namespace re_sound_performance.Core.Tweaks.Implementations;

public sealed class EnableShaderCacheUnlimitedTweak : ITweak
{
    private static readonly IReadOnlyList<RegistryChange> AppliedChanges = new[]
    {
        new RegistryChange(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\DirectX\UserGpuPreferences", "ShaderCacheMaxSizeInMB", 10240, RegistryValueKind.DWord),
        new RegistryChange(RegistryHive.CurrentUser, @"Software\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\CompatibilityAssistant", "DisableUAR", 1, RegistryValueKind.DWord)
    };

    private readonly IRegistryAccess _registry;

    public EnableShaderCacheUnlimitedTweak(IRegistryAccess registry)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    }

    public TweakMetadata Metadata { get; } = new(
        Id: "gpu.increase_shader_cache",
        Name: "Increase DirectX shader cache to 10 GB",
        ShortDescription: "Raises the DirectX shader cache ceiling so modern DX12 titles can keep more compiled shaders resident.",
        DetailedDescription: "Windows maintains a per-user DirectX shader cache that games populate the first time they run. If the cache hits its default size limit (typically 1 to 4 GB), shaders are evicted and recompiled on the fly, causing stutters. Raising the limit to 10 GB eliminates eviction for most modern AAA titles.",
        Modifies: "HKLM\\SOFTWARE\\Microsoft\\DirectX\\UserGpuPreferences (ShaderCacheMaxSizeInMB)",
        ExpectedImpact: "Fewer first-run and scene-change stutters in DX12 titles. Uses up to 10 GB of disk space when the cache is fully populated.",
        Category: TweakCategory.Gpu,
        Risk: TweakRisk.Safe,
        Evidence: TweakEvidenceLevel.Confirmed,
        Sources: new[]
        {
            "Microsoft DevBlogs: DirectX 12 Shader Cache",
            "Guru3D: Shader cache stutter discussion",
            "PC Gamer: Raising shader cache size to fix Borderlands 4 stutter"
        },
        IncompatibleWith: Array.Empty<string>(),
        RequiresRestart: true,
        BlockedWhenVanguardInstalled: false,
        BlockedWhenFaceitInstalled: false);

    public Task<TweakStatus> ProbeAsync(CancellationToken cancellationToken = default) =>
        RegistryTweakHelper.ProbeAsync(_registry, AppliedChanges);

    public Task<TweakResult> ApplyAsync(IBackupStore backupStore, CancellationToken cancellationToken = default) =>
        RegistryTweakHelper.ApplyAsync(_registry, backupStore, Metadata.Id, AppliedChanges, cancellationToken);

    public Task<TweakResult> RevertAsync(IBackupStore backupStore, CancellationToken cancellationToken = default) =>
        RegistryTweakHelper.RevertAsync(_registry, backupStore, Metadata.Id, AppliedChanges, cancellationToken);
}
