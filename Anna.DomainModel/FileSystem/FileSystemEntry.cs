using Anna.DomainModel.Foundations;

namespace Anna.DomainModel.FileSystem;

public class FileSystemEntry : NotificationObject
{
    #region Name

    private string _Name = "";

    public string Name
    {
        get => _Name;
        private set => SetProperty(ref _Name, value);
    }

    #endregion


    #region Timestamp

    private DateTime _Timestamp;

    public DateTime Timestamp
    {
        get => _Timestamp;
        private set => SetProperty(ref _Timestamp, value);
    }

    #endregion


    #region Size

    private ulong _Size;

    public ulong Size
    {
        get => _Size;
        private set => SetProperty(ref _Size, value);
    }

    #endregion
}