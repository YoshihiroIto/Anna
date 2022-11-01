using Reactive.Bindings.Extensions;
using System.Reactive.Linq;
using IServiceProvider=Anna.Service.IServiceProvider;

namespace Anna.Foundation;

public sealed class ObservableFileSystemWatcher : HasArgDisposableNotificationObject<ObservableFileSystemWatcher, string>
{
    public readonly IObservable<FileSystemEventArgs> Created;
    public readonly IObservable<FileSystemEventArgs> Changed;
    public readonly IObservable<FileSystemEventArgs> Deleted;
    public readonly IObservable<RenamedEventArgs> Renamed;
    public readonly IObservable<ErrorEventArgs> Errors;

    public ObservableFileSystemWatcher(IServiceProvider dic)
        : base(dic)
    {
        var watcher = new FileSystemWatcher(Arg)
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
                h => watcher.Created += h,
                h => watcher.Created -= h)
            .Select(x => x.EventArgs);

        Changed = Observable
            .FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
                h => watcher.Changed += h,
                h => watcher.Changed -= h)
            .Select(x => x.EventArgs);

        Deleted = Observable
            .FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
                h => watcher.Deleted += h,
                h => watcher.Deleted -= h)
            .Select(x => x.EventArgs);

        Renamed = Observable
            .FromEventPattern<RenamedEventHandler, RenamedEventArgs>(
                h => watcher.Renamed += h,
                h => watcher.Renamed -= h)
            .Select(x => x.EventArgs);

        Errors = Observable
            .FromEventPattern<ErrorEventHandler, ErrorEventArgs>(
                h => watcher.Error += h,
                h => watcher.Error -= h)
            .Select(x => x.EventArgs);

        watcher.EnableRaisingEvents = true;
        watcher.AddTo(Trash);
    }
}