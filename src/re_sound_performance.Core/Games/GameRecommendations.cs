namespace re_sound_performance.Core.Games;

public static class GameRecommendations
{
    public static RecommendedConfig Cs2 { get; } = new(
        LaunchOptions: "-novid -tickrate 128 -freq 240 +fps_max 0 -high",
        ConfigFileRelativePath: "game/csgo/cfg/autoexec.cfg",
        ConfigFileContent: string.Join('\n', new[]
        {
            "// re_sound Performance - CS2 autoexec",
            "cl_updaterate 128",
            "cl_interp 0",
            "cl_interp_ratio 1",
            "cl_predict 1",
            "cl_predictweapons 1",
            "cl_lagcompensation 1",
            "fps_max 0",
            "rate 786432",
            "cl_forcepreload 1",
            "host_writeconfig"
        }),
        Settings: new[]
        {
            new RecommendedSetting("cl_updaterate", "128", "Target 128-tick updates (required for FACEIT 128t servers)"),
            new RecommendedSetting("cl_interp_ratio", "1", "Lowest safe interpolation ratio for competitive play"),
            new RecommendedSetting("fps_max", "0", "Uncapped frame rate; trust the monitor + low-latency stack"),
            new RecommendedSetting("rate", "786432", "Max bandwidth allowed on Valve Network (Source2 default)")
        },
        Notes: new[]
        {
            "Append \"+exec autoexec\" to Steam launch options if the game does not autoload your cfg.",
            "Verify `game/csgo/cfg/video.txt` is not read-only; Source2 rewrites it on exit."
        });

    public static RecommendedConfig Valorant { get; } = new(
        LaunchOptions: "(Valorant does not accept custom launch flags via the Riot Client shortcut.)",
        ConfigFileRelativePath: null,
        ConfigFileContent: null,
        Settings: new[]
        {
            new RecommendedSetting("Display Mode", "Fullscreen", "Exclusive fullscreen reduces input latency vs. windowed/borderless"),
            new RecommendedSetting("Material Quality", "Low", "Minimal GPU cost"),
            new RecommendedSetting("Texture Quality", "Low", "Reduces VRAM pressure"),
            new RecommendedSetting("Detail Quality", "Low", "No competitive impact"),
            new RecommendedSetting("UI Quality", "Low", "Frees GPU time for the render queue"),
            new RecommendedSetting("Vignette", "Off", "Removes peripheral darkening"),
            new RecommendedSetting("VSync", "Off", "Always off for competitive play"),
            new RecommendedSetting("Anti-Aliasing", "None", "Clarity over smoothness"),
            new RecommendedSetting("Anisotropic Filtering", "1x", "Cheap texture filtering"),
            new RecommendedSetting("Improve Clarity", "On", "Sharpens target silhouettes"),
            new RecommendedSetting("Bloom", "Off", "Removes muzzle-flash glow"),
            new RecommendedSetting("Distortion", "Off", "Removes Brimstone/Viper smoke distortion"),
            new RecommendedSetting("Cast Shadows", "Off", "Avoids the shadow-read tell")
        },
        Notes: new[]
        {
            "Valorant config lives in %LOCALAPPDATA%\\VALORANT\\Saved\\Config\\<hash>\\Windows\\ and is per-account; re_sound Performance will not overwrite it.",
            "Vanguard runs at boot. System tweaks flagged `BlockedWhenVanguardInstalled` will refuse to apply to prevent VAN:RESTRICTION."
        });

    public static RecommendedConfig Apex { get; } = new(
        LaunchOptions: "+fps_max 0 -novid -forcenovsync",
        ConfigFileRelativePath: null,
        ConfigFileContent: null,
        Settings: new[]
        {
            new RecommendedSetting("setting.cl_gib_allow", "0", "Disables corpse gibs to reduce render cost on kills"),
            new RecommendedSetting("setting.cl_ragdoll_collide", "0", "Removes ragdoll collisions, saves CPU"),
            new RecommendedSetting("setting.mat_letterbox_aspect_goal", "0", "Disables letterboxing (some widescreen setups benefit)"),
            new RecommendedSetting("setting.r_dynamic", "0", "Disables dynamic lights for consistent frame time"),
            new RecommendedSetting("setting.r_lod_switch_scale", "0.5", "Aggressive LOD for FPS gain at distance")
        },
        Notes: new[]
        {
            "videoconfig.txt is at %USERPROFILE%\\Saved Games\\Respawn\\Apex\\local\\videoconfig.txt.",
            "For the EA App: `+` launch options must be set in-app (Game settings > Advanced launch options). Shortcut-based launch options are ignored."
        });

    public static RecommendedConfig For(GameId game) => game switch
    {
        GameId.Cs2 => Cs2,
        GameId.Valorant => Valorant,
        GameId.Apex => Apex,
        _ => throw new ArgumentOutOfRangeException(nameof(game))
    };
}
