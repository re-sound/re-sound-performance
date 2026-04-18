using re_sound_performance.Core.Services;

namespace re_sound_performance.Core.Detection;

public sealed class WindowsAntiCheatDetector : IAntiCheatDetector
{
    private static readonly string[] VanguardServices = { "vgc", "vgk" };
    private static readonly string[] VanguardPaths =
    {
        @"C:\Program Files\Riot Vanguard"
    };

    private static readonly string[] FaceitServices = { "FACEIT", "faceitservice" };
    private static readonly string[] FaceitPaths =
    {
        @"C:\Program Files\FACEIT AC",
        @"C:\Program Files (x86)\FACEIT AC"
    };

    private static readonly string[] BattlEyeServices = { "BEService", "BEDaisy" };
    private static readonly string[] BattlEyePaths =
    {
        @"C:\Program Files (x86)\Common Files\BattlEye"
    };

    private static readonly string[] EasyAntiCheatServices = { "EasyAntiCheat", "EasyAntiCheat_EOS" };
    private static readonly string[] EasyAntiCheatPaths =
    {
        @"C:\Program Files (x86)\EasyAntiCheat",
        @"C:\Program Files\EasyAntiCheat"
    };

    private static readonly string[] SteamLibraryGuessPaths =
    {
        @"C:\Program Files (x86)\Steam\steamapps\common",
        @"C:\Program Files\Steam\steamapps\common",
        @"D:\SteamLibrary\steamapps\common",
        @"D:\Steam\steamapps\common",
        @"E:\SteamLibrary\steamapps\common",
        @"E:\Steam\steamapps\common"
    };

    private readonly IServiceManager _services;
    private readonly IFileSystemProbe _fs;

    public WindowsAntiCheatDetector(IServiceManager services, IFileSystemProbe fileSystem)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
        _fs = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    }

    public Task<AntiCheatInfo> DetectAsync(CancellationToken cancellationToken = default)
    {
        var installed = AntiCheat.None;
        var details = new List<string>();

        if (TryMatch(VanguardServices, VanguardPaths, out var vanguardHit))
        {
            installed |= AntiCheat.Vanguard;
            details.Add($"Vanguard: {vanguardHit}");
        }

        if (TryMatch(FaceitServices, FaceitPaths, out var faceitHit))
        {
            installed |= AntiCheat.FaceitAc;
            details.Add($"FACEIT AC: {faceitHit}");
        }

        if (TryMatch(BattlEyeServices, BattlEyePaths, out var beHit))
        {
            installed |= AntiCheat.BattlEye;
            details.Add($"BattlEye: {beHit}");
        }

        if (!TryMatch(EasyAntiCheatServices, EasyAntiCheatPaths, out var eacHit))
        {
            eacHit = ProbeSteamLibrariesForEac();
        }

        if (!string.IsNullOrEmpty(eacHit))
        {
            installed |= AntiCheat.EasyAntiCheat;
            details.Add($"EasyAntiCheat: {eacHit}");
        }

        return Task.FromResult(new AntiCheatInfo(installed, details));
    }

    private bool TryMatch(string[] serviceCandidates, string[] pathCandidates, out string? match)
    {
        foreach (var service in serviceCandidates)
        {
            if (_services.ServiceExists(service))
            {
                match = service;
                return true;
            }
        }

        foreach (var path in pathCandidates)
        {
            if (_fs.DirectoryExists(path))
            {
                match = path;
                return true;
            }
        }

        match = null;
        return false;
    }

    private string? ProbeSteamLibrariesForEac()
    {
        foreach (var root in SteamLibraryGuessPaths)
        {
            if (!_fs.DirectoryExists(root))
            {
                continue;
            }

            var match = _fs.EnumerateDirectories(root, "EasyAntiCheat*", recursive: true).FirstOrDefault();
            if (match is not null)
            {
                return match;
            }
        }

        return null;
    }

}
