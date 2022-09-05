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
        private set => SetProperty(ref _Path, value);
    }

    #endregion
    
    #region Directories

    private ObservableCollection<FileObject> _Directories = new();

    public ObservableCollection<FileObject> Directories
    {
        get => _Directories;
        private set => SetProperty(ref _Directories, value);
    }

    #endregion

    #region Files

    private ObservableCollection<FileObject> _Files = new();

    public ObservableCollection<FileObject> Files
    {
        get => _Files;
        private set => SetProperty(ref _Files, value);
    }

    #endregion
}