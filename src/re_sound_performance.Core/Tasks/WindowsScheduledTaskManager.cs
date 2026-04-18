using System.Diagnostics;
using System.Text;

namespace re_sound_performance.Core.Tasks;

public sealed class WindowsScheduledTaskManager : IScheduledTaskManager
{
    private const string SchtasksExecutable = "schtasks.exe";

    public bool TaskExists(string taskPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(taskPath);
        var (exitCode, _, _) = RunSchtasks($"/Query /TN \"{taskPath}\"");
        return exitCode == 0;
    }

    public ScheduledTaskState? GetState(string taskPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(taskPath);
        var (exitCode, stdout, _) = RunSchtasks($"/Query /TN \"{taskPath}\" /FO LIST /V");
        if (exitCode != 0)
        {
            return null;
        }

        foreach (var line in stdout.Split('\n'))
        {
            var trimmed = line.TrimEnd('\r');
            if (!trimmed.StartsWith("Status:", StringComparison.OrdinalIgnoreCase)
                && !trimmed.StartsWith("Estado:", StringComparison.OrdinalIgnoreCase)
                && !trimmed.StartsWith("Scheduled Task State:", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var value = trimmed.Split(':', 2)[1].Trim();
            if (value.Contains("Disabled", StringComparison.OrdinalIgnoreCase)
                || value.Contains("Deshabilitada", StringComparison.OrdinalIgnoreCase))
            {
                return ScheduledTaskState.Disabled;
            }

            if (value.Contains("Ready", StringComparison.OrdinalIgnoreCase)
                || value.Contains("Running", StringComparison.OrdinalIgnoreCase)
                || value.Contains("Listo", StringComparison.OrdinalIgnoreCase)
                || value.Contains("Habilitada", StringComparison.OrdinalIgnoreCase))
            {
                return ScheduledTaskState.Ready;
            }
        }

        return ScheduledTaskState.Unknown;
    }

    public void SetState(string taskPath, ScheduledTaskState state)
    {
        var outcome = TrySetState(taskPath, state);
        if (!outcome.IsSuccess)
        {
            throw new InvalidOperationException(outcome.ErrorMessage ?? "schtasks failed");
        }
    }

    public SetStateOutcome TrySetState(string taskPath, ScheduledTaskState state)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(taskPath);
        var flag = state switch
        {
            ScheduledTaskState.Disabled => "/DISABLE",
            ScheduledTaskState.Ready => "/ENABLE",
            _ => throw new InvalidOperationException($"Unsupported target state: {state}")
        };

        var (exitCode, _, stderr) = RunSchtasks($"/Change /TN \"{taskPath}\" {flag}");
        if (exitCode == 0)
        {
            return new SetStateOutcome(SetStateResult.Success, null);
        }

        var message = $"schtasks {flag} on {taskPath} failed (exit {exitCode}): {stderr.Trim()}";
        if (IsAccessDenied(stderr))
        {
            return new SetStateOutcome(SetStateResult.AccessDenied, message);
        }

        return new SetStateOutcome(SetStateResult.Failed, message);
    }

    private static bool IsAccessDenied(string stderr) =>
        stderr.Contains("Access is denied", StringComparison.OrdinalIgnoreCase)
        || stderr.Contains("Acceso denegado", StringComparison.OrdinalIgnoreCase)
        || stderr.Contains("ERROR: 5", StringComparison.OrdinalIgnoreCase);

    private static (int ExitCode, string StdOut, string StdErr) RunSchtasks(string arguments)
    {
        var psi = new ProcessStartInfo
        {
            FileName = SchtasksExecutable,
            Arguments = arguments,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        using var process = Process.Start(psi)
            ?? throw new InvalidOperationException("Could not start schtasks.exe");

        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();
        return (process.ExitCode, stdout, stderr);
    }
}
