using Anna.Foundation;
using Anna.Service.Interfaces;
using Anna.Service.Workers;
using Reactive.Bindings.Extensions;
using System.Threading.Channels;
using IDisposable=System.IDisposable;

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

    private Channel<IBackgroundOperator> Channel { get; } =
        System.Threading.Channels.Channel.CreateUnbounded<IBackgroundOperator>(
            new UnboundedChannelOptions { SingleReader = true });

    private readonly ManualResetEventSlim _taskCompleted = new();

    private int _operatorCount;

    public BackgroundWorker()
    {
        Task.Run(ChannelLoop);
    }

    private async Task ChannelLoop()
    {
        while (await Channel.Reader.WaitToReadAsync())
        {
            if (Channel.Reader.TryRead(out var @operator) == false)
                continue;

            using (@operator.ObserveProperty(x => x.Progress)
                       .Subscribe(x => Progress = x))
            {
                await @operator.ExecuteAsync();

                Progress = 100;
            }

            @operator.Dispose();
            IsInProcessing = Interlocked.Decrement(ref _operatorCount) > 0;

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

    public ValueTask PushOperator(IBackgroundOperator @operator)
    {
        IsInProcessing = Interlocked.Increment(ref _operatorCount) > 0;

        return Channel.Writer.WriteAsync(@operator);
    }
}