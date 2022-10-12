using Anna.Service.Interfaces;
using System.ComponentModel;

namespace Anna.Service;

public interface IBackgroundService : INotifyPropertyChanged
{
    bool IsInProcessing { get; }
    double ProgressRatio { get; }
    string Message { get; }

    void CopyFileSystemEntry(string destPath, IEnumerable<IEntry> sourceEntries);
}