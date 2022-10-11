namespace Anna.Service;

public interface IFileSystemService
{
    bool IsAccessible(string path);
    
    void Copy(string currentPath, string destPath, IEnumerable<string> sourceEntries);
}