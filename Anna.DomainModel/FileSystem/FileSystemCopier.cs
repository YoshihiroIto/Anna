using Anna.Constants;
using Anna.Service.Interfaces;
using Anna.Service.Services;
using System.Diagnostics;
using IServiceProvider=Anna.Service.IServiceProvider;

namespace Anna.DomainModel.FileSystem;

public abstract class FileSystemCopier : IFileProcessable
{
    public event EventHandler? FileProcessed;

    protected CancellationTokenSource? CancellationTokenSource { get; private set; }
    private readonly IServiceProvider _dic;

    protected FileSystemCopier(IServiceProvider dic)
    {
        _dic = dic;
    }

    public void Invoke(IEnumerable<IEntry> sourceEntries, string destPath)
    {
        CancellationTokenSource = new CancellationTokenSource();

        try
        {
            var po = new ParallelOptions { CancellationToken = CancellationTokenSource.Token };

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

                        var strategy = SamePathCopyFileActions.Override;
                        var dest = Path.Combine(destPath, Path.GetFileName(src));

                        if (src == dest)
                        {
                            (strategy, dest) = CopyStrategyWhenSamePath(dest);

                            if (CancellationTokenSource is not null &&
                                CancellationTokenSource.IsCancellationRequested)
                                return;
                        }

                        if (strategy == SamePathCopyFileActions.Override)
                            CopyFileInternal(new FileInfo(src), new FileInfo(dest));

                        FileProcessed?.Invoke(this, EventArgs.Empty);
                    }
                });
        }
        catch (OperationCanceledException)
        {
            _dic.GetInstance<ILoggerService>().Information("FileSystemCopier.Invoke() -- Canceled");
        }
        finally
        {
            var d = CancellationTokenSource;
            CancellationTokenSource = null;
            d.Dispose();
        }
    }

    private void CopyFolder(DirectoryInfo srcInfo, string dst, ParallelOptions po)
    {
        Debug.Assert(CancellationTokenSource is not null);

        var src = srcInfo.FullName;

        var targetFolderPath = Path.Combine(dst, Path.GetFileName(src));
        {
            var strategy = SamePathCopyFileActions.Override;

            if (srcInfo.FullName == targetFolderPath)
            {
                (strategy, targetFolderPath) = CopyStrategyWhenSamePath(targetFolderPath);

                if (CancellationTokenSource.IsCancellationRequested)
                    return;
            }

            if (strategy == SamePathCopyFileActions.Skip)
                return;
        }

        Directory.CreateDirectory(targetFolderPath);
        File.SetAttributes(targetFolderPath, srcInfo.Attributes);

        var di = new DirectoryInfo(src);

        Parallel.ForEach(di.EnumerateFiles(),
            po,
            file =>
            {
                Debug.Assert(CancellationTokenSource is not null);

                var strategy = SamePathCopyFileActions.Override;
                var dest = Path.Combine(targetFolderPath, file.Name);

                if (file.FullName == dest)
                {
                    (strategy, dest) = CopyStrategyWhenSamePath(dest);

                    if (CancellationTokenSource.IsCancellationRequested)
                        return;
                }

                if (strategy == SamePathCopyFileActions.Override)
                    CopyFileInternal(file, new FileInfo(dest));

                FileProcessed?.Invoke(this, EventArgs.Empty);
            });

        Parallel.ForEach(di.EnumerateDirectories(),
            po,
            dir => CopyFolder(dir, targetFolderPath, po));
    }

    private void CopyFileInternal(FileInfo srcFile, FileInfo destFile)
    {
        Debug.Assert(CancellationTokenSource is not null);

        var isSkip = false;
        string? destPath = null;

        if (destFile.Exists)
        {
            var result = CopyStrategyWhenExists(srcFile.FullName, destFile.FullName);

            switch (result.Action)
            {
                case ExistsCopyFileActions.Skip:
                    isSkip = true;
                    break;

                case ExistsCopyFileActions.NewerTimestamp:
                    isSkip = srcFile.LastWriteTime < destFile.LastWriteTime;
                    break;

                case ExistsCopyFileActions.Override:
                    // do nothing
                    break;

                case ExistsCopyFileActions.ChangeName:
                    destPath = result.NewDestPath;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        if (isSkip == false)
        {
            if (destPath == null && destFile.Exists)
            {
                if ((destFile.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                    destFile.Attributes &= ~FileAttributes.ReadOnly;
            }

            destPath ??= destFile.FullName;

            File.Copy(srcFile.FullName, destPath, true);
            File.SetAttributes(destPath, srcFile.Attributes);
        }
    }

    protected abstract CopyStrategyWhenExistsResult CopyStrategyWhenExists(string srcPath, string destPath);
    protected abstract CopyStrategyWhenSamePathResult CopyStrategyWhenSamePath(string destPath);

    public record struct CopyStrategyWhenExistsResult(ExistsCopyFileActions Action, string NewDestPath,
        bool IsSameStrategyThereafter);

    public record struct CopyStrategyWhenSamePathResult(SamePathCopyFileActions Action, string NewDestPath);
}

public sealed class DefaultFileSystemCopier : FileSystemCopier
{
    public DefaultFileSystemCopier(IServiceProvider dic)
        : base(dic)
    {
    }

    protected override CopyStrategyWhenExistsResult CopyStrategyWhenExists(string srcPath, string destPath)
    {
        return new CopyStrategyWhenExistsResult(ExistsCopyFileActions.Override, destPath, true);
    }

    protected override CopyStrategyWhenSamePathResult CopyStrategyWhenSamePath(
        string destPath)
    {
        return new CopyStrategyWhenSamePathResult(SamePathCopyFileActions.Skip, destPath);
    }
}