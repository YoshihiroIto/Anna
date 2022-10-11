using Anna.Service;

namespace Anna.DomainModel.Service;

public sealed class FileSystemService : IFileSystemService
{
    public bool IsAccessible(string path)
    {
        FileStream? stream = null;

        try
        {
            if (File.Exists(path))
                stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None);
            else
                _ = Directory.EnumerateDirectories(path).FirstOrDefault();

            return true;
        }
        catch
        {
            return false;
        }
        finally
        {
            stream?.Dispose();
        }
    }
    
    public void Copy(string currentPath, string destPath, IEnumerable<string> sourceEntries)
    {
        var targetFolderPath = Path.IsPathRooted(destPath) ? destPath : Path.Combine(currentPath, destPath);
    }
}