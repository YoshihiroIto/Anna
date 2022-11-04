using Anna.Foundation;
using Anna.Service.Interfaces;
using Anna.Service.Services;
using Anna.Service.Workers;
using Reactive.Bindings.Extensions;
using System.Threading.Channels;
using IServiceProvider=Anna.Service.IServiceProvider;

namespace Anna.DomainModel.Service;

public sealed class BackgroundWorker : DisposableNotificationObject, IBackgroundWorker
{
    public event EventHandler<ExceptionThrownEventArgs>? ExceptionThrown;

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

    #region ExecutingOperator

    private string _ExecutingOperator = "";

    public string ExecutingOperator
    {
        get => _ExecutingOperator;
        set => SetProperty(ref _ExecutingOperator, value);
    }

    #endregion

    private readonly Channel<IBackgroundOperator> _channel =
        Channel.CreateUnbounded<IBackgroundOperator>(
            new UnboundedChannelOptions { SingleReader = true });

    private readonly ManualResetEventSlim _taskCompleted = new();

    private int _operatorCount;

    public BackgroundWorker(IServiceProvider dic)
        : base(dic)
    {
        Task.Run(ChannelLoopAsync);

        Trash.Add(() =>
        {
            _channel.Writer.TryComplete();
            _taskCompleted.Wait();
            _taskCompleted.Dispose();
        });
    }

    private async Task ChannelLoopAsync()
    {
        while (await _channel.Reader.WaitToReadAsync().ConfigureAwait(false))
        {
            if (_channel.Reader.TryRead(out var @operator) == false)
                continue;

            ExecutingOperator = @operator.Name;

            try
            {
                using var d = @operator.ObserveProperty(x => x.Progress).Subscribe(x => Progress = x);

                Dic.GetInstance<ILoggerService>()
                    .Information($"{nameof(BackgroundWorker)}.{nameof(ChannelLoopAsync)} : Start {@operator.Name}");

                await @operator.ExecuteAsync().ConfigureAwait(false);

                Dic.GetInstance<ILoggerService>()
                    .Information($"{nameof(BackgroundWorker)}.{nameof(ChannelLoopAsync)} : End   {@operator.Name}");
            }
            catch (Exception e)
            {
                ExceptionThrown?.Invoke(this, new ExceptionThrownEventArgs(e));
            }
            finally
            {
                ExecutingOperator = "";
            }

            Progress = 100;

            @operator.Dispose();
            IsInProcessing = Interlocked.Decrement(ref _operatorCount) > 0;

            Progress = 0;
        }

        _taskCompleted.Set();
    }

    public ValueTask PushOperatorAsync(IBackgroundOperator @operator)
    {
        IsInProcessing = Interlocked.Increment(ref _operatorCount) > 0;

        return _channel.Writer.WriteAsync(@operator);
    }
}