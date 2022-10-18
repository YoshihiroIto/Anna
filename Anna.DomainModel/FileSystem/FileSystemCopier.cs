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

                        var isSkip = false;
                        var dest = Path.Combine(destPath, Path.GetFileName(src));

                        if (src == dest)
                        {
                            (isSkip, dest) = CopyStrategyWhenSamePath(dest);

                            if (CancellationTokenSource is not null &&
                                CancellationTokenSource.IsCancellationRequested)
                                return;
                        }

                        if (isSkip == false)
                        {
                            File.Copy(src, dest, true);
                            File.SetAttributes(dest, File.GetAttributes(src));
                        }

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
            var isSkip = false;

            if (srcInfo.FullName == targetFolderPath)
            {
                (isSkip, targetFolderPath) = CopyStrategyWhenSamePath(targetFolderPath);

                if (CancellationTokenSource.IsCancellationRequested)
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
                Debug.Assert(CancellationTokenSource is not null);

                var isSkip = false;
                var dest = Path.Combine(targetFolderPath, file.Name);

                if (file.FullName == dest)
                {
                    (isSkip, dest) = CopyStrategyWhenSamePath(dest);

                    if (CancellationTokenSource.IsCancellationRequested)
                        return;
                }

                if (isSkip == false)
                {
                    File.Copy(file.FullName, dest, true);
                    File.SetAttributes(dest, file.Attributes);
                }

                FileProcessed?.Invoke(this, EventArgs.Empty);
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

public sealed class DefaultFileSystemCopier : FileSystemCopier
{
    public DefaultFileSystemCopier(IServiceProvider dic)
        : base(dic)
    {
    }
}