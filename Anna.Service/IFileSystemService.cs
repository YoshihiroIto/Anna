using Anna.Service.Interfaces;

namespace Anna.Service;

public interface IFileSystemService
{
    bool IsAccessible(string path);

    void Copy(IEnumerable<IEntry> sourceEntries, string destPath);
}