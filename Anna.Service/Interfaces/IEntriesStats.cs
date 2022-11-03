using System.ComponentModel;

namespace Anna.Service.Interfaces;

public interface IEntriesStats : INotifyPropertyChanged
{
    bool IsInMeasuring { get; }
    int FileCount { get; }
    int FolderCount { get; }
    long AllSize { get; }
}

public sealed class NopEntriesStats : IEntriesStats
{
    public static readonly IEntriesStats Instance = new NopEntriesStats();

#pragma warning disable 0067
    public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore 0067
    public bool IsInMeasuring => false;
    public int FileCount => 0;
    public int FolderCount => 0;
    public long AllSize => 0L;

    private NopEntriesStats()
    {
    }
}