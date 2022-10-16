using Anna.Service;
using Anna.Service.Interfaces;
using System.Diagnostics;
using IServiceProvider=Anna.Service.IServiceProvider;

namespace Anna.DomainModel.FileSystem;

public abstract class FileSystemCopyOperator : IFileSystemCopyOperator
{
    public event EventHandler? FileCopied;

    protected CancellationTokenSource? CopyCancellationTokenSource { get; private set; }
    private readonly IServiceProvider _dic;

    protected FileSystemCopyOperator(IServiceProvider dic)
    {
        _dic = dic;
    }

    public void Invoke(IEnumerable<IEntry> sourceEntries, string destPath)
    {
        CopyCancellationTokenSource = new CancellationTokenSource();

        try
        {
            var po = new ParallelOptions { CancellationToken = CopyCancellationTokenSource.Token };

            Parallel.ForEach(sourceEntries,
                po,
                entry =>
                {
                    var src = entry.Path;

                    if (entry.IsFolder)
                    {
                        CopyFolder(new DirectoryInfo(src), destPath, po);
                    }
                    else
                    {
                        Directory.CreateDirectory(destPath);

                        var isSkip = false;
                        var dest = Path.Combine(destPath, Path.GetFileName(src));

                        if (string.CompareOrdinal(src, dest) == 0)
                        {
                            (isSkip, dest) = CopyStrategyWhenSamePath(dest);

                            if (CopyCancellationTokenSource is not null &&
                                CopyCancellationTokenSource.IsCancellationRequested)
                                return;
                        }

                        if (isSkip == false)
                        {
                            File.Copy(src, dest, true);
                            File.SetAttributes(dest, File.GetAttributes(src));
                        }

                        FileCopied?.Invoke(this, EventArgs.Empty);
                    }
                });
        }
        catch (OperationCanceledException)
        {
            _dic.GetInstance<ILoggerService>().Information("FileSystemOperator.Copy() -- Canceled");
        }
        finally
        {
            var d = CopyCancellationTokenSource;
            CopyCancellationTokenSource = null;
            d.Dispose();
        }
    }

    private void CopyFolder(DirectoryInfo srcInfo, string dst, ParallelOptions po)
    {
        Debug.Assert(CopyCancellationTokenSource is not null);

        var src = srcInfo.FullName;

        var targetFolderPath = Path.Combine(dst, Path.GetFileName(src));
        {
            var isSkip = false;

            if (string.CompareOrdinal(srcInfo.FullName, targetFolderPath) == 0)
            {
                (isSkip, targetFolderPath) = CopyStrategyWhenSamePath(targetFolderPath);

                if (CopyCancellationTokenSource.IsCancellationRequested)
                    return;
            }

            if (isSkip)
                return;
        }

        Directory.CreateDirectory(targetFolderPath);
        File.SetAttributes(targetFolderPath, srcInfo.Attributes);

        var di = new DirectoryInfo(src);

        Parallel.ForEach(di.EnumerateFiles(),
            po,
            file =>
            {
                Debug.Assert(CopyCancellationTokenSource is not null);

                var isSkip = false;
                var dest = Path.Combine(targetFolderPath, file.Name);

                if (string.CompareOrdinal(file.FullName, dest) == 0)
                {
                    (isSkip, dest) = CopyStrategyWhenSamePath(dest);

                    if (CopyCancellationTokenSource.IsCancellationRequested)
                        return;
                }

                if (isSkip == false)
                {
                    File.Copy(file.FullName, dest, true);
                    File.SetAttributes(dest, file.Attributes);
                }

                FileCopied?.Invoke(this, EventArgs.Empty);
            });

        Parallel.ForEach(di.EnumerateDirectories(),
            po,
            dir => CopyFolder(dir, targetFolderPath, po));
    }

    protected virtual (bool IsSkip, string NewDestPath) CopyStrategyWhenSamePath(string destPath)
    {
        return (true, destPath);
    }
}

public sealed class DefaultFileSystemCopyOperator : FileSystemCopyOperator
{
    public DefaultFileSystemCopyOperator(IServiceProvider dic)
        : base(dic)
    {
    }
}