using System.Diagnostics;

namespace re_sound_performance.Core.Power;

public sealed class WindowsPowerCfgRunner : IPowerCfgRunner
{
    public async Task<string> ExecuteAsync(string arguments, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(arguments);

        var startInfo = new ProcessStartInfo
        {
            FileName = "powercfg.exe",
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Failed to start powercfg.exe.");

        var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);

        await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);

        var output = await outputTask.ConfigureAwait(false);
        var error = await errorTask.ConfigureAwait(false);

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"powercfg.exe exited with code {process.ExitCode}. Error: {error}. Output: {output}");
        }

        return output;
    }
}
