using Anna.Foundation;
using System.Runtime.CompilerServices;

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


    #region IsSelected

    private bool _IsSelected;

    public bool IsSelected
    {
        get => _IsSelected;
        set => SetProperty(ref _IsSelected, value);
    }

    #endregion
    

    #region IsParentDirectory

    private bool _IsParentDirectory;

    public bool IsParentDirectory
    {
        get => _IsParentDirectory;
        private set => SetProperty(ref _IsParentDirectory, value);
    }

    #endregion


    #region Attributes

    private FileAttributes _Attributes;

    public FileAttributes Attributes
    {
        get => _Attributes;
        private set
        {
            if (SetProperty(ref _Attributes, value) == false)
                return;

            RaisePropertyChanged(nameof(IsDirectory));
            RaisePropertyChanged(nameof(IsReadOnly));
            RaisePropertyChanged(nameof(IsReparsePoint));
            RaisePropertyChanged(nameof(IsHidden));
            RaisePropertyChanged(nameof(IsSystem));
            RaisePropertyChanged(nameof(IsCompressed));
            RaisePropertyChanged(nameof(IsEncrypted));
        }
    }

    #endregion

    public string Path { get; private set; } = "";

    public bool IsDirectory => (Attributes & FileAttributes.Directory) == FileAttributes.Directory;
    public bool IsSelectable => IsParentDirectory == false;

    public bool IsReadOnly
    {
        get => (Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly;
        set => SetAttribute(FileAttributes.ReadOnly, value);
    }

    public bool IsReparsePoint
    {
        get => (Attributes & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint;
        set => SetAttribute(FileAttributes.ReparsePoint, value);
    }

    public bool IsHidden
    {
        get => (Attributes & FileAttributes.Hidden) == FileAttributes.Hidden;
        set => SetAttribute(FileAttributes.Hidden, value);
    }

    public bool IsSystem
    {
        get => (Attributes & FileAttributes.System) == FileAttributes.System;
        set => SetAttribute(FileAttributes.System, value);
    }

    public bool IsEncrypted
    {
        get => (Attributes & FileAttributes.Encrypted) == FileAttributes.Encrypted;
        set => SetAttribute(FileAttributes.Encrypted, value);
    }

    public bool IsCompressed
    {
        get => (Attributes & FileAttributes.Compressed) == FileAttributes.Compressed;
        set => SetAttribute(FileAttributes.Compressed, value);
    }

    public void CopyTo(Entry target)
    {
        target.NameWithExtension = NameWithExtension;
        target.Name = Name;
        target.Extension = Extension;
        target.Timestamp = Timestamp;
        target.Size = Size;
        target.IsParentDirectory = IsParentDirectory;
        target.Attributes = Attributes;
        target.Path = Path;
        //
        target.IsSelected = IsSelected;
    }

    public void SetName(string nameWithExtension)
    {
        NameWithExtension = nameWithExtension;

        if (PathStringHelper.IsDotfile(nameWithExtension))
        {
            Name = System.IO.Path.GetExtension(nameWithExtension);
            Extension = "";
        }
        else
        {
            Name = System.IO.Path.GetFileNameWithoutExtension(nameWithExtension);
            Extension = string.Intern(System.IO.Path.GetExtension(nameWithExtension));
        }
    }

    public static Entry Create(string path, string nameWithExtension)
    {
        var fileInfo = new FileInfo(path);
        
        //
        // note:
        //
        // If the file does not exist at this timing, fileInfo will be in an invalid state.
        // Therefore, the entity created from the fileInfo will also be invalid.
        //
        
        var isDirectory = (fileInfo.Attributes & FileAttributes.Directory) == FileAttributes.Directory;

        var entry = new Entry
        {
            Timestamp = fileInfo.LastWriteTime, Size = isDirectory ? 0 : fileInfo.Length, Attributes = fileInfo.Attributes
        };

        entry.SetName(nameWithExtension);
        entry.IsParentDirectory = string.CompareOrdinal(nameWithExtension, "..") == 0;
        entry.Path = path;

        return entry;
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

    private void SetAttribute(FileAttributes a, bool i, [CallerMemberName] string propertyName = "")
    {
        if ((Attributes & a) == a == i)
            return;

        if (i)
            Attributes |= a;
        else
            Attributes &= ~a;

        RaisePropertyChanged(propertyName);
    }
}