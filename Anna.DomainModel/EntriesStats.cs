using Anna.Foundation;
using Anna.Service.Interfaces;
using Anna.Service.Services;
using IServiceProvider=Anna.Service.IServiceProvider;

namespace Anna.DomainModel;

public sealed class EntriesStats : HasArgDisposableNotificationObject<EntriesStats, IEntry[]>, IEntriesStats
{
    #region IsInMeasuring

    private bool _IsInMeasuring;

    public bool IsInMeasuring
    {
        get => _IsInMeasuring;
        private set => SetProperty(ref _IsInMeasuring, value);
    }

    #endregion

    
    #region FileCount

    private int _FileCount;

    public int FileCount
    {
        get => _FileCount;
        private set => SetProperty(ref _FileCount, value);
    }

    #endregion
    

    #region FolderCount

    private int _FolderCount;

    public int FolderCount
    {
        get => _FolderCount;
        private set => SetProperty(ref _FolderCount, value);
    }

    #endregion


    #region AllSize

    private long _AllSize;

    public long AllSize
    {
        get => _AllSize;
        private set => SetProperty(ref _AllSize, value);
    }

    #endregion

    private readonly IFileSystemIsAccessibleService _fileSystemIsAccessibleService;
    private readonly ManualResetEventSlim _measuringMre = new();

    private readonly CancellationTokenSource _cts = new();
    
    // for make minimum stats
    private readonly ManualResetEventSlim _mre = new();

    public EntriesStats(IServiceProvider dic)
        : base(dic)
    {
        _fileSystemIsAccessibleService = dic.GetInstance<IFileSystemIsAccessibleService>();

        Trash.Add(() =>
        {
            _cts.Cancel();

            _measuringMre.Wait();
            _measuringMre.Dispose();

            _cts.Dispose();

            _mre.Dispose();
        });
        
        DoMeasure();
    }

    private void DoMeasure()
    {
        Task.Run(() =>
            {
                try
                {
                    IsInMeasuring = true;

                    MeasureInternal(Arg);
                }
                finally
                {
                    IsInMeasuring = false;
                    _measuringMre.Set();
                }
            },
            _cts.Token);

        _mre.Wait(_cts.Token);
    }

    private void MeasureInternal(IEntry[] targets)
    {
        foreach (var target in targets)
        {
            if (target.IsSelectable == false)
                continue;

            if (target.IsFolder)
            {
                ++FolderCount;
                MeasureFolder(target.Path);
            }
            else
            {
                ++FileCount;
                AllSize += target.Size;

                if (_mre.IsSet == false)
                    _mre.Set();
            }

            if (_cts.Token.IsCancellationRequested)
                break;
        }

        if (_mre.IsSet == false)
            _mre.Set();
    }

    private void MeasureFolder(string path)
    {
        if (_fileSystemIsAccessibleService.IsAccessible(path) == false)
            return;

        try
        {
            var di = new DirectoryInfo(path);

            foreach (var d in di.EnumerateDirectories())
            {
                ++FolderCount;
                MeasureFolder(d.FullName);

                if (_cts.Token.IsCancellationRequested)
                    break;
            }

            foreach (var f in di.EnumerateFiles())
            {
                ++FileCount;
                AllSize += f.Length;

                if (_mre.IsSet == false)
                    _mre.Set();

                if (_cts.Token.IsCancellationRequested)
                    break;
            }
        }
        catch
        {
            // ignored
        }
    }
}