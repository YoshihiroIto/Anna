﻿using Anna.Foundation;
using Anna.Service.Services;
using Reactive.Bindings.Extensions;
using System.Reactive.Disposables;
using System.Runtime.CompilerServices;
using IServiceProvider=Anna.Service.IServiceProvider;

[assembly: InternalsVisibleTo("Anna.DomainModel.Interactor")]

namespace Anna.DomainModel.FileSystem;

public sealed class FileSystemFolder : Folder
{
    public override bool IsRoot => System.IO.Path.GetPathRoot(Path) == Path;

    private readonly CompositeDisposable _watchTrash = new();

    internal FileSystemFolder(IServiceProvider dic, int id, string path)
        : base(dic, id, path)
    {
        UpdateWatcher(path);

        this.ObserveProperty(x => x.Path)
            .Subscribe(UpdateWatcher)
            .AddTo(Trash);

        _watchTrash.AddTo(Trash);
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

    public override Stream OpenRead(string path)
    {
        return File.OpenRead(path);
    }

    public override Task<byte[]> ReadAllAsync(string path)
    {
        return File.ReadAllBytesAsync(path);
    }

    public override void CreateEntry(bool isFolder, string path, bool isInvokeEntryExplicitlyCreated)
    {
        if (isFolder)
            Directory.CreateDirectory(path);
        else
            File.WriteAllBytes(path, Array.Empty<byte>());

        if (isInvokeEntryExplicitlyCreated)
            InvokeEntryExplicitlyCreated(path);
    }

    public override void RenameEntry(Entry entry, string newName, bool isInvokeEntryExplicitlyCreated)
    {
        var destPath = System.IO.Path.Combine(
            System.IO.Path.GetDirectoryName(entry.Path) ?? throw new NullReferenceException(),
            newName);

        if (entry.IsFolder)
            Directory.Move(entry.Path, destPath);
        else
            File.Move(entry.Path, destPath);

        if (isInvokeEntryExplicitlyCreated)
            InvokeEntryExplicitlyCreated(destPath);
    }

    private void UpdateWatcher(string path)
    {
        _watchTrash.Clear();

        var watcher = Dic.GetInstance(ObservableFileSystemWatcher.T, path).AddTo(_watchTrash);

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
            .Subscribe(x => Dic.GetInstance<ILoggerService>().Error(x.GetException().ToString()))
            .AddTo(_watchTrash);
    }

    private Entry? Create(string path, string nameWithExtension)
    {
        var fileInfo = new FileInfo(path);

        return Create(fileInfo, path, nameWithExtension);
    }

    private Entry? Create(FileSystemInfo fsInfo, string path, string nameWithExtension)
    {
        if ((int)fsInfo.Attributes == -1)
            return null;

        var isFolder = (fsInfo.Attributes & FileAttributes.Directory) == FileAttributes.Directory;

        var entry = new Entry(this)
        {
            Timestamp = fsInfo.LastWriteTime,
            Size = isFolder ? 0 : (fsInfo as FileInfo)!.Length,
            Attributes = fsInfo.Attributes,
            IsParentFolder = nameWithExtension == "..",
            Path = path
        };

        entry.SetName(nameWithExtension, false);

        return entry;
    }
}