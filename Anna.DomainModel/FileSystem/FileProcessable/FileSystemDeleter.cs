using Anna.Constants;
using Anna.Service.Interfaces;
using Anna.Service.Services;
using System.Diagnostics;
using ArgumentOutOfRangeException=System.ArgumentOutOfRangeException;
using IServiceProvider=Anna.Service.IServiceProvider;

namespace Anna.DomainModel.FileSystem.FileProcessable;

public abstract class FileSystemDeleter : IFileProcessable
{
    public event EventHandler? FileProcessed;

    protected CancellationTokenSource? CancellationTokenSource { get; private set; }
    private readonly IServiceProvider _dic;

    private sealed class State : IDisposable
    {
        public readonly ParallelOptions ParallelOptions;

        public bool IsAllDelete
        {
            get => _IsAllDelete != 0;
            set => Interlocked.Exchange(ref _IsAllDelete, value ? 1 : 0);
        }

        private int _IsAllDelete;
        private readonly CancellationTokenSource _cts;

        public State(CancellationTokenSource cts)
        {
            _cts = cts;
            ParallelOptions = new ParallelOptions { CancellationToken = _cts.Token };
        }

        public void Dispose()
        {
            _cts.Dispose();
        }
    }

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
            using var state = new State(CancellationTokenSource);

            // ReSharper disable AccessToDisposedClosure
            Parallel.ForEach(sourceEntries,
                state.ParallelOptions,
                entry =>
                {
                    if (entry.IsFolder)
                        DeleteFolder(new DirectoryInfo(entry.Path), state);
                    else
                        DeleteFile(new FileInfo(entry.Path), state);
                });
            // ReSharper restore AccessToDisposedClosure
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

    private bool DeleteFile(FileInfo file, State state)
    {
        Debug.Assert(CancellationTokenSource is not null);

        var isSkip = false;
        var isReadonly = (file.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly;

        if (isReadonly && state.IsAllDelete == false)
        {
            switch (DeleteActionWhenReadonly(file))
            {
                case ReadOnlyDeleteActions.Delete:
                    // do nothing
                    break;

                case ReadOnlyDeleteActions.AllDelete:
                    state.IsAllDelete = true;
                    break;
                
                case ReadOnlyDeleteActions.Skip:
                    isSkip = true;
                    break;
                    
                case ReadOnlyDeleteActions.Cancel:
                    CancellationTokenSource.Cancel();
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (CancellationTokenSource.IsCancellationRequested)
                return true;
        }

        if (isSkip == false)
        {
            if (isReadonly)
                file.Attributes &= ~FileAttributes.ReadOnly;

            DeleteEntryInternal(file);
            FileProcessed?.Invoke(this, EventArgs.Empty);
        }

        return isSkip;
    }

    private bool DeleteFolder(DirectoryInfo srcInfo, State state)
    {
        Debug.Assert(CancellationTokenSource is not null);

        var isSkipped = 0;

        {
            var (isSkip, _) = CheckDeleteActionWhenReadonly();
            if (CancellationTokenSource.IsCancellationRequested)
                return true;

            if (isSkip)
                return true;
        }

        Parallel.ForEach(srcInfo.EnumerateDirectories(),
            state.ParallelOptions,
            dir =>
            {
                if (DeleteFolder(dir, state))
                    // ReSharper disable once AccessToModifiedClosure
                    Interlocked.Exchange(ref isSkipped, 1);
            });

        Parallel.ForEach(srcInfo.EnumerateFiles(),
            state.ParallelOptions,
            file =>
            {
                if (DeleteFile(file, state))
                    Interlocked.Exchange(ref isSkipped, 1);
            });

        {
            var (isSkip, isReadOnly) = CheckDeleteActionWhenReadonly();
            if (CancellationTokenSource.IsCancellationRequested)
                return true;

            if (isSkipped == 0 & isSkip == false)
            {
                if (isReadOnly)
                    srcInfo.Attributes &= ~FileAttributes.ReadOnly;

                DeleteEntryInternal(srcInfo);
            }
            else
                isSkipped = 1;
        }

        return isSkipped != 0;

        //-----------------------------------------------------------------------------------
        (bool IsSkip, bool IsReadonly) CheckDeleteActionWhenReadonly()
        {
            Debug.Assert(CancellationTokenSource is not null);

            var isSkip = false;
            var isReadonly = (srcInfo.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly;

            if (isReadonly && state.IsAllDelete == false)
            {
                switch (DeleteActionWhenReadonly(srcInfo))
                {
                    case ReadOnlyDeleteActions.Delete:
                        // do nothing
                        break;

                    case ReadOnlyDeleteActions.AllDelete:
                        state.IsAllDelete = true;
                        break;
                    
                    case ReadOnlyDeleteActions.Skip:
                        isSkip = true;
                        break;
                    
                    case ReadOnlyDeleteActions.Cancel:
                        CancellationTokenSource.Cancel();
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return (isSkip, isReadonly);
        }
    }

    private void DeleteEntryInternal(FileSystemInfo di)
    {
        Debug.Assert(CancellationTokenSource is not null);

        for (var isSkip = false; isSkip == false;)
        {
            if (CancellationTokenSource.IsCancellationRequested)
                return;

            try
            {
                di.Delete();
                break;
            }
            catch (IOException)
            {
                switch (DeleteActionWhenAccessFailure(di))
                {
                    case AccessFailureDeleteActions.Skip:
                        isSkip = true;
                        break;

                    case AccessFailureDeleteActions.Cancel:
                        CancellationTokenSource.Cancel();
                        break;

                    case AccessFailureDeleteActions.Retry:
                        // do nothing
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }

    protected abstract ReadOnlyDeleteActions DeleteActionWhenReadonly(FileSystemInfo info);
    protected abstract AccessFailureDeleteActions DeleteActionWhenAccessFailure(FileSystemInfo info);
}

public class DefaultFileSystemDeleter : FileSystemDeleter
{
    public DefaultFileSystemDeleter(IServiceProvider dic)
        : base(dic)
    {
    }
    
    protected override ReadOnlyDeleteActions DeleteActionWhenReadonly(FileSystemInfo info)
    {
        return ReadOnlyDeleteActions.Skip;
    }

    protected override AccessFailureDeleteActions DeleteActionWhenAccessFailure(FileSystemInfo info)
    {
        return AccessFailureDeleteActions.Skip;
    }
}