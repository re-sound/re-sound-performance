namespace re_sound_performance.Core.Presets;

public sealed record PresetProgress(
    int Completed,
    int Total,
    string CurrentTweakId,
    string CurrentTweakName,
    PresetStepOutcome Outcome,
    string? Message);
