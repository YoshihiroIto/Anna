using Anna.DomainModel.FileSystem.FileProcessable;
using Anna.Foundation;
using Anna.Service.Interfaces;
using Reactive.Bindings.Extensions;
using System;
using System.Threading;
using System.Threading.Tasks;
using IServiceProvider=Anna.Service.IServiceProvider;

namespace Anna.Gui.BackgroundOperators;

public class EntryBackgroundOperator : HasArgDisposableNotificationObject
    <EntryBackgroundOperator, (IEntriesStats Stats, IFileProcessable FileProcessable,
        EntryBackgroundOperator.ProgressModes ProgressMode)>
    , IBackgroundOperator
{
    public string Name => nameof(EntryBackgroundOperator) + ":" + Arg.FileProcessable;
    
    public enum ProgressModes
    {
        Stats,
        Direct,
    }

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
            Arg.FileProcessable.Invoke();
        }
        finally
        {
            Arg.FileProcessable.FileProcessed -= OnFileProcessed;
        }

        return ValueTask.CompletedTask;
    }

    private void OnFileProcessed(object? sender, EventArgs e)
    {
        switch (Arg.ProgressMode)
        {
            case ProgressModes.Stats:
                Interlocked.Increment(ref _fileProcessedCount);
                UpdateProgress();
                break;

            case ProgressModes.Direct:
                var fpe = (FileProcessedDirectEventArgs)e;
                Progress = fpe.Progress;
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void UpdateProgress()
    {
        if (Arg.ProgressMode != ProgressModes.Stats)
            return;
        
        Progress = _fileCount == 0
            ? 0
            : Math.Min(0.999999, (double)_fileProcessedCount / _fileCount);
    }
}