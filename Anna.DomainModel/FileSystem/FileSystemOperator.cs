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

                    var isSkip = false;
                    var isCancel = false;
                    var dest = Path.Combine(destPath, Path.GetFileName(src));

                    if (string.CompareOrdinal(src, dest) == 0)
                        (isSkip, isCancel, dest) = CopyStrategyWhenSamePath(dest);

                    if (isCancel)
                        return;

                    if (isSkip == false)
                    {
                        File.Copy(src, dest, true);
                        File.SetAttributes(dest, File.GetAttributes(src));
                    }

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
                var isSkip = false;
                var isCancel = false;
                var dest = Path.Combine(targetFolderPath, file.Name);

                if (string.CompareOrdinal(file.FullName, dest) == 0)
                    (isSkip, isCancel, dest) = CopyStrategyWhenSamePath(dest);

                if (isCancel)
                    return;

                if (isSkip == false)
                {
                    File.Copy(file.FullName, dest, true);
                    File.SetAttributes(dest, file.Attributes);
                }

                FileCopied?.Invoke(this, EventArgs.Empty);
            });

        Parallel.ForEach(di.EnumerateDirectories(),
            dir => CopyFolder(dir, targetFolderPath));
    }

    protected virtual (bool IsSkip, bool IsCancel, string NewDestPath) CopyStrategyWhenSamePath(
        string destPath)
    {
        return (true, false, destPath);
    }
}

public sealed class DefaultFileSystemOperator : FileSystemOperator
{
}