using Anna.Service.Interfaces;

namespace Anna.DomainModel.FileSystem;

public abstract class FileSystemOperator : IFileSystemOperator
{
    public event EventHandler? FileCopied;

    public Task CopyAsync(IEnumerable<IEntry> sourceEntries, string destPath)
    {
        return Parallel.ForEachAsync(sourceEntries,
            async (entry, _) =>
            {
                var src = entry.Path;

                if (entry.IsFolder)
                {
                    var srcInfo = new DirectoryInfo(src);
                    await CopyFolderAsync(srcInfo, destPath);
                }
                else
                {
                    Directory.CreateDirectory(destPath);

                    var isSkip = false;
                    var dest = Path.Combine(destPath, Path.GetFileName(src));

                    if (string.CompareOrdinal(src, dest) == 0)
                        (isSkip, dest) = await CopyStrategyWhenSamePathAsync(dest);

                    if (isSkip == false)
                    {
                        File.Copy(src, dest, true);
                        File.SetAttributes(dest, File.GetAttributes(src));
                    }

                    FileCopied?.Invoke(this, EventArgs.Empty);
                }
            });
    }

    private async ValueTask CopyFolderAsync(DirectoryInfo srcInfo, string dst)
    {
        var src = srcInfo.FullName;

        var targetFolderPath = Path.Combine(dst, Path.GetFileName(src));
        Directory.CreateDirectory(targetFolderPath);
        File.SetAttributes(targetFolderPath, srcInfo.Attributes);

        var di = new DirectoryInfo(src);

        await Parallel.ForEachAsync(di.EnumerateFiles(),
            async (file, _) =>
            {
                var isSkip = false;
                var dest = Path.Combine(targetFolderPath, file.Name);

                if (string.CompareOrdinal(file.FullName, dest) == 0)
                    (isSkip, dest) = await CopyStrategyWhenSamePathAsync(dest);

                if (isSkip == false)
                {
                    File.Copy(file.FullName, dest, true);
                    File.SetAttributes(dest, file.Attributes);
                }

                FileCopied?.Invoke(this, EventArgs.Empty);
            });

        await Parallel.ForEachAsync(di.EnumerateDirectories(),
             (dir, _) => CopyFolderAsync(dir, targetFolderPath));
    }

    protected virtual ValueTask<(bool IsSkip, string NewDestPath)> CopyStrategyWhenSamePathAsync(string destPath)
    {
        return ValueTask.FromResult((true, destPath));
    }
}

public sealed class DefaultFileSystemOperator : FileSystemOperator
{
}