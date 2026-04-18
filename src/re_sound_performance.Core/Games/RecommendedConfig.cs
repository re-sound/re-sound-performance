namespace re_sound_performance.Core.Games;

public sealed record RecommendedConfig(
    string LaunchOptions,
    string? ConfigFileRelativePath,
    string? ConfigFileContent,
    IReadOnlyList<RecommendedSetting> Settings,
    IReadOnlyList<string> Notes);

public sealed record RecommendedSetting(string Key, string Value, string Explanation);
