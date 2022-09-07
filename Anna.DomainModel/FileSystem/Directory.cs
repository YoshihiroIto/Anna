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

            Task.Run(Update);
        }
    }

    #endregion


    #region Directories

    private ObservableCollection<FileSystemEntry> _Directories = new();

    public ObservableCollection<FileSystemEntry> Directories
    {
        get => _Directories;
        private set => SetProperty(ref _Directories, value);
    }

    #endregion


    #region Files

    private ObservableCollection<FileSystemEntry> _Files = new();

    public ObservableCollection<FileSystemEntry> Files
    {
        get => _Files;
        private set => SetProperty(ref _Files, value);
    }

    #endregion


    #region DirectoriesAndFiles

    private ObservableCollection<FileSystemEntry> _DirectoriesAndFiles = new();

    public ObservableCollection<FileSystemEntry> DirectoriesAndFiles
    {
        get => _DirectoriesAndFiles;
        private set => SetProperty(ref _DirectoriesAndFiles, value);
    }

    #endregion


    public Directory()
    {
        Path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    }

    private void Update()
    {
        Directories.Clear();
        Files.Clear();
        DirectoriesAndFiles.Clear();

        foreach (var p in System.IO.Directory.EnumerateDirectories(Path))
        {
            var e = new FileSystemEntry { Name = System.IO.Path.GetRelativePath(Path, p) };
            Directories.Add(e);
            DirectoriesAndFiles.Add(e);
        }
        
        foreach (var p in System.IO.Directory.EnumerateFiles(Path))
        {
            var e = new FileSystemEntry { Name = System.IO.Path.GetRelativePath(Path, p) };
            Files.Add(e);
            DirectoriesAndFiles.Add(e);
        }
    }
}
