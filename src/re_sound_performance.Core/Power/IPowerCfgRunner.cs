namespace re_sound_performance.Core.Power;

public interface IPowerCfgRunner
{
    Task<string> ExecuteAsync(string arguments, CancellationToken cancellationToken = default);
}
