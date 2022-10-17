using Anna.Constants;
using Anna.DomainModel;
using Anna.DomainModel.FileSystem;
using Anna.Foundation;
using Anna.Service.Interfaces;
using Reactive.Bindings.Extensions;
using System;
using System.Threading;
using System.Threading.Tasks;
using IServiceProvider=Anna.Service.IServiceProvider;

namespace Anna.Gui.BackgroundOperators;

public class EntryDeleteBackgroundOperator
    : HasArgDisposableNotificationObject
        <(Entry[] SourceEntries, EntryDeleteModes Mode, IEntriesStats Stats)>
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

    private int _fileDeletedCount;
    private int _fileCount;

    public EntryDeleteBackgroundOperator(IServiceProvider dic)
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
        var worker = Dic.GetInstance<DefaultFileSystemDeleter>();

        try
        {
            worker.FileDeleted += OnFileDeleted;
            worker.Invoke(Arg.SourceEntries, Arg.Mode);
        }
        finally
        {
            worker.FileDeleted -= OnFileDeleted;
        }

        return ValueTask.CompletedTask;
    }

    private void OnFileDeleted(object? sender, EventArgs e)
    {
        Interlocked.Increment(ref _fileDeletedCount);
        UpdateProgress();
    }

    private void UpdateProgress()
    {
        Progress = Math.Min(0.999999, (double)_fileDeletedCount / _fileCount) * 100;
    }
}