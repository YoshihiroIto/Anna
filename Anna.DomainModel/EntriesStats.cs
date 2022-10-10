using Anna.Foundation;
using Anna.Service;

namespace Anna.DomainModel;

public sealed class EntriesStats : NotificationObject
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

    private readonly IFolderService _folderService;

    public EntriesStats(IFolderService folderService)
    {
        _folderService = folderService;
    }

    public EntriesStats Measure(Entry[] targets)
    {
        // for make minimum stats
        using var mre = new ManualResetEventSlim();

        Task.Run(() =>
        {
            try
            {
                IsInMeasuring = true;

                // ReSharper disable once AccessToDisposedClosure
                Measure(targets, mre);
            }
            finally
            {
                IsInMeasuring = false;
            }
        });

        mre.Wait();

        return this;
    }

    private void Measure(Entry[] targets, ManualResetEventSlim mre)
    {
        foreach (var target in targets)
        {
            if (target.IsSelectable == false)
                continue;

            if (target.IsFolder)
            {
                ++FolderCount;
                MeasureFolder(target.Path, mre);
            }
            else
            {
                ++FileCount;
                AllSize += target.Size;

                if (mre.IsSet == false)
                    mre.Set();
            }
        }

        if (mre.IsSet == false)
            mre.Set();
    }

    private void MeasureFolder(string path, ManualResetEventSlim mre)
    {
        if (_folderService.IsAccessible(path) == false)
            return;

        var di = new DirectoryInfo(path);

        foreach (var d in di.EnumerateDirectories())
        {
            ++FolderCount;
            MeasureFolder(d.FullName, mre);
        }

        foreach (var f in di.EnumerateFiles())
        {
            ++FileCount;
            AllSize += f.Length;

            if (mre.IsSet == false)
                mre.Set();
        }
    }
}