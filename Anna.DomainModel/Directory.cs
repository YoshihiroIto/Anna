using Anna.Foundation;
using System.Diagnostics;

namespace Anna.DomainModel;

public abstract class Directory : NotificationObject
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
                    UpdateInternal();
                }
            });
        }
    }

    #endregion

    #region Entries

    private ObservableCollectionEx<Entry> _entries = new();

    public ObservableCollectionEx<Entry> Entries
    {
        get => _entries;
        private set => SetProperty(ref _entries, value);
    }

    #endregion


    #region SortOrder

    private SortModes _SortMode = SortModes.Name;

    public SortModes SortMode
    {
        get => _SortMode;
        set
        {
            if (SetProperty(ref _SortMode, value) == false)
                return;

            UpdateEntryCompare();
            SortEntries();
        }
    }

    #endregion


    #region SortMode

    private SortOrders _SortOrder = SortOrders.Ascending;

    public SortOrders SortOrder
    {
        get => _SortOrder;
        set
        {
            if (SetProperty(ref _SortOrder, value) == false)
                return;

            UpdateEntryCompare();
            SortEntries();
        }
    }

    #endregion

    public readonly object UpdateLockObj = new();

    protected Directory(string path)
    {
        Path = path;
    }

    protected void OnCreated(Entry newEntry)
    {
        Debug.WriteLine($"OnCreated: {newEntry.Name}");

        lock (UpdateLockObj)
        {
            AddEntityInternal(newEntry);
        }
    }

    protected void OnChanged(Entry entry)
    {
        Debug.WriteLine($"OnChanged: {entry.Name}");

        lock (UpdateLockObj)
        {
            if (_entriesDict.TryGetValue(entry.Name, out var target) == false)
                return;// todo: logging

            entry.CopyTo(target);
        }
    }

    protected void OnDeleted(string name)
    {
        Debug.WriteLine($"OnDeleted: {name}");

        lock (UpdateLockObj)
        {
            if (_entriesDict.TryGetValue(name, out var target) == false)
                return;// todo: logging

            RemoveEntityInternal(target);
        }
    }

    protected void OnRenamed(string oldName, string newName)
    {
        Debug.WriteLine($"OnRenamed: {oldName}, {newName}");

        lock (UpdateLockObj)
        {
            if (_entriesDict.TryGetValue(oldName, out var target) == false)
                return;// todo: logging

            RemoveEntityInternal(target);

            var newEntry = new Entry();
            target.CopyTo(newEntry);
            newEntry.Name = newName;

            AddEntityInternal(newEntry);
        }
    }

    private void UpdateInternal()
    {
        try
        {
            Entries.BeginChange();

            Entries.Clear();

            Entries.AddRange(EnumerateDirectories());
            _directoriesCount = Entries.Count;

            Entries.AddRange(EnumerateFiles());
            _filesCount = Entries.Count - _directoriesCount;

            SortEntries();

            //
            _entriesDict.Clear();
            foreach (var e in Entries)
                _entriesDict.Add(e.Name, e);
        }
        finally
        {
            Entries.EndChange();
        }
    }

    private void AddEntityInternal(Entry entry)
    {
        if (entry.IsDirectory)
        {
            Span<Entry> span = Entries.AsSpan().Slice(0, _directoriesCount);
            var pos = SpanHelper.UpperBound(span, entry, _entryCompare);

            Entries.Insert(pos, entry);

            ++_directoriesCount;
        }
        else
        {
            Span<Entry> span = Entries.AsSpan().Slice(_directoriesCount, _filesCount);
            var pos = SpanHelper.UpperBound(span, entry, _entryCompare);

            Entries.Insert(_directoriesCount + pos, entry);

            ++_filesCount;
        }

        _entriesDict.Add(entry.Name, entry);
    }

    private void RemoveEntityInternal(Entry entry)
    {
        Entries.Remove(entry);

        if (entry.IsDirectory)
            --_directoriesCount;
        else
            --_filesCount;

        if (_entriesDict.Remove(entry.Name) == false)
            Debug.WriteLine(entry.Name);
    }

    private void UpdateEntryCompare()
    {
        _entryCompare = EntryComparison.FindEntryCompare(SortMode, SortOrder);
    }

    private void SortEntries()
    {
        try
        {
            Entries.BeginChange();

            Entries.AsSpan().Slice(0, _directoriesCount).Sort(_entryCompare);
            Entries.AsSpan().Slice(_directoriesCount, _filesCount).Sort(_entryCompare);
        }
        finally
        {
            Entries.EndChange();
        }
    }

    private Comparison<Entry> _entryCompare = EntryComparison.ByNameAscending;

    protected abstract IEnumerable<Entry> EnumerateDirectories();
    protected abstract IEnumerable<Entry> EnumerateFiles();

    private int _directoriesCount;
    private int _filesCount;
    private readonly Dictionary<string, Entry> _entriesDict = new();
}

public enum SortModes
{
    Name,
    Extension,
    Timestamp,
    Size,
    Attributes,
}

public enum SortOrders
{
    Ascending,
    Descending
}