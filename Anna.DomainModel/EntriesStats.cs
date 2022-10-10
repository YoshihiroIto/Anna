using Anna.Foundation;
using Anna.Service;

namespace Anna.DomainModel;

public sealed class EntriesStats : NotificationObject
{
    private readonly IFolderService _folderService;

    #region FileCount

    private int _FileCount;

    public int FileCount
    {
        get => _FileCount;
        set => SetProperty(ref _FileCount, value);
    }

    #endregion

    #region FolderCount

    private int _FolderCount;

    public int FolderCount
    {
        get => _FolderCount;
        set => SetProperty(ref _FolderCount, value);
    }

    #endregion

    #region AllSize

    private long _AllSize;

    public long AllSize
    {
        get => _AllSize;
        set => SetProperty(ref _AllSize, value);
    }

    #endregion

    public EntriesStats(IFolderService folderService)
    {
        _folderService = folderService;
    }

    public EntriesStats Measure(Entry[] targets)
    {
        // for make minimum stats
        var mre = new ManualResetEventSlim();

        Task.Run(() => Measure(targets, mre));

        mre.Wait();

        return this;
    }

    private void Measure(Entry[] targets, ManualResetEventSlim? mre)
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
            }

            if (mre is not null)
            {
                mre.Set();
                mre = null;
            }
        }

        mre?.Set();
    }

    private void MeasureFolder(string path)
    {
        if (_folderService.IsAccessible(path) == false)
            return;
        
        var di = new DirectoryInfo(path);

        foreach (var d in di.EnumerateDirectories())
        {
            ++FolderCount;
            MeasureFolder(d.FullName);
        }

        foreach (var f in di.EnumerateFiles())
        {
            ++FileCount;
            AllSize += f.Length;
        }
    }
}