using Anna.Foundation;
using NaturalSort.Extension;
using System.Diagnostics;

namespace Anna.DomainModel;

public class Entry : NotificationObject
{
    #region NameWithExtension

    private string _NameWithExtension = "";

    public string NameWithExtension
    {
        get => _NameWithExtension;
        set => SetProperty(ref _NameWithExtension, value);
    }

    #endregion
    
    
    #region Name

    private string _Name = "";

    public string Name
    {
        get => _Name;
        internal set => SetProperty(ref _Name, value);
    }

    #endregion


    #region Extension

    private string _Extension = "";

    public string Extension
    {
        get => _Extension;
        set => SetProperty(ref _Extension, value);
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
        target.NameWithExtension = NameWithExtension;
        target.Name = Name;
        target.Extension = Extension;
        target.Timestamp = Timestamp;
        target.Size = Size;
        target.Attributes = Attributes;
    }

    public void SetName(string nameWithExtension)
    {
        NameWithExtension = nameWithExtension;
        
        // dotfile
        if (nameWithExtension.Length > 0 && nameWithExtension[0] == '.')
        {
            Name = Path.GetExtension(nameWithExtension);
            Extension = "";
        }
        else
        {
            Name = Path.GetFileNameWithoutExtension(nameWithExtension);
            Extension = string.Intern(Path.GetExtension(nameWithExtension));
        }
    }

    public static int CompareByName(Entry x, Entry y)
    {
        Debug.Assert(x.IsDirectory == y.IsDirectory);
        
        if (x.IsDirectory)
        {
            var nameWithExt = NameComparer.Compare(x.NameWithExtension, y.NameWithExtension);
            if (nameWithExt != 0)
                return nameWithExt;
        }
        else
        {
            var name = NameComparer.Compare(x.Name, y.Name);
            if (name != 0)
                return name;
            
            var ext = NameComparer.Compare(x.Extension, y.Extension);
            if (ext != 0)
                return ext;
        }

        return 0;
    }

    public static int CompareByExtension(Entry x, Entry y)
    {
        Debug.Assert(x.IsDirectory == y.IsDirectory);
        
        if (x.IsDirectory)
        {
            var nameWithExt = NameComparer.Compare(x.NameWithExtension, y.NameWithExtension);
            if (nameWithExt != 0)
                return nameWithExt;
        }
        else
        {
            var ext = NameComparer.Compare(x.Extension, y.Extension);
            if (ext != 0)
                return ext;
            
            var name = NameComparer.Compare(x.Name, y.Name);
            if (name != 0)
                return name;
        }

        return 0;
    }

    public static Entry Create(string fillPath, string nameWithExtension)
    {
        var fi = new FileInfo(fillPath);

        var isDirectory = (fi.Attributes & FileAttributes.Directory) == FileAttributes.Directory;
        
        var e = new Entry
        {
            Timestamp = fi.LastWriteTime,
            Size = isDirectory ? 0 : fi.Length,
            Attributes = fi.Attributes
        };

        e.SetName(nameWithExtension);

        return e;
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
    
    private static readonly NaturalSortComparer NameComparer = new (StringComparison.OrdinalIgnoreCase);
}