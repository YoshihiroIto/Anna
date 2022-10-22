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

                        var strategy = SamePathCopyFileStrategies.Override;
                        var dest = Path.Combine(destPath, Path.GetFileName(src));

                        if (src == dest)
                        {
                            (strategy, dest) = CopyStrategyWhenSamePath(dest);

                            if (CancellationTokenSource is not null &&
                                CancellationTokenSource.IsCancellationRequested)
                                return;
                        }

                        if (strategy == SamePathCopyFileStrategies.Override)
                            CopyFileInternal(src, dest);

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
            var strategy = SamePathCopyFileStrategies.Override;

            if (srcInfo.FullName == targetFolderPath)
            {
                (strategy, targetFolderPath) = CopyStrategyWhenSamePath(targetFolderPath);

                if (CancellationTokenSource.IsCancellationRequested)
                    return;
            }

            if (strategy == SamePathCopyFileStrategies.Skip)
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

                var strategy = SamePathCopyFileStrategies.Override;
                var dest = Path.Combine(targetFolderPath, file.Name);

                if (file.FullName == dest)
                {
                    (strategy, dest) = CopyStrategyWhenSamePath(dest);

                    if (CancellationTokenSource.IsCancellationRequested)
                        return;
                }
                
                if (strategy == SamePathCopyFileStrategies.Override)
                    CopyFileInternal(file.FullName, dest, file.Attributes);

                FileProcessed?.Invoke(this, EventArgs.Empty);
            });

        Parallel.ForEach(di.EnumerateDirectories(),
            po,
            dir => CopyFolder(dir, targetFolderPath, po));
    }

    private void CopyFileInternal(string srcPath, string destPath)
    {
        CopyFileInternal(srcPath, destPath, File.GetAttributes(srcPath));
    }

    private void CopyFileInternal(string srcPath, string destPath, FileAttributes srcAttr)
    {
        if (File.Exists(destPath))
        {
            var result = CopyStrategyWhenExists(destPath);

            throw new NotImplementedException();
        }

        File.Copy(srcPath, destPath, true);
        File.SetAttributes(destPath, srcAttr);
    }

    protected abstract (ExistsCopyFileStrategies strategy, string NewDestPath) CopyStrategyWhenExists(
        string destPath);
    protected abstract (SamePathCopyFileStrategies strategy, string NewDestPath) CopyStrategyWhenSamePath(
        string destPath);
}

public sealed class DefaultFileSystemCopier : FileSystemCopier
{
    public DefaultFileSystemCopier(IServiceProvider dic)
        : base(dic)
    {
    }

    protected override (ExistsCopyFileStrategies strategy, string NewDestPath) CopyStrategyWhenExists(
        string destPath)
    {
        return (ExistsCopyFileStrategies.Override, destPath);
    }

    protected override (SamePathCopyFileStrategies strategy, string NewDestPath) CopyStrategyWhenSamePath(
        string destPath)
    {
        return (SamePathCopyFileStrategies.Skip, destPath);
    }
}