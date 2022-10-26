using Anna.Constants;
using Anna.Service.Interfaces;
using Anna.Service.Services;
using System.Diagnostics;
using IServiceProvider=Anna.Service.IServiceProvider;

namespace Anna.DomainModel.FileSystem.FileProcessable;

public abstract class FileSystemCopier : IFileProcessable
{
    public event EventHandler? FileProcessed;

    protected CancellationTokenSource? CancellationTokenSource { get; private set; }
    private readonly IServiceProvider _dic;

    protected CopyOrMove CopyOrMove { get; init; } = CopyOrMove.Unset;

    private sealed class State : IDisposable
    {
        public readonly ParallelOptions ParallelOptions;
        private readonly CancellationTokenSource _cts;

        public CopyActionWhenExistsResult CopyActionWhenExistsResult =
            new(ExistsCopyFileActions.Skip, "", false, true);

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

    protected FileSystemCopier(IServiceProvider dic)
    {
        _dic = dic;
    }

    public void Invoke(IEnumerable<IEntry> sourceEntries, string destPath)
    {
        CancellationTokenSource = new CancellationTokenSource();

        try
        {
            switch (CopyOrMove)
            {
                case CopyOrMove.Copy:
                    Copy(sourceEntries, destPath);
                    break;

                case CopyOrMove.Move:
                    Move(sourceEntries, destPath);
                    break;

                case CopyOrMove.Unset:
                default:
                    throw new InvalidOperationException();
            }
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

    private void Copy(IEnumerable<IEntry> sourceEntries, string destPath)
    {
        Debug.Assert(CancellationTokenSource is not null);

        using var state = new State(CancellationTokenSource);

        Parallel.ForEach(sourceEntries,
            state.ParallelOptions,
            entry =>
            {
                var src = entry.Path;

                if (entry.IsFolder)
                {
                    // ReSharper disable once AccessToDisposedClosure
                    CopyFolder(new DirectoryInfo(src), destPath, state);
                }
                else
                {
                    Directory.CreateDirectory(destPath);

                    var action = SamePathCopyFileActions.Override;
                    var dest = Path.Combine(destPath, Path.GetFileName(src));

                    if (src == dest)
                    {
                        (action, dest) = CopyActionWhenSamePath(dest);

                        if (CancellationTokenSource is not null &&
                            CancellationTokenSource.IsCancellationRequested)
                            return;
                    }

                    if (action == SamePathCopyFileActions.Override)
                        // ReSharper disable once AccessToDisposedClosure
                        CopyFileInternal(new FileInfo(src), new FileInfo(dest), state);

                    FileProcessed?.Invoke(this, EventArgs.Empty);
                }
            });
    }

    private static void Move(IEnumerable<IEntry> sourceEntries, string destPath)
    {
        foreach (var entry in sourceEntries)
        {
            var src = entry.Path;

            var dest = Path.Combine(destPath, Path.GetFileName(src));
            Directory.CreateDirectory(destPath);

            if (entry.IsFolder)
                Directory.Move(src, dest);
            else
                File.Move(src, dest);
        }
    }

    private void CopyFolder(DirectoryInfo srcInfo, string dst, State state)
    {
        Debug.Assert(CancellationTokenSource is not null);

        var src = srcInfo.FullName;

        var targetFolderPath = Path.Combine(dst, Path.GetFileName(src));
        {
            var action = SamePathCopyFileActions.Override;

            if (srcInfo.FullName == targetFolderPath)
            {
                (action, targetFolderPath) = CopyActionWhenSamePath(targetFolderPath);

                if (CancellationTokenSource.IsCancellationRequested)
                    return;
            }

            if (action == SamePathCopyFileActions.Skip)
                return;
        }

        Directory.CreateDirectory(targetFolderPath);
        File.SetAttributes(targetFolderPath, srcInfo.Attributes);

        var di = new DirectoryInfo(src);

        Parallel.ForEach(di.EnumerateFiles(),
            state.ParallelOptions,
            file =>
            {
                Debug.Assert(CancellationTokenSource is not null);

                var action = SamePathCopyFileActions.Override;
                var dest = Path.Combine(targetFolderPath, file.Name);

                if (file.FullName == dest)
                {
                    (action, dest) = CopyActionWhenSamePath(dest);

                    if (CancellationTokenSource.IsCancellationRequested)
                        return;
                }

                if (action == SamePathCopyFileActions.Override)
                    CopyFileInternal(file, new FileInfo(dest), state);

                FileProcessed?.Invoke(this, EventArgs.Empty);
            });

        Parallel.ForEach(di.EnumerateDirectories(),
            state.ParallelOptions,
            dir => CopyFolder(dir, targetFolderPath, state));
    }

    private void CopyFileInternal(FileInfo srcFile, FileInfo destFile, State state)
    {
        Debug.Assert(CancellationTokenSource is not null);

        var isSkip = false;
        string? destPath = null;

        if (destFile.Exists)
        {
            CopyActionWhenExists(srcFile.FullName, destFile.FullName, ref state.CopyActionWhenExistsResult);

            switch (state.CopyActionWhenExistsResult.Action)
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

                case ExistsCopyFileActions.Rename:
                    destPath = state.CopyActionWhenExistsResult.NewDestPath;
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

    protected abstract void
        CopyActionWhenExists(string srcPath, string destPath, ref CopyActionWhenExistsResult result);

    protected abstract CopyActionWhenSamePathResult CopyActionWhenSamePath(string destPath);

    public record struct CopyActionWhenExistsResult(ExistsCopyFileActions Action, string NewDestPath,
        bool IsSameActionThereafter, bool IsFirst);

    public record struct CopyActionWhenSamePathResult(SamePathCopyFileActions Action, string NewDestPath);
}

public sealed class DefaultFileSystemCopier : FileSystemCopier
{
    public DefaultFileSystemCopier(IServiceProvider dic)
        : base(dic)
    {
    }

    // ReSharper disable once RedundantAssignment
    protected override void CopyActionWhenExists(string srcPath, string destPath, ref CopyActionWhenExistsResult result)
    {
        result = new CopyActionWhenExistsResult(ExistsCopyFileActions.Override, destPath, true, true);
    }

    protected override CopyActionWhenSamePathResult CopyActionWhenSamePath(string destPath)
    {
        return new CopyActionWhenSamePathResult(SamePathCopyFileActions.Skip, destPath);
    }
}