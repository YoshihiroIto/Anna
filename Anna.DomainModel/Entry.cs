using Anna.Foundation;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Anna.DomainModel;

[DebuggerDisplay("Name={Name}, Path={Path}")]
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


    #region IsParentFolder

    private bool _isParentFolder;

    public bool IsParentFolder
    {
        get => _isParentFolder;
        set => SetProperty(ref _isParentFolder, value);
    }

    #endregion


    #region Attributes

    private FileAttributes _Attributes;

    public FileAttributes Attributes
    {
        get => _Attributes;
        set
        {
            if (SetProperty(ref _Attributes, value) == false)
                return;

            RaisePropertyChanged(nameof(IsFolder));
            RaisePropertyChanged(nameof(IsReadOnly));
            RaisePropertyChanged(nameof(IsReparsePoint));
            RaisePropertyChanged(nameof(IsHidden));
            RaisePropertyChanged(nameof(IsSystem));
            RaisePropertyChanged(nameof(IsCompressed));
            RaisePropertyChanged(nameof(IsEncrypted));
        }
    }

    #endregion

    public string Path { get;  set; } = "";

    public bool IsFolder => (Attributes & FileAttributes.Directory) == FileAttributes.Directory;
    public bool IsSelectable => IsParentFolder == false;

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

    private Folder _folder;

    public Entry(Folder folder)
    {
        _folder = folder;
    }

    public void CopyTo(Entry target)
    {
        CopyToWithoutIsSelected(target);

        target.IsSelected = IsSelected;
    }

    public void CopyToWithoutIsSelected(Entry target)
    {
        target._folder = _folder;
        target.NameWithExtension = NameWithExtension;
        target.Name = Name;
        target.Extension = Extension;
        target.Timestamp = Timestamp;
        target.Size = Size;
        target.IsParentFolder = IsParentFolder;
        target.Attributes = Attributes;
        target.Path = Path;
    }

    public void SetName(string nameWithExtension, bool updatePath)
    {
        NameWithExtension = nameWithExtension;

        if (nameWithExtension == "..")
        {
            Name = nameWithExtension;
            Extension = "";
        }
        else if (PathStringHelper.IsDotfile(nameWithExtension))
        {
            Name = System.IO.Path.GetExtension(nameWithExtension);
            Extension = "";
        }
        else
        {
            Name = System.IO.Path.GetFileNameWithoutExtension(nameWithExtension);
            Extension = string.Intern(System.IO.Path.GetExtension(nameWithExtension));
        }

        if (updatePath)
        {
            var dir = System.IO.Path.GetDirectoryName(Path);

            if (dir is not null)
            {
                Path = System.IO.Path.Combine(
                    System.IO.Path.GetDirectoryName(Path) ?? throw new NullReferenceException(),
                    NameWithExtension
                );
            }
        }
    }

    public static Entry CreateFrom(Entry from)
    {
        var entry = new Entry(from._folder);
        from.CopyTo(entry);

        return entry;
    }

    public Task<string> ReadStringAsync()
    {
        return _folder.ReadStringAsync(Path);
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