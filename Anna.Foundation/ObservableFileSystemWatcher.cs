using System.Reactive.Linq;

namespace Anna.Foundation;

public class ObservableFileSystemWatcher : IDisposable
{
    public readonly IObservable<FileSystemEventArgs> Created;
    public readonly IObservable<FileSystemEventArgs> Changed;
    public readonly IObservable<FileSystemEventArgs> Deleted;
    public readonly IObservable<RenamedEventArgs> Renamed;
    public readonly IObservable<ErrorEventArgs> Errors;

    public ObservableFileSystemWatcher(string path)
    {
        _watcher = new FileSystemWatcher(path)
        {
            NotifyFilter =
                NotifyFilters.FileName |
                NotifyFilters.DirectoryName |
                NotifyFilters.Attributes |
                NotifyFilters.Size |
                NotifyFilters.LastWrite |
                NotifyFilters.LastAccess |
                NotifyFilters.CreationTime |
                NotifyFilters.Security
        };

        Created = Observable
            .FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
            h => _watcher.Created += h,
            h => _watcher.Created -= h)
            .Select(x => x.EventArgs);

        Changed = Observable
            .FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
            h => _watcher.Changed += h,
            h => _watcher.Changed -= h)
            .Select(x => x.EventArgs);

        Deleted = Observable
            .FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
            h => _watcher.Deleted += h,
            h => _watcher.Deleted -= h)
            .Select(x => x.EventArgs);

        Renamed = Observable
            .FromEventPattern<RenamedEventHandler, RenamedEventArgs>(
            h => _watcher.Renamed += h,
            h => _watcher.Renamed -= h)
            .Select(x => x.EventArgs);

        Errors = Observable
            .FromEventPattern<ErrorEventHandler, ErrorEventArgs>(
            h => _watcher.Error += h,
            h => _watcher.Error -= h)
            .Select(x => x.EventArgs);

        _watcher.EnableRaisingEvents = true;
    }

    public void Dispose()
    {
        _watcher.Dispose();
    }

    private readonly FileSystemWatcher _watcher;
}