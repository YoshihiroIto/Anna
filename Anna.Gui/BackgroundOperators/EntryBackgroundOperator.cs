using Anna.DomainModel.FileSystem.FileProcessable;
using Anna.Foundation;
using Anna.Service.Interfaces;
using Reactive.Bindings.Extensions;
using System;
using System.Threading;
using System.Threading.Tasks;
using IServiceProvider=Anna.Service.IServiceProvider;

namespace Anna.Gui.BackgroundOperators;

public class EntryBackgroundOperator
    : HasArgDisposableNotificationObject
        <(IEntriesStats Stats, IFileProcessable FileProcessable, Action FileOperationPrim)>
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

    private int _fileProcessedCount;
    private int _fileCount;

    public EntryBackgroundOperator(IServiceProvider dic)
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
            Arg.FileProcessable.FileProcessed += OnFileProcessed;
            Arg.FileOperationPrim.Invoke();
        }
        finally
        {
            Arg.FileProcessable.FileProcessed -= OnFileProcessed;
        }

        return ValueTask.CompletedTask;
    }

    private void OnFileProcessed(object? sender, EventArgs e)
    {
        Interlocked.Increment(ref _fileProcessedCount);
        UpdateProgress();
    }

    private void UpdateProgress()
    {
        Progress = _fileCount == 0
            ? 0
            : Math.Min(0.999999, (double)_fileProcessedCount / _fileCount);
    }
}