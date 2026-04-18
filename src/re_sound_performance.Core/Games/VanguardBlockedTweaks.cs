using re_sound_performance.Core.Tweaks;

namespace re_sound_performance.Core.Games;

public static class VanguardBlockedTweaks
{
    public static IReadOnlyList<TweakMetadata> ListFor(IEnumerable<ITweak> tweaks)
    {
        ArgumentNullException.ThrowIfNull(tweaks);
        return tweaks
            .Where(t => t.Metadata.BlockedWhenVanguardInstalled)
            .Select(t => t.Metadata)
            .OrderBy(m => m.Category)
            .ThenBy(m => m.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }
}
