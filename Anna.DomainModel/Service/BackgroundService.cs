using Anna.Foundation;
using Anna.Service;
using Anna.Service.Interfaces;
using System.Threading.Channels;
using IDisposable=System.IDisposable;
using IServiceProvider=Anna.Service.IServiceProvider;

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

    private Channel<IBackgroundServiceProcess> Channel { get; } =
        System.Threading.Channels.Channel.CreateUnbounded<IBackgroundServiceProcess>(
            new UnboundedChannelOptions { SingleReader = true });

    private readonly ManualResetEventSlim _taskCompleted = new();
    private readonly IServiceProvider _dic;

    private int _processCount;

    public BackgroundService(IServiceProvider dic)
    {
        _dic = dic;

        Task.Run(async () =>
        {
            while (await Channel.Reader.WaitToReadAsync())
            {
                if (Channel.Reader.TryRead(out var process))
                {
                    await process.ExecuteAsync();
                    process.Dispose();

                    IsInProcessing = Interlocked.Decrement(ref _processCount) > 0;
                }
            }

            _taskCompleted.Set();
        });
    }

    public void Dispose()
    {
        Channel.Writer.TryComplete();
        _taskCompleted.Wait();
        _taskCompleted.Dispose();

        GC.SuppressFinalize(this);
    }

    public ValueTask CopyFileSystemEntryAsync(string destPath, IEnumerable<IEntry> sourceEntries, IEntriesStats stats)
    {
        var process =
            _dic.GetInstance<CopyFileSystemEntryProcess,
                    (IFileSystemService, string, IEnumerable<IEntry>, IEntriesStats)>
                ((_dic.GetInstance<IFileSystemService>(), destPath, sourceEntries, stats));

        IsInProcessing = Interlocked.Increment(ref _processCount) > 0;

        return Channel.Writer.WriteAsync(process);
    }
}

internal interface IBackgroundServiceProcess : IDisposable
{
    double Progress { get; }
    string Message { get; }

    ValueTask ExecuteAsync();
}

internal class CopyFileSystemEntryProcess
    : HasArgDisposableNotificationObject<(
            IFileSystemService FileSystemService,
            string DestPath,
            IEnumerable<IEntry> SourceEntries,
            IEntriesStats Stats)>
        , IBackgroundServiceProcess
{
    #region Progress

    private double _Progress = 0.5;

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

    public CopyFileSystemEntryProcess(IServiceProvider dic)
        : base(dic)
    {
        if (Arg.Stats is IDisposable disposable)
            Trash.Add(disposable);
    }

    public ValueTask ExecuteAsync()
    {
        Arg.FileSystemService.Copy(Arg.SourceEntries, Arg.DestPath);

        return ValueTask.CompletedTask;
    }
}