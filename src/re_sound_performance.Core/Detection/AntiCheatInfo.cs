namespace re_sound_performance.Core.Detection;

public sealed record AntiCheatInfo(AntiCheat Installed, IReadOnlyList<string> Details)
{
    public bool HasAny => Installed != AntiCheat.None;

    public bool Has(AntiCheat flag) => (Installed & flag) == flag;

    public static AntiCheatInfo None { get; } = new(AntiCheat.None, Array.Empty<string>());
}
