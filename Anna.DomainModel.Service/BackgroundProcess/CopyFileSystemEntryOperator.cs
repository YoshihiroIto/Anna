using Anna.Foundation;
using Anna.Service.Interfaces;
using Reactive.Bindings.Extensions;
using IServiceProvider=Anna.Service.IServiceProvider;

namespace Anna.DomainModel.Service.BackgroundProcess;

internal sealed class CopyFileSystemEntryOperator
    : HasArgDisposableNotificationObject<(IFileSystemCopyOperator FileSystemOperator, string DestPath, IEnumerable<IEntry>
            SourceEntries, IEntriesStats Stats)>
        , IBackgroundOperator
{
    #region Progress

    private double _Progress;

    public double Progress
    {
        get => _Progress;
        set => SetProperty(ref _Progress, value);
    }

    #endregion

    #region Message

    private string _Message = "CopyFileSystemEntryProcess";

    public string Message
    {
        get => _Message;
        set => SetProperty(ref _Message, value);
    }

    #endregion

    private int _fileCopiedCount;
    private int _fileCount;

    public CopyFileSystemEntryOperator(IServiceProvider dic)
        : base(dic)
    {
        if (Arg.Stats is IDisposable disposable)
            Trash.Add(disposable);

        _fileCount = Arg.Stats.FileCount;

        Arg.Stats.ObserveProperty(x => x.FileCount)
            .Subscribe(x =>
            {
                _fileCount = x;
                UpdateProgress();
            }).AddTo(Trash);
    }

    public ValueTask ExecuteAsync()
    {
        try
        {
            Arg.FileSystemOperator.FileCopied += OnFileCopied;
            Arg.FileSystemOperator.Invoke(Arg.SourceEntries, Arg.DestPath);
        }
        finally
        {
            Arg.FileSystemOperator.FileCopied -= OnFileCopied;
        }

        return ValueTask.CompletedTask;
    }

    private void OnFileCopied(object? sender, EventArgs e)
    {
        Interlocked.Increment(ref _fileCopiedCount);
        UpdateProgress();
    }

    private void UpdateProgress()
    {
        Progress = Math.Min(0.999999, (double)_fileCopiedCount / _fileCount) * 100;
    }
}