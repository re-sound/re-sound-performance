namespace re_sound_performance.Core.Presets;

public sealed record PresetRunSummary(
    PresetKind Kind,
    int Applied,
    int Skipped,
    int Blocked,
    int Failed,
    int Unavailable,
    bool Cancelled,
    IReadOnlyList<PresetProgress> Steps);
