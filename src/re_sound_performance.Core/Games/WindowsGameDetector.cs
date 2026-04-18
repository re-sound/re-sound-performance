using Microsoft.Win32;
using re_sound_performance.Core.Detection;
using re_sound_performance.Core.Registry;

namespace re_sound_performance.Core.Games;

public sealed class WindowsGameDetector : IGameDetector
{
    private static readonly string[] SteamLibraryGuessPaths =
    {
        @"C:\Program Files (x86)\Steam",
        @"C:\Program Files\Steam",
        @"D:\SteamLibrary",
        @"D:\Steam",
        @"E:\SteamLibrary",
        @"E:\Steam"
    };

    private static readonly string[] ApexOriginPaths =
    {
        @"C:\Program Files (x86)\Origin Games\Apex",
        @"C:\Program Files\Origin Games\Apex"
    };

    private static readonly string[] ApexEaPaths =
    {
        @"C:\Program Files\EA Games\Apex",
        @"C:\Program Files (x86)\EA Games\Apex"
    };

    private readonly IRegistryAccess _registry;
    private readonly IFileSystemProbe _fs;

    public WindowsGameDetector(IRegistryAccess registry, IFileSystemProbe fileSystem)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _fs = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    }

    public Task<GameInstallation> DetectAsync(GameId game, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(game switch
        {
            GameId.Cs2 => DetectCs2(),
            GameId.Valorant => DetectValorant(),
            GameId.Apex => DetectApex(),
            _ => GameInstallation.NotFound(game)
        });
    }

    private GameInstallation DetectCs2()
    {
        foreach (var steam in SteamLibraryGuessPaths)
        {
            var candidate = JoinWindows(steam, "steamapps", "common", "Counter-Strike Global Offensive");
            if (_fs.DirectoryExists(candidate))
            {
                return new GameInstallation(GameId.Cs2, true, candidate, GameLauncher.Steam, null);
            }
        }

        return GameInstallation.NotFound(GameId.Cs2);
    }

    private static string JoinWindows(params string[] parts)
    {
        var result = new System.Text.StringBuilder();
        for (var i = 0; i < parts.Length; i++)
        {
            if (i > 0)
            {
                result.Append('\\');
            }
            result.Append(parts[i].TrimEnd('\\', '/'));
        }
        return result.ToString();
    }

    private GameInstallation DetectValorant()
    {
        var raw = _registry.GetValue(RegistryHive.LocalMachine,
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Riot Game valorant.live",
            "InstallLocation") as string;

        var fallback = new[]
        {
            @"C:\Riot Games\VALORANT\live",
            @"D:\Riot Games\VALORANT\live"
        };

        if (!string.IsNullOrWhiteSpace(raw) && _fs.DirectoryExists(raw))
        {
            return new GameInstallation(GameId.Valorant, true, raw, GameLauncher.RiotClient, null);
        }

        foreach (var path in fallback)
        {
            if (_fs.DirectoryExists(path))
            {
                return new GameInstallation(GameId.Valorant, true, path, GameLauncher.RiotClient, null);
            }
        }

        return GameInstallation.NotFound(GameId.Valorant);
    }

    private GameInstallation DetectApex()
    {
        foreach (var steam in SteamLibraryGuessPaths)
        {
            var candidate = JoinWindows(steam, "steamapps", "common", "Apex Legends");
            if (_fs.DirectoryExists(candidate))
            {
                return new GameInstallation(GameId.Apex, true, candidate, GameLauncher.Steam, null);
            }
        }

        foreach (var path in ApexEaPaths)
        {
            if (_fs.DirectoryExists(path))
            {
                return new GameInstallation(GameId.Apex, true, path, GameLauncher.EaApp,
                    "EA App ignores `+` launch options set through the shortcut. Set them in the EA App properties > Game settings instead.");
            }
        }

        foreach (var path in ApexOriginPaths)
        {
            if (_fs.DirectoryExists(path))
            {
                return new GameInstallation(GameId.Apex, true, path, GameLauncher.Origin, null);
            }
        }

        return GameInstallation.NotFound(GameId.Apex);
    }
}
