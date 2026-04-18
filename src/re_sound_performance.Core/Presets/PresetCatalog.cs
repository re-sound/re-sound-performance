namespace re_sound_performance.Core.Presets;

public static class PresetCatalog
{
    private static readonly Preset SafePreset = new(
        PresetKind.Safe,
        "Safe",
        "Risk-free cleanup",
        "Reversible Microsoft-documented tweaks only. No services disabled, no debloat, nothing that could trip an anti-cheat or a compliance scanner.",
        new[]
        {
            "system.disable_xbox_game_bar",
            "system.disable_game_dvr",
            "system.disable_edge_startup_boost",
            "privacy.disable_tips_and_suggestions",
            "input.disable_mouse_acceleration",
            "input.disable_accessibility_shortcuts",
            "privacy.disable_telemetry",
            "privacy.disable_activity_history",
            "privacy.disable_advertising_id",
            "privacy.disable_copilot_recall",
            "privacy.disable_location_tracking",
            "gpu.increase_shader_cache"
        });

    private static readonly Preset BalancedPreset = new(
        PresetKind.Balanced,
        "Balanced",
        "Most recommended",
        "Safe preset plus background service hardening and Appx debloat. Good for a daily driver that also games. Skips anything Valorant/FACEIT-sensitive.",
        new[]
        {
            "system.disable_xbox_game_bar",
            "system.disable_game_dvr",
            "system.disable_mpo",
            "system.multimedia_gaming_profile",
            "system.disable_edge_startup_boost",
            "system.disable_maps_broker",
            "system.disable_legacy_services",
            "system.disable_xbox_services",
            "system.disable_wer_and_sysmain",
            "system.disable_uso_service",
            "input.disable_mouse_acceleration",
            "input.disable_accessibility_shortcuts",
            "network.disable_ndu",
            "power.enable_ultimate_performance",
            "privacy.disable_telemetry",
            "privacy.disable_activity_history",
            "privacy.disable_advertising_id",
            "privacy.disable_copilot_recall",
            "privacy.disable_tips_and_suggestions",
            "privacy.disable_location_tracking",
            "privacy.disable_diagtrack_services",
            "privacy.disable_telemetry_scheduled_tasks",
            "gpu.increase_shader_cache",
            "debloat.remove_teams_consumer",
            "debloat.remove_clipchamp",
            "debloat.remove_bing_apps",
            "debloat.remove_xbox_apps_consumer",
            "debloat.remove_copilot_app",
            "debloat.remove_recall_app",
            "debloat.remove_stock_annoyances"
        });

    private static readonly Preset CompetitivePreset = new(
        PresetKind.Competitive,
        "Competitive",
        "Maximum responsiveness",
        "Balanced preset plus hibernation disabled and full legacy media removal. Requires SSD/NVMe and 16+ GB RAM. Every high-impact tweak your anti-cheat tolerates.",
        new[]
        {
            "system.disable_xbox_game_bar",
            "system.disable_game_dvr",
            "system.disable_mpo",
            "system.multimedia_gaming_profile",
            "system.disable_edge_startup_boost",
            "system.disable_maps_broker",
            "system.disable_legacy_services",
            "system.disable_xbox_services",
            "system.disable_wer_and_sysmain",
            "system.disable_uso_service",
            "input.disable_mouse_acceleration",
            "input.disable_accessibility_shortcuts",
            "network.disable_ndu",
            "power.enable_ultimate_performance",
            "power.disable_hibernation",
            "privacy.disable_telemetry",
            "privacy.disable_activity_history",
            "privacy.disable_advertising_id",
            "privacy.disable_copilot_recall",
            "privacy.disable_tips_and_suggestions",
            "privacy.disable_location_tracking",
            "privacy.disable_diagtrack_services",
            "privacy.disable_telemetry_scheduled_tasks",
            "gpu.increase_shader_cache",
            "debloat.remove_teams_consumer",
            "debloat.remove_clipchamp",
            "debloat.remove_bing_apps",
            "debloat.remove_xbox_apps_consumer",
            "debloat.remove_copilot_app",
            "debloat.remove_recall_app",
            "debloat.remove_stock_annoyances",
            "debloat.remove_legacy_media_apps"
        });

    public static IReadOnlyList<Preset> All { get; } = new[] { SafePreset, BalancedPreset, CompetitivePreset };

    public static Preset Get(PresetKind kind) => kind switch
    {
        PresetKind.Safe => SafePreset,
        PresetKind.Balanced => BalancedPreset,
        PresetKind.Competitive => CompetitivePreset,
        _ => throw new ArgumentOutOfRangeException(nameof(kind))
    };
}
