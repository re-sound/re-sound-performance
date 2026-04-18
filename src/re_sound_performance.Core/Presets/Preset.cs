namespace re_sound_performance.Core.Presets;

public sealed record Preset(
    PresetKind Kind,
    string Name,
    string Tagline,
    string Description,
    IReadOnlyList<string> TweakIds);
