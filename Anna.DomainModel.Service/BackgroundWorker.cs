using Anna.DomainModel.Service.BackgroundProcess;
using Anna.Foundation;
using Anna.Service.Interfaces;
using Anna.Service.Workers;
using Reactive.Bindings.Extensions;
using System.Threading.Channels;
using IDisposable=System.IDisposable;
using IServiceProvider=Anna.Service.IServiceProvider;

namespace Anna.DomainModel.Service;

public sealed class BackgroundWorker : NotificationObject, IBackgroundWorker, IDisposable
{
    #region IsInProcessing

    private bool _IsInProcessing;

    public bool IsInProcessing
    {
        get => _IsInProcessing;
        set => SetProperty(ref _IsInProcessing, value);
    }

    #endregion


    #region Progress

    private double _Progress;

    public double Progress
    {
        get => _Progress;
        set => SetProperty(ref _Progress, value);
    }

    #endregion


    #region Message

    private string _Message = "";

    public string Message
    {
        get => _Message;
        set => SetProperty(ref _Message, value);
    }

    #endregion

    private Channel<IBackgroundProcess> Channel { get; } =
        System.Threading.Channels.Channel.CreateUnbounded<IBackgroundProcess>(
            new UnboundedChannelOptions { SingleReader = true });

    private readonly ManualResetEventSlim _taskCompleted = new();
    private readonly IServiceProvider _dic;

    private int _processCount;

    public BackgroundWorker(IServiceProvider dic)
    {
        _dic = dic;

        Task.Run(ChannelLoop);
    }

    private async Task ChannelLoop()
    {
        while (await Channel.Reader.WaitToReadAsync())
        {
            if (Channel.Reader.TryRead(out var process) == false)
                continue;

            using (process.ObserveProperty(x => x.Progress)
                       .Subscribe(x => Progress = x))
            {
                await process.ExecuteAsync();

                Progress = 100;
            }

            process.Dispose();
            IsInProcessing = Interlocked.Decrement(ref _processCount) > 0;

            Progress = 0;
        }

        _taskCompleted.Set();
    }

    public void Dispose()
    {
        Channel.Writer.TryComplete();
        _taskCompleted.Wait();
        _taskCompleted.Dispose();
    }

    private ValueTask PushProcess(IBackgroundProcess process)
    {
        IsInProcessing = Interlocked.Increment(ref _processCount) > 0;

        return Channel.Writer.WriteAsync(process);
    }

    public ValueTask CopyFileSystemEntryAsync(IFileSystemCopyOperator fileSystemCopyOperator, string destPath, IEnumerable<IEntry> sourceEntries, IEntriesStats stats)
    {
        return
            PushProcess(
                _dic.GetInstance<CopyFileSystemEntryProcess, (IFileSystemCopyOperator, string, IEnumerable<IEntry>, IEntriesStats)>
                    ((fileSystemCopyOperator, destPath, sourceEntries, stats))
            );
    }
}