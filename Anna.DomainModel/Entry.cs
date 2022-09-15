using Anna.Foundation;

namespace Anna.DomainModel;

public class Entry : NotificationObject
{
    #region Name

    private string _Name = "";

    public string Name
    {
        get => _Name;
        internal set => SetProperty(ref _Name, value);
    }

    #endregion


    #region Timestamp

    private DateTime _Timestamp;

    public DateTime Timestamp
    {
        get => _Timestamp;
        internal set => SetProperty(ref _Timestamp, value);
    }

    #endregion


    #region Size

    private long _Size;

    public long Size
    {
        get => _Size;
        internal set => SetProperty(ref _Size, value);
    }

    #endregion


    #region Attributes

    private FileAttributes _Attributes;

    public FileAttributes Attributes
    {
        get => _Attributes;
        set => SetProperty(ref _Attributes, value);
    }

    #endregion

    public bool IsDirectory => (Attributes & FileAttributes.Directory) == FileAttributes.Directory;

    public void CopyTo(Entry target)
    {
        target.Name = Name;
        target.Timestamp = Timestamp;
        target.Size = Size;
        target.Attributes = Attributes;
    }

    public static Entry Create(string fillPath, string name)
    {
        var fi = new FileInfo(fillPath);

        var isDirectory = (fi.Attributes & FileAttributes.Directory) == FileAttributes.Directory;

        return new Entry
        {
            Name = name,
            Timestamp = fi.LastWriteTime,
            Size = isDirectory ? 0 : fi.Length,
            Attributes = fi.Attributes
        };
    }

    public static Entry Create(Entry from)
    {
        var entry = new Entry();
        from.CopyTo(entry);

        return entry;
    }

    private Entry()
    {
    }
}