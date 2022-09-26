using Anna.Foundation;
using Anna.UseCase;
using Reactive.Bindings.Extensions;
using System.Reactive.Disposables;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Anna.DomainModel.Interactor")]

namespace Anna.DomainModel.FileSystem;

public sealed class FileSystemDirectory : Directory
{
    internal FileSystemDirectory(string path, ILoggerUseCase logger,
        IObjectLifetimeCheckerUseCase objectLifetimeChecker)
        : base(path, logger)
    {
        _objectLifetimeChecker = objectLifetimeChecker;

        _objectLifetimeChecker.Add(this);
        UpdateWatcher(path);

        _pathObserver = this.ObserveProperty(x => x.Path)
            .Subscribe(UpdateWatcher);
    }

    protected override IEnumerable<Entry> EnumerateDirectories()
    {
        if (string.CompareOrdinal(System.IO.Path.GetPathRoot(Path), Path) != 0)
        {
            var d = new DirectoryInfo(Path);
            yield return Entry.Create(d.Parent?.FullName ?? throw new NullReferenceException(), "..");
        }

        foreach (var entry in System.IO.Directory.EnumerateDirectories(Path)
                     .Select(p => Entry.Create(p, System.IO.Path.GetRelativePath(Path, p))))
            yield return entry;
    }

    protected override IEnumerable<Entry> EnumerateFiles()
    {
        return System.IO.Directory.EnumerateFiles(Path)
            .Select(p => Entry.Create(p, System.IO.Path.GetRelativePath(Path, p)));
    }

    public override void Dispose()
    {
        if (_isDispose)
            return;

        _isDispose = true;

        _pathObserver.Dispose();
        _watchTrash.Dispose();
        _objectLifetimeChecker.Remove(this);
    }

    private void UpdateWatcher(string path)
    {
        _watchTrash.Clear();

        var watcher = new ObservableFileSystemWatcher(path).AddTo(_watchTrash);

        watcher.Created
            .Subscribe(e => OnCreated(Entry.Create(e.FullPath, e.Name ?? "????")))
            .AddTo(_watchTrash);

        watcher.Changed
            .Subscribe(e => OnChanged(Entry.Create(e.FullPath, e.Name ?? "????")))
            .AddTo(_watchTrash);

        watcher.Deleted
            .Subscribe(e => OnDeleted(e.Name ?? "????"))
            .AddTo(_watchTrash);

        watcher.Renamed
            .Subscribe(e => OnRenamed(e.OldName ?? "????", e.Name ?? "????"))
            .AddTo(_watchTrash);

        watcher.Errors
            .Subscribe(x => _Logger.Error(x.GetException().ToString()))
            .AddTo(_watchTrash);
    }

    private bool _isDispose;
    private readonly IObjectLifetimeCheckerUseCase _objectLifetimeChecker;

    private readonly IDisposable _pathObserver;
    private readonly CompositeDisposable _watchTrash = new();
}