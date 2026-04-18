using re_sound_performance.Core.Tweaks;

namespace re_sound_performance.Core.Detection;

public sealed record TweakGateDecision(bool Allowed, string? Reason);

public static class TweakGate
{
    public static TweakGateDecision Evaluate(TweakMetadata metadata, AntiCheatInfo? antiCheat)
    {
        if (antiCheat is null)
        {
            return new TweakGateDecision(true, null);
        }

        if (metadata.BlockedWhenVanguardInstalled && antiCheat.Has(AntiCheat.Vanguard))
        {
            return new TweakGateDecision(false,
                "Blocked: Riot Vanguard is installed. Applying this tweak can trigger VAN:RESTRICTION and refuse to launch Valorant.");
        }

        if (metadata.BlockedWhenFaceitInstalled && antiCheat.Has(AntiCheat.FaceitAc))
        {
            return new TweakGateDecision(false,
                "Blocked: FACEIT AC is installed. Applying this tweak can flag you as tampering and refuse match access.");
        }

        return new TweakGateDecision(true, null);
    }
}
