using System.Diagnostics;
using System.Text;

namespace re_sound_performance.Core.Appx;

public sealed class PowerShellAppxManager : IAppxManager
{
    private const string PowerShellExecutable = "powershell.exe";

    public IReadOnlyList<AppxPackageInfo> FindInstalled(string packageName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packageName);
        var script = $"Get-AppxPackage -AllUsers -Name '{EscapeSingleQuotes(packageName)}' | Select-Object -ExpandProperty PackageFullName";
        var (exitCode, stdout, _) = RunPowerShell(script);
        if (exitCode != 0 || string.IsNullOrWhiteSpace(stdout))
        {
            return Array.Empty<AppxPackageInfo>();
        }

        return stdout.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(line => line.Length > 0)
            .Select(fullName => new AppxPackageInfo(packageName, fullName, AppxPackageKind.User))
            .ToArray();
    }

    public bool IsProvisioned(string packageName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packageName);
        var script = $"Get-AppxProvisionedPackage -Online | Where-Object DisplayName -eq '{EscapeSingleQuotes(packageName)}' | Select-Object -ExpandProperty DisplayName";
        var (exitCode, stdout, _) = RunPowerShell(script);
        return exitCode == 0 && !string.IsNullOrWhiteSpace(stdout);
    }

    public void RemoveForAllUsers(string packageName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packageName);
        var script = $"Get-AppxPackage -AllUsers -Name '{EscapeSingleQuotes(packageName)}' | Remove-AppxPackage -AllUsers -ErrorAction SilentlyContinue";
        var (exitCode, _, stderr) = RunPowerShell(script);
        if (exitCode != 0)
        {
            throw new InvalidOperationException($"Remove-AppxPackage for {packageName} failed (exit {exitCode}): {stderr.Trim()}");
        }
    }

    public void RemoveProvisioned(string packageName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packageName);
        var script = $"Get-AppxProvisionedPackage -Online | Where-Object DisplayName -eq '{EscapeSingleQuotes(packageName)}' | Remove-AppxProvisionedPackage -Online -ErrorAction SilentlyContinue | Out-Null";
        var (exitCode, _, stderr) = RunPowerShell(script);
        if (exitCode != 0)
        {
            throw new InvalidOperationException($"Remove-AppxProvisionedPackage for {packageName} failed (exit {exitCode}): {stderr.Trim()}");
        }
    }

    private static string EscapeSingleQuotes(string value) => value.Replace("'", "''", StringComparison.Ordinal);

    private static (int ExitCode, string StdOut, string StdErr) RunPowerShell(string script)
    {
        var psi = new ProcessStartInfo
        {
            FileName = PowerShellExecutable,
            Arguments = $"-NoProfile -NonInteractive -ExecutionPolicy Bypass -Command \"{script.Replace("\"", "\\\"", StringComparison.Ordinal)}\"",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        using var process = Process.Start(psi)
            ?? throw new InvalidOperationException("Could not start powershell.exe");

        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();
        return (process.ExitCode, stdout, stderr);
    }
}
