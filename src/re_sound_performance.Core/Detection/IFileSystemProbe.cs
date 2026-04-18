namespace re_sound_performance.Core.Detection;

public interface IFileSystemProbe
{
    bool DirectoryExists(string path);

    bool FileExists(string path);

    IEnumerable<string> EnumerateDirectories(string path, string searchPattern, bool recursive);
}
