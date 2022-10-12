using Anna.Service;
using Anna.Service.Interfaces;

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

    public void Copy(string destPath, IEnumerable<IEntry> sourceEntries)
    {
        foreach (var entry in sourceEntries)
        {
            var src = entry.Path;

            if (entry.IsFolder)
            {
                var srcInfo = new DirectoryInfo(src);
                CopyFolder(srcInfo, destPath);
            }
            else
            {
                Directory.CreateDirectory(destPath);

                var dest = Path.Combine(destPath, Path.GetFileName(src));

                File.Copy(src, dest, true);
                File.SetAttributes(dest, File.GetAttributes(src));
            }
        }
    }

    private static void CopyFolder(DirectoryInfo srcInfo, string dst)
    {
        var src = srcInfo.FullName;
        
        var targetFolderPath = Path.Combine(dst, Path.GetFileName(src));
        Directory.CreateDirectory(targetFolderPath);
        File.SetAttributes(targetFolderPath, srcInfo.Attributes);
        
        var di = new DirectoryInfo(src);
        foreach (var file in di.EnumerateFiles())
        {
            var dest = Path.Combine(targetFolderPath, file.Name);

            File.Copy(file.FullName, dest, true);
            File.SetAttributes(dest, file.Attributes);
        }

        foreach (var dir in di.EnumerateDirectories())
            CopyFolder(dir, targetFolderPath);
    }
}