using Anna.Foundation;
using Anna.UseCase;
using Reactive.Bindings.Extensions;
using System.Reactive.Disposables;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Anna.DomainModel.Interactor")]

namespace Anna.DomainModel.FileSystem;

public sealed class FileSystemDirectory : Directory, IDisposable
{
    internal FileSystemDirectory(string path, ILoggerUseCase logger,
        IObjectLifetimeCheckerUseCase objectLifetimeChecker)
        : base(path, logger)
    {
        _objectLifetimeChecker = objectLifetimeChecker;

        _objectLifetimeChecker.Add(this);
        SetupWatcher(path);
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

    public void Dispose()
    {
        if (_isDispose)
            return;

        _isDispose = true;

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
            .Subscribe(x => _Logger.Error(x.GetException().ToString()))
            .AddTo(_trash);
    }

    private bool _isDispose;
    private readonly IObjectLifetimeCheckerUseCase _objectLifetimeChecker;
    private readonly CompositeDisposable _trash = new();
}