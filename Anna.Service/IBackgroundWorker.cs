using Anna.Service.Interfaces;
using System.ComponentModel;

namespace Anna.Service;

public interface IBackgroundWorker : INotifyPropertyChanged
{
    bool IsInProcessing { get; }
    double Progress { get; }
    string Message { get; }

    ValueTask CopyFileSystemEntryAsync(IFileSystemCopyOperator fileSystemCopyOperator, string destPath,
        IEnumerable<IEntry> sourceEntries, IEntriesStats stats);
}