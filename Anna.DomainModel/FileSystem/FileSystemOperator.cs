using Anna.Service.Interfaces;

namespace Anna.DomainModel.FileSystem;

public abstract class FileSystemOperator : IFileSystemOperator
{
    public event EventHandler? FileCopied;

    public void Copy(IEnumerable<IEntry> sourceEntries, string destPath)
    {
        Parallel.ForEach(sourceEntries,
            entry =>
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

                    var isSame = string.CompareOrdinal(src, dest) == 0;

                    File.Copy(src, dest, true);
                    File.SetAttributes(dest, File.GetAttributes(src));
                    FileCopied?.Invoke(this, EventArgs.Empty);
                }
            });
    }

    private void CopyFolder(DirectoryInfo srcInfo, string dst)
    {
        var src = srcInfo.FullName;

        var targetFolderPath = Path.Combine(dst, Path.GetFileName(src));
        Directory.CreateDirectory(targetFolderPath);
        File.SetAttributes(targetFolderPath, srcInfo.Attributes);

        var di = new DirectoryInfo(src);

        Parallel.ForEach(di.EnumerateFiles(),
            file =>
            {
                var dest = Path.Combine(targetFolderPath, file.Name);

                File.Copy(file.FullName, dest, true);
                File.SetAttributes(dest, file.Attributes);
                FileCopied?.Invoke(this, EventArgs.Empty);
            });

        Parallel.ForEach(di.EnumerateDirectories(),
            dir =>
            {
                CopyFolder(dir, targetFolderPath);
            });
    }
}

public sealed class DefaultFileSystemOperator : FileSystemOperator
{
}