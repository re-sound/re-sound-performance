namespace re_sound_performance.Core.Tweaks;

public sealed record TweakResult(
    string TweakId,
    bool Success,
    string? Message,
    TweakStatus ResultingStatus,
    IReadOnlyList<string> BackupReferences,
    DateTimeOffset CompletedAt)
{
    public static TweakResult Ok(string id, TweakStatus status, IReadOnlyList<string>? backupReferences = null, string? message = null) =>
        new(id, true, message, status, backupReferences ?? Array.Empty<string>(), DateTimeOffset.UtcNow);

    public static TweakResult Fail(string id, string message, TweakStatus status = TweakStatus.Unknown) =>
        new(id, false, message, status, Array.Empty<string>(), DateTimeOffset.UtcNow);
}
