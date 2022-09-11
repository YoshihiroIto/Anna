using Anna.DomainModel.Interface;
using Anna.Foundation;
using Reactive.Bindings.Extensions;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Anna.DomainModel.Operator")]

namespace Anna.DomainModel.FileSystem;

public sealed class FileSystemDirectory : Directory, IDisposable
{
    private readonly IObjectLifetimeChecker _objectLifetimeChecker;

    internal FileSystemDirectory(string path, IObjectLifetimeChecker objectLifetimeChecker)
        : base(path)
    {
        _objectLifetimeChecker = objectLifetimeChecker;
        _objectLifetimeChecker.Add(this);

        SetupWatcher(path);
    }

    protected override void Update()
    {
        Entries.Clear();

        foreach (var p in System.IO.Directory.EnumerateDirectories(Path))
        {
            Entries.Add(
            new Entry { Name = System.IO.Path.GetRelativePath(Path, p) }
            );
        }

        foreach (var p in System.IO.Directory.EnumerateFiles(Path))
        {
            Entries.Add(
            new Entry { Name = System.IO.Path.GetRelativePath(Path, p) }
            );
        }
    }

    public void Dispose()
    {
        _objectLifetimeChecker.Remove(this);
        _trash.Dispose();
    }

    private void SetupWatcher(string path)
    {
        var watcher = new ObservableFileSystemWatcher(path).AddTo(_trash);

        watcher.Created
            .Subscribe(
                x => Debug.WriteLine("Created:" + x.FullPath)
            ).AddTo(_trash);

        watcher.Changed
            .Subscribe(
                x => Debug.WriteLine("Changed:" + x.FullPath)
            ).AddTo(_trash);

        watcher.Deleted
            .Subscribe(
                x => Debug.WriteLine("Deleted:" + x.FullPath)
            ).AddTo(_trash);

        watcher.Renamed
            .Subscribe(
                x => Debug.WriteLine("Renamed:" + x.FullPath)
            ).AddTo(_trash);

        watcher.Errors
            .Subscribe(
                x => Debug.WriteLine("Errors:" + x.GetException())
            ).AddTo(_trash);
    }

    private readonly CompositeDisposable _trash = new();
}