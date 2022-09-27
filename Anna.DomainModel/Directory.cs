using Anna.Constants;
using Anna.Foundation;
using Anna.UseCase;
using System.Buffers;

namespace Anna.DomainModel;

public abstract class Directory : NotificationObject, IDisposable
{
    public ObservableCollectionEx<Entry> Entries { get; } = new();
    public readonly object EntitiesUpdatingLockObj = new();
    
    #region Path

    private string _Path = "";

    public string Path
    {
        get => _Path;
        set
        {
            if (SetProperty(ref _Path, value) == false)
                return;

            Task.Run(() =>
            {
                lock (EntitiesUpdatingLockObj)
                {
                    UpdateEntries();
                }
            });
        }
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

            lock (EntitiesUpdatingLockObj)
            {
                SortEntries();
            }
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

            lock (EntitiesUpdatingLockObj)
            {
                SortEntries();
            }
        }
    }

    #endregion

    public void SetSortModeAndOrder(SortModes mode, SortOrders order)
    {
        if (mode == _SortMode && order == _SortOrder)
            return;

        _SortMode = mode;
        _SortOrder = order;

        UpdateEntryCompare();

        lock (EntitiesUpdatingLockObj)
        {
            SortEntries();
        }
    }

    protected Directory(string path, ILoggerUseCase logger)
    {
        _Logger = logger;
        Path = PathStringHelper.Normalize(path);
    }

    protected void OnCreated(Entry newEntry)
    {
        _Logger.Information($"OnCreated: {Path}, {newEntry.NameWithExtension}");

        lock (EntitiesUpdatingLockObj)
        {
            AddEntryInternal(newEntry);
        }
    }

    protected void OnChanged(Entry entry)
    {
        _Logger.Information($"OnChanged: {Path}, {entry.NameWithExtension}");

        lock (EntitiesUpdatingLockObj)
        {
            if (_entriesDict.TryGetValue(entry.NameWithExtension, out var target) == false)
            {
                _Logger.Error($"OnChanged: {Path}, {entry.NameWithExtension}");
                return;
            }

            entry.CopyTo(target);
        }
    }

    protected void OnDeleted(string name)
    {
        _Logger.Information($"OnDeleted: {Path}, {name}");

        lock (EntitiesUpdatingLockObj)
        {
            if (_entriesDict.TryGetValue(name, out var target) == false)
            {
                _Logger.Error($"OnDeleted: {Path}, {name}");
                return;
            }

            RemoveEntryInternal(target);
        }
    }

    protected void OnRenamed(string oldName, string newName)
    {
        _Logger.Information($"OnRenamed: {Path}, {oldName}, {newName}");

        lock (EntitiesUpdatingLockObj)
        {
            if (_entriesDict.TryGetValue(oldName, out var target) == false)
            {
                _Logger.Error($"OnRenamed: {Path}, {oldName}, {newName}");
                return;
            }

            RemoveEntryInternal(target);

            var newEntry = Entry.Create(target);
            newEntry.SetName(newName);

            AddEntryInternal(newEntry);
        }
    }

    private void UpdateEntries()
    {
        try
        {
            IsInEntriesUpdating = true;
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
                _entriesDict.Add(e.NameWithExtension, e);
        }
        finally
        {
            Entries.EndChange();
            IsInEntriesUpdating = false;
        }
    }

    private void AddEntryInternal(Entry entry)
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

        _entriesDict.Add(entry.NameWithExtension, entry);
    }

    private void RemoveEntryInternal(Entry entry)
    {
        Entries.Remove(entry);

        if (entry.IsDirectory)
            --_directoriesCount;
        else
            --_filesCount;

        if (_entriesDict.Remove(entry.NameWithExtension) == false)
            _Logger.Error($"RemoveEntryInternal: {Path}, {entry.NameWithExtension}");
    }

    private void UpdateEntryCompare()
    {
        _entryCompare = EntryComparison.FindEntryCompare(SortMode, SortOrder);
    }

    private void SortEntries()
    {
        var source = Entries.AsSpan();
        var length = source.Length;

        var temp = ArrayPool<Entry>.Shared.Rent(length);
        try
        {
            for (var i = 0; i != length; ++i)
                temp[i] = Entry.Create(source[i]);

            temp.AsSpan().Slice(0, _directoriesCount).Sort(_entryCompare);
            temp.AsSpan().Slice(_directoriesCount, _filesCount).Sort(_entryCompare);

            for (var i = 0; i != length; ++i)
                temp[i].CopyTo(source[i]);
        }
        finally
        {
            ArrayPool<Entry>.Shared.Return(temp);
        }
    }

    public bool IsInEntriesUpdating { get; private set; }

    private Comparison<Entry> _entryCompare = EntryComparison.FindEntryCompare(SortModes.Name, SortOrders.Ascending);

    protected abstract IEnumerable<Entry> EnumerateDirectories();
    protected abstract IEnumerable<Entry> EnumerateFiles();

    private int _directoriesCount;
    private int _filesCount;
    private readonly Dictionary<string, Entry> _entriesDict = new();

    protected readonly ILoggerUseCase _Logger;
    
    public virtual void Dispose()
    {
    }
}