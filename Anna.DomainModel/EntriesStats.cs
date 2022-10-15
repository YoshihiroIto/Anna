using Anna.Foundation;
using Anna.Service;
using Anna.Service.Interfaces;
using IServiceProvider=Anna.Service.IServiceProvider;

namespace Anna.DomainModel;

public sealed class EntriesStats : DisposableNotificationObject, IEntriesStats
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

    // for make minimum stats
    private readonly ManualResetEventSlim _mre = new();

    public EntriesStats(IServiceProvider dic)
        : base(dic)
    {
        _fileSystemIsAccessibleService = dic.GetInstance<IFileSystemIsAccessibleService>();
    }

    public override void Dispose()
    {
        _measuringMre.Wait();
        _measuringMre.Dispose();

        _mre.Dispose();
        base.Dispose();
    }

    public EntriesStats Measure(Entry[] targets, CancellationToken ct)
    {
        Task.Run(() =>
            {
                try
                {
                    IsInMeasuring = true;

                    // ReSharper disable once AccessToDisposedClosure
                    MeasureInternal(targets, ct);
                }
                finally
                {
                    IsInMeasuring = false;
                    _measuringMre.Set();
                }
            },
            ct);

        _mre.Wait(ct);

        return this;
    }

    private void MeasureInternal(Entry[] targets, CancellationToken ct)
    {
        foreach (var target in targets)
        {
            if (target.IsSelectable == false)
                continue;

            if (target.IsFolder)
            {
                ++FolderCount;
                MeasureFolder(target.Path, ct);
            }
            else
            {
                ++FileCount;
                AllSize += target.Size;

                if (_mre.IsSet == false)
                    _mre.Set();
            }

            if (ct.IsCancellationRequested)
                break;
        }

        if (_mre.IsSet == false)
            _mre.Set();
    }

    private void MeasureFolder(string path, CancellationToken ct)
    {
        if (_fileSystemIsAccessibleService.IsAccessible(path) == false)
            return;

        var di = new DirectoryInfo(path);

        foreach (var d in di.EnumerateDirectories())
        {
            ++FolderCount;
            MeasureFolder(d.FullName, ct);

            if (ct.IsCancellationRequested)
                break;
        }

        foreach (var f in di.EnumerateFiles())
        {
            ++FileCount;
            AllSize += f.Length;

            if (_mre.IsSet == false)
                _mre.Set();

            if (ct.IsCancellationRequested)
                break;
        }
    }
}