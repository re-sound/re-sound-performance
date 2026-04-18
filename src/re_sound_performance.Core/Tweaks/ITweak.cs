using re_sound_performance.Core.Backup;

namespace re_sound_performance.Core.Tweaks;

public interface ITweak
{
    TweakMetadata Metadata { get; }

    Task<TweakStatus> ProbeAsync(CancellationToken cancellationToken = default);

    Task<TweakResult> ApplyAsync(IBackupStore backupStore, CancellationToken cancellationToken = default);

    Task<TweakResult> RevertAsync(IBackupStore backupStore, CancellationToken cancellationToken = default);
}
