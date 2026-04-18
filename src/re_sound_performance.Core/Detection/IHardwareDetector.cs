namespace re_sound_performance.Core.Detection;

public interface IHardwareDetector
{
    Task<HardwareInfo> DetectAsync(CancellationToken cancellationToken = default);
}
