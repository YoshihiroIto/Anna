using Anna.DomainModel.Foundations;

namespace Anna.DomainModel.FileSystem;

public class FileObject : NotificationObject
{
    #region Name

    private string _Name = "";

    public string Name
    {
        get => _Name;
        private set => SetProperty(ref _Name, value);
    }

    #endregion
}