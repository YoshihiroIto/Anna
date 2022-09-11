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
            e =>
            {
                Entries.Add(new Entry { Name = e.Name ?? "??????????????????" });
            }
            ).AddTo(_trash);

        watcher.Changed
            .Subscribe(
            e =>
            {
                Debug.WriteLine("Changed:" + e.FullPath);
            }
            ).AddTo(_trash);

        watcher.Deleted
            .Subscribe(
            e =>
            {
                var target = Entries.FirstOrDefault(x => x.Name == e.Name);
                if (target is null)
                {
                    // todo: logging
                    return;
                }

                Entries.Remove(target);
            }
            ).AddTo(_trash);

        watcher.Renamed
            .Subscribe(
            e =>
            {
                var target = Entries.FirstOrDefault(x => x.Name == e.OldName);
                if (target is null)
                {
                    // todo: logging
                    return;
                }

                target.Name = e.Name ?? "??????????????????";
            }
            ).AddTo(_trash);

        watcher.Errors
            .Subscribe(
            x => Debug.WriteLine("Errors:" + x.GetException())
            ).AddTo(_trash);
    }
}