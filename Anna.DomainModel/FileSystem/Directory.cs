using Anna.DomainModel.Foundations;
using System.Collections.ObjectModel;

namespace Anna.DomainModel.FileSystem;

public class Directory : NotificationObject
{
    #region Path

    private string _Path = "";

    public string Path
    {
        get => _Path;
        private set
        {
            if (SetProperty(ref _Path, value) == false)
                return;

            Task.Run(() =>
            {
                lock (UpdateLockObj)
                {
                    Update();
                }
            });
        }
    }

    #endregion


    #region Directories

    private ObservableCollection<Entry> _Directories = new();

    public ObservableCollection<Entry> Directories
    {
        get => _Directories;
        private set => SetProperty(ref _Directories, value);
    }

    #endregion


    #region Files

    private ObservableCollection<Entry> _Files = new();

    public ObservableCollection<Entry> Files
    {
        get => _Files;
        private set => SetProperty(ref _Files, value);
    }

    #endregion


    #region DirectoriesAndFiles

    private ObservableCollection<Entry> _entries = new();

    public ObservableCollection<Entry> Entries
    {
        get => _entries;
        private set => SetProperty(ref _entries, value);
    }

    #endregion

    public readonly object UpdateLockObj = new();

    public Directory(string path)
    {
        Path = path;
    }

    private void Update()
    {
        Directories.Clear();
        Files.Clear();
        Entries.Clear();

        foreach (var p in System.IO.Directory.EnumerateDirectories(Path))
        {
            var e = new Entry { Name = System.IO.Path.GetRelativePath(Path, p) };
            Directories.Add(e);
            Entries.Add(e);
        }

        foreach (var p in System.IO.Directory.EnumerateFiles(Path))
        {
            var e = new Entry { Name = System.IO.Path.GetRelativePath(Path, p) };
            Files.Add(e);
            Entries.Add(e);
        }
    }
}