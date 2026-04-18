namespace re_sound_performance.Core.Detection;

public interface IAntiCheatDetector
{
    Task<AntiCheatInfo> DetectAsync(CancellationToken cancellationToken = default);
}
