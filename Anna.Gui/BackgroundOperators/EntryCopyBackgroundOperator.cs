using Anna.DomainModel;
using Anna.Foundation;
using Anna.Gui.BackgroundOperators.Internals;
using Anna.Gui.Messaging;
using Anna.Service.Interfaces;
using Reactive.Bindings.Extensions;
using System;
using System.Threading;
using System.Threading.Tasks;
using IServiceProvider=Anna.Service.IServiceProvider;

namespace Anna.Gui.BackgroundOperators;

public class EntryCopyBackgroundOperator
    : HasArgDisposableNotificationObject
        <(InteractionMessenger Messenger, Entry[] SourceEntries, string DestPath, IEntriesStats Stats)>
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

    private int _fileCopiedCount;
    private int _fileCount;

    public EntryCopyBackgroundOperator(IServiceProvider dic)
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
        var worker = Dic.GetInstance<ConfirmedFileSystemCopier, (InteractionMessenger, int)>((Arg.Messenger, 0));

        try
        {
            worker.FileCopied += OnFileCopied;
            worker.Invoke(Arg.SourceEntries, Arg.DestPath);
        }
        finally
        {
            worker.FileCopied -= OnFileCopied;
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

