namespace re_sound_performance.Core.Detection;

public sealed class FileSystemProbe : IFileSystemProbe
{
    public bool DirectoryExists(string path) => !string.IsNullOrWhiteSpace(path) && Directory.Exists(path);

    public bool FileExists(string path) => !string.IsNullOrWhiteSpace(path) && File.Exists(path);

    public IEnumerable<string> EnumerateDirectories(string path, string searchPattern, bool recursive)
    {
        if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
        {
            return Array.Empty<string>();
        }

        try
        {
            return Directory.EnumerateDirectories(
                path,
                searchPattern,
                recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
        }
        catch (UnauthorizedAccessException)
        {
            return Array.Empty<string>();
        }
        catch (IOException)
        {
            return Array.Empty<string>();
        }
    }
}
