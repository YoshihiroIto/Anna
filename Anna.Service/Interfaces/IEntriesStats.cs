using System.ComponentModel;

namespace Anna.Service.Interfaces;

public interface IEntriesStats : INotifyPropertyChanged
{
    bool IsInMeasuring { get; }
    int FileCount { get; }
    int FolderCount { get; }
    long AllSize { get; }
}