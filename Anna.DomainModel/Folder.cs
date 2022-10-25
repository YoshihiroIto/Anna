using Anna.Constants;
using Anna.Foundation;
using Anna.Service.Services;
using Anna.Service.Workers;
using Reactive.Bindings.Extensions;
using System.Diagnostics;
using IServiceProvider=Anna.Service.IServiceProvider;

namespace Anna.DomainModel;

[DebuggerDisplay("Path={Path}")]
public abstract class Folder : DisposableNotificationObject
{
    public ObservableCollectionEx<Entry> Entries { get; } = new();
    public readonly object EntriesUpdatingLockObj = new();

    public EventHandler? EntrySizeChanged;

    public abstract bool IsRoot { get; }
    public bool IsInEntriesUpdating { get; private set; }

    #region Path

    private string _Path = "";

    public string Path
    {
        get => _Path;
        set
        {
            if (SetProperty(ref _Path, value) == false)
                return;

            UpdateEntries();
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

    protected abstract IEnumerable<Entry> EnumerateDirectories();
    protected abstract IEnumerable<Entry> EnumerateFiles();

    private Comparison<Entry> _entryCompare = EntryComparison.FindEntryCompare(SortModes.Name, SortOrders.Ascending);
    private int _foldersCount;
    private int _filesCount;
    private readonly Dictionary<string, Entry> _entriesDict = new();
    private readonly Dictionary<string, DateTime> _removedSelectedEntries = new();

    public readonly IBackgroundWorker BackgroundWorker;

    public abstract Stream OpenRead(string path);
    public abstract Task<byte[]> ReadAllAsync(string path);

    protected Folder(string path, IServiceProvider dic)
        : base(dic)
    {
        BackgroundWorker = dic.GetInstance<IBackgroundWorker>();// create new
        (BackgroundWorker as IDisposable)?.AddTo(Trash);
        
        Path = PathStringHelper.Normalize(path);
    }

    public void SetSortModeAndOrder(SortModes mode, SortOrders order)
    {
        if (mode == _SortMode && order == _SortOrder)
            return;

        _SortMode = mode;
        _SortOrder = order;

        UpdateEntryCompare();
        SortEntries();
    }

    protected void OnCreated(Entry newEntry)
    {
        Dic.GetInstance<ILoggerService>().Information($"OnCreated: {Path}, {newEntry.NameWithExtension}");

        AddEntryInternal(newEntry);
    }

    protected void OnChanged(Entry entry)
    {
        Dic.GetInstance<ILoggerService>().Information($"OnChanged: {Path}, {entry.NameWithExtension}");

        bool isSizeChanged;

        lock (EntriesUpdatingLockObj)
        {
            if (_entriesDict.TryGetValue(entry.NameWithExtension, out var target) == false)
                return;

            isSizeChanged = entry.Size != target.Size;

            entry.CopyToWithoutIsSelected(target);
        }

        if (isSizeChanged)
            EntrySizeChanged?.Invoke(this, EventArgs.Empty);
    }

    protected void OnDeleted(string name)
    {
        Dic.GetInstance<ILoggerService>().Information($"OnDeleted: {Path}, {name}");

        lock (EntriesUpdatingLockObj)
        {
            if (_entriesDict.TryGetValue(name, out var target) == false)
                return;

            RemoveEntryInternal(target);
        }
    }

    protected void OnRenamed(string oldName, string newName)
    {
        Dic.GetInstance<ILoggerService>().Information($"OnRenamed: {Path}, {oldName}, {newName}");

        lock (EntriesUpdatingLockObj)
        {
            if (_entriesDict.TryGetValue(oldName, out var target) == false)
                return;

            RemoveEntryInternal(target);

            var newEntry = Entry.CreateFrom(target);
            newEntry.SetName(newName, true);

            AddEntryInternal(newEntry);
        }
    }

    private void UpdateEntries()
    {
        try
        {
            lock (EntriesUpdatingLockObj)
            {
                IsInEntriesUpdating = true;
                Entries.BeginChange();

                Entries.Clear();

                Entries.AddRange(EnumerateDirectories());
                _foldersCount = Entries.Count;

                Entries.AddRange(EnumerateFiles());
                _filesCount = Entries.Count - _foldersCount;

                SortEntries();

                //
                _entriesDict.Clear();
                foreach (var e in Entries)
                    _entriesDict.Add(e.NameWithExtension, e);
            }
        }
        finally
        {
            Entries.EndChange();
            IsInEntriesUpdating = false;
        }
    }

    private void AddEntryInternal(Entry entry)
    {
        lock (EntriesUpdatingLockObj)
        {
            TrimRemovedSelectedEntries();
        
            if (_entriesDict.TryGetValue(entry.NameWithExtension, out var alreadyExistsEntry))
                RemoveEntryInternal(alreadyExistsEntry);

            if (entry.IsFolder)
            {
                var span = Entries.AsSpan().Slice(0, _foldersCount);
                var index = SpanHelper.UpperBound(span, entry, _entryCompare);

                Entries.Insert(index, entry);

                ++_foldersCount;
            }
            else
            {
                var span = Entries.AsSpan().Slice(_foldersCount, _filesCount);
                var index = SpanHelper.UpperBound(span, entry, _entryCompare);

                Entries.Insert(_foldersCount + index, entry);

                ++_filesCount;
            }

            _entriesDict.Add(entry.NameWithExtension, entry);

            if (_removedSelectedEntries.ContainsKey(entry.NameWithExtension))
            {
                entry.IsSelected = true;

                _removedSelectedEntries.Remove(entry.NameWithExtension, out _);
            }
        }
    }

    private void RemoveEntryInternal(Entry entry)
    {
        lock (EntriesUpdatingLockObj)
        {
            TrimRemovedSelectedEntries();
        
            // todo: Binary search
            var index = Entries.IndexOf(entry);

            if (index == -1)
                throw new IndexOutOfRangeException();

            Entries.RemoveAt(index);

            if (entry.IsSelected)
                _removedSelectedEntries[entry.NameWithExtension] = DateTime.Now;

            if (entry.IsFolder)
                --_foldersCount;
            else
                --_filesCount;

            _entriesDict.Remove(entry.NameWithExtension);
        }
    }

    private void UpdateEntryCompare()
    {
        _entryCompare = EntryComparison.FindEntryCompare(SortMode, SortOrder);
    }

    private void SortEntries()
    {
        try
        {
            lock (EntriesUpdatingLockObj)
            {
                Entries.BeginChange();

                Entries.AsSpan().Slice(0, _foldersCount).Sort(_entryCompare);
                Entries.AsSpan().Slice(_foldersCount, _filesCount).Sort(_entryCompare);
            }
        }
        finally
        {
            Entries.EndChange();
        }
    }

    private void TrimRemovedSelectedEntries()
    {
        lock (EntriesUpdatingLockObj)
        {
            var now = DateTime.Now;
            foreach (var name in _removedSelectedEntries.Keys)
            {
                var dateTime = _removedSelectedEntries[name];

                if (now - dateTime >= TimeSpan.FromMilliseconds(50))
                    _removedSelectedEntries.Remove(name);
            }
        }
    }
}