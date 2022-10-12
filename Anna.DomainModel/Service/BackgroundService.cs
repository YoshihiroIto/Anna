using Anna.Foundation;
using Anna.Service;
using Anna.Service.Interfaces;
using System.Threading.Channels;

namespace Anna.DomainModel.Service;

public class BackgroundService : NotificationObject, IBackgroundService, IDisposable
{
    #region IsInProcessing

    private bool _IsInProcessing;

    public bool IsInProcessing
    {
        get => _IsInProcessing;
        set => SetProperty(ref _IsInProcessing, value);
    }

    #endregion


    #region ProgressRatio

    private double _ProgressRatio;

    public double ProgressRatio
    {
        get => _ProgressRatio;
        set => SetProperty(ref _ProgressRatio, value);
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
    
    private Channel<IBackgroundServiceProcess> Channel { get; } =
        System.Threading.Channels.Channel.CreateUnbounded<IBackgroundServiceProcess>(
            new UnboundedChannelOptions { SingleReader = true });

    private readonly ManualResetEventSlim _taskCompleted = new();

    public BackgroundService()
    {
        Task.Run(async () =>
        {
            while (await Channel.Reader.WaitToReadAsync())
            {
                if (Channel.Reader.TryRead(out var process))
                    await process.ExecuteAsync();
            }

            _taskCompleted.Set();
        });
    }

    public void Dispose()
    {
        Channel.Writer.TryComplete();
        _taskCompleted.Wait();
        _taskCompleted.Dispose();
    }

    public void CopyFileSystemEntry(string destPath, IEnumerable<IEntry> sourceEntries)
    {
        throw new NotImplementedException();
    }
}

interface IBackgroundServiceProcess
{
    ValueTask ExecuteAsync();
}