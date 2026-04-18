using re_sound_performance.Core.Detection;

namespace re_sound_performance.Tests.Detection;

internal sealed class InMemoryFileSystemProbe : IFileSystemProbe
{
    private readonly HashSet<string> _directories = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _files = new(StringComparer.OrdinalIgnoreCase);

    public void AddDirectory(string path) => _directories.Add(path);

    public void AddFile(string path) => _files.Add(path);

    public bool DirectoryExists(string path) => _directories.Contains(path);

    public bool FileExists(string path) => _files.Contains(path);

    public IEnumerable<string> EnumerateDirectories(string path, string searchPattern, bool recursive)
    {
        if (!_directories.Contains(path))
        {
            return Array.Empty<string>();
        }

        var prefix = path.TrimEnd('\\', '/') + "\\";
        var regex = WildcardToRegex(searchPattern);
        return _directories
            .Where(d => d.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .Where(d => recursive || CountPathSeparators(d, prefix) == 0)
            .Where(d => regex.IsMatch(GetLeaf(d)))
            .ToArray();
    }

    private static System.Text.RegularExpressions.Regex WildcardToRegex(string pattern)
    {
        var escaped = System.Text.RegularExpressions.Regex.Escape(pattern)
            .Replace("\\*", ".*", StringComparison.Ordinal)
            .Replace("\\?", ".", StringComparison.Ordinal);
        return new System.Text.RegularExpressions.Regex("^" + escaped + "$", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    }

    private static int CountPathSeparators(string directory, string prefix)
    {
        var tail = directory[prefix.Length..];
        return tail.Count(c => c == '\\' || c == '/');
    }

    private static string GetLeaf(string directory)
    {
        var trimmed = directory.TrimEnd('\\', '/');
        var idx = trimmed.LastIndexOfAny(new[] { '\\', '/' });
        return idx < 0 ? trimmed : trimmed[(idx + 1)..];
    }
}
