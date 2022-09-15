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
    private readonly CompositeDisposable _trash = new();

    internal FileSystemDirectory(string path, IObjectLifetimeChecker objectLifetimeChecker)
        : base(path)
    {
        _objectLifetimeChecker = objectLifetimeChecker;
        _objectLifetimeChecker.Add(this);

        SetupWatcher(path);
    }

    protected override IEnumerable<Entry> EnumerateDirectories()
    {
        return System.IO.Directory.EnumerateDirectories(Path)
            .Select(p => Entry.Create(p, System.IO.Path.GetRelativePath(Path, p)));
    }

    protected override IEnumerable<Entry> EnumerateFiles()
    {
        return System.IO.Directory.EnumerateFiles(Path)
            .Select(p => Entry.Create(p, System.IO.Path.GetRelativePath(Path, p)));
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
            .Subscribe(e => OnCreated(Entry.Create(e.FullPath, e.Name ?? "????")))
            .AddTo(_trash);

        watcher.Changed
            .Subscribe(e => OnChanged(Entry.Create(e.FullPath, e.Name ?? "????")))
            .AddTo(_trash);

        watcher.Deleted
            .Subscribe(e => OnDeleted(e.Name ?? "????"))
            .AddTo(_trash);

        watcher.Renamed
            .Subscribe(e => OnRenamed(e.OldName ?? "????", e.Name ?? "????"))
            .AddTo(_trash);

        watcher.Errors
            .Subscribe(x => Debug.WriteLine("Errors:" + x.GetException()))
            .AddTo(_trash);
    }
}