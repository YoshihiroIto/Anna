using Anna.Constants;
using Anna.Service.Interfaces;
using Anna.Service.Services;
using System.Diagnostics;
using IServiceProvider=Anna.Service.IServiceProvider;

namespace Anna.DomainModel.FileSystem;

public abstract class FileSystemDeleter : IFileProcessable
{
    public event EventHandler? FileProcessed;

    protected CancellationTokenSource? CancellationTokenSource { get; private set; }
    private readonly IServiceProvider _dic;

    protected FileSystemDeleter(IServiceProvider dic)
    {
        _dic = dic;
    }

    public void Invoke(IEnumerable<IEntry> sourceEntries, EntryDeleteModes mode)
    {
        CancellationTokenSource = new CancellationTokenSource();

        if (mode == EntryDeleteModes.TrashCan)
            throw new NotImplementedException();

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
                        DeleteFolder(new DirectoryInfo(src), po);
                    }
                    else
                    {
                        var file = new FileInfo(entry.Path);

                        DeleteFile(file);
                    }
                });
        }
        catch (OperationCanceledException)
        {
            _dic.GetInstance<ILoggerService>().Information("FileSystemDeleter.Invoke() -- Canceled");
        }
        finally
        {
            var d = CancellationTokenSource;
            CancellationTokenSource = null;
            d.Dispose();
        }
    }

    private bool DeleteFile(FileInfo file)
    {
        Debug.Assert(CancellationTokenSource is not null);

        var isSkip = false;
        var isReadonly = (file.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly;

        if (isReadonly)
        {
            isSkip = DeleteStrategyWhenReadonly(file) == DeleteStrategies.Skip;

            if (CancellationTokenSource.IsCancellationRequested)
                return true;
        }

        if (isSkip == false)
        {
            if (isReadonly)
                file.Attributes &= ~FileAttributes.ReadOnly;

            file.Delete();
            FileProcessed?.Invoke(this, EventArgs.Empty);
        }

        return isSkip;
    }

    private bool DeleteFolder(DirectoryInfo srcInfo, ParallelOptions po)
    {
        Debug.Assert(CancellationTokenSource is not null);

        var isSkipped = 0;

        {
            var isSkip = false;
            var isReadonly = (srcInfo.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly;

            if (isReadonly)
            {
                isSkip = DeleteStrategyWhenReadonly(srcInfo) == DeleteStrategies.Skip;

                if (CancellationTokenSource.IsCancellationRequested)
                    return true;
            }

            if (isSkip)
                return true;
        }

        Parallel.ForEach(srcInfo.EnumerateDirectories(),
            po,
            dir =>
            {
                if (DeleteFolder(dir, po))
                    // ReSharper disable once AccessToModifiedClosure
                    Interlocked.Exchange(ref isSkipped, 1);
            });

        Parallel.ForEach(srcInfo.EnumerateFiles(),
            po,
            file =>
            {
                if (DeleteFile(file))
                    Interlocked.Exchange(ref isSkipped, 1);
            });

        {
            var isSkip = false;
            var isReadonly = (srcInfo.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly;

            if (isReadonly)
            {
                isSkip = DeleteStrategyWhenReadonly(srcInfo) == DeleteStrategies.Skip;

                if (CancellationTokenSource.IsCancellationRequested)
                    return true;
            }

            if (isSkipped == 0 & isSkip == false)
            {
                if (isReadonly)
                    srcInfo.Attributes &= ~FileAttributes.ReadOnly;

                srcInfo.Delete();
            }
            else
                isSkipped = 1;
        }

        return isSkipped != 0;
    }

    protected virtual DeleteStrategies DeleteStrategyWhenReadonly(FileSystemInfo info)
    {
        return DeleteStrategies.Skip;
    }
}

public class DefaultFileSystemDeleter : FileSystemDeleter
{
    public DefaultFileSystemDeleter(IServiceProvider dic)
        : base(dic)
    {
    }
}