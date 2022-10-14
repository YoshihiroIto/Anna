using Anna.Service.Interfaces;
using System.ComponentModel;

namespace Anna.Service;

public interface IBackgroundService : INotifyPropertyChanged
{
    bool IsInProcessing { get; }
    double Progress { get; }
    string Message { get; }

    ValueTask CopyFileSystemEntryAsync(string destPath, IEnumerable<IEntry> sourceEntries, IEntriesStats stats);
}