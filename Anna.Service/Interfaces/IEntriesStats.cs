namespace Anna.Service.Interfaces;

public interface IEntriesStats
{
    bool IsInMeasuring { get; }
    int FileCount { get; }
    int FolderCount { get; }
    long AllSize { get; }
}