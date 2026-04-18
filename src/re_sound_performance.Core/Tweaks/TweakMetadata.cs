namespace re_sound_performance.Core.Tweaks;

public sealed record TweakMetadata(
    string Id,
    string Name,
    string ShortDescription,
    string DetailedDescription,
    string Modifies,
    string ExpectedImpact,
    TweakCategory Category,
    TweakRisk Risk,
    TweakEvidenceLevel Evidence,
    IReadOnlyList<string> Sources,
    IReadOnlyList<string> IncompatibleWith,
    bool RequiresRestart,
    bool BlockedWhenVanguardInstalled,
    bool BlockedWhenFaceitInstalled);
