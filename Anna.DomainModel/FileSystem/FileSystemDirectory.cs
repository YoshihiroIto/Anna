using Anna.Foundation;
using Anna.UseCase;
using Reactive.Bindings.Extensions;
using System.Reactive.Disposables;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Anna.DomainModel.Interactor")]

namespace Anna.DomainModel.FileSystem;

public sealed class FileSystemDirectory : Directory
{
    public override bool IsRoot => string.CompareOrdinal(System.IO.Path.GetPathRoot(Path), Path) == 0;

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
        if (IsRoot == false)
        {
            var d = new DirectoryInfo(Path);

            var entry = Create(d.Parent?.FullName ?? throw new NullReferenceException($"{Path}"), "..");

            if (entry is not null)
                yield return entry;
        }

        var dirInfo = new DirectoryInfo(Path);

        foreach (var fileInfo in dirInfo.EnumerateDirectories())
        {
            var file = fileInfo.FullName;

            var entry = Create(fileInfo, file, fileInfo.Name);

            if (entry is not null)
                yield return entry;
        }
    }

    protected override IEnumerable<Entry> EnumerateFiles()
    {
        var dirInfo = new DirectoryInfo(Path);

        foreach (var fileInfo in dirInfo.EnumerateFiles())
        {
            var file = fileInfo.FullName;

            var entry = Create(fileInfo, file, fileInfo.Name);

            if (entry is not null)
                yield return entry;
        }
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
            .Subscribe(e =>
            {
                var entry = Create(e.FullPath, e.Name ?? "????");
                if (entry is null)
                    return;

                OnCreated(entry);
            })
            .AddTo(_watchTrash);

        watcher.Changed
            .Subscribe(e =>
            {
                var entry = Create(e.FullPath, e.Name ?? "????");
                if (entry is null)
                    return;

                OnChanged(entry);
            })
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

    private static Entry? Create(string path, string nameWithExtension)
    {
        var fileInfo = new FileInfo(path);

        return Create(fileInfo, path, nameWithExtension);
    }

    private static Entry? Create(FileSystemInfo fsInfo, string path, string nameWithExtension)
    {
        if ((int)fsInfo.Attributes == -1)
            return null;

        var isDirectory = (fsInfo.Attributes & FileAttributes.Directory) == FileAttributes.Directory;

        var entry = new Entry
        {
            Timestamp = fsInfo.LastWriteTime,
            Size = isDirectory ? 0 : (fsInfo as FileInfo)!.Length,
            Attributes = fsInfo.Attributes,
            IsParentDirectory = string.CompareOrdinal(nameWithExtension, "..") == 0,
            Path = path
        };

        entry.SetName(nameWithExtension, false);

        return entry;
    }

    private bool _isDispose;
    private readonly IObjectLifetimeCheckerUseCase _objectLifetimeChecker;

    private readonly IDisposable _pathObserver;
    private readonly CompositeDisposable _watchTrash = new();
}