﻿using Anna.Constants;
using Anna.Foundation;
using Anna.UseCase;
using System.Buffers;

namespace Anna.DomainModel;

public abstract class Directory : NotificationObject, IDisposable
{
    public ObservableCollectionEx<Entry> Entries { get; } = new();
    public readonly object EntitiesUpdatingLockObj = new();
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

    public void SetSortModeAndOrder(SortModes mode, SortOrders order)
    {
        if (mode == _SortMode && order == _SortOrder)
            return;

        _SortMode = mode;
        _SortOrder = order;

        UpdateEntryCompare();
        SortEntries();
    }

    protected Directory(string path, ILoggerUseCase logger)
    {
        _Logger = logger;
        Path = PathStringHelper.Normalize(path);

        UpdateEntries();
    }

    protected void OnCreated(Entry newEntry)
    {
        _Logger.Information($"OnCreated: {Path}, {newEntry.NameWithExtension}");

        AddEntryInternal(newEntry);
    }

    protected void OnChanged(Entry entry)
    {
        _Logger.Information($"OnChanged: {Path}, {entry.NameWithExtension}");

        lock (EntitiesUpdatingLockObj)
        {
            if (_entriesDict.TryGetValue(entry.NameWithExtension, out var target) == false)
            {
                // _Logger.Error($"OnChanged: {Path}, {entry.NameWithExtension}");
                return;
            }

            entry.CopyToWithoutIsSelected(target);
        }
    }

    protected void OnDeleted(string name)
    {
        _Logger.Information($"OnDeleted: {Path}, {name}");

        lock (EntitiesUpdatingLockObj)
        {
            if (_entriesDict.TryGetValue(name, out var target) == false)
            {
                // _Logger.Error($"OnDeleted: {Path}, {name}");
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
                //_Logger.Error($"OnRenamed: {Path}, {oldName}, {newName}");
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
        lock (EntitiesUpdatingLockObj)
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
    }

    private void AddEntryInternal(Entry entry)
    {
        lock (EntitiesUpdatingLockObj)
        {
            if (_entriesDict.TryGetValue(entry.NameWithExtension, out var alreadyExistsEntry))
                RemoveEntryInternal(alreadyExistsEntry);
            
            if (entry.IsDirectory)
            {
                var span = Entries.AsSpan().Slice(0, _directoriesCount);
                var index = SpanHelper.UpperBound(span, entry, _entryCompare);

                Entries.Insert(index, entry);

                ++_directoriesCount;
            }
            else
            {
                var span = Entries.AsSpan().Slice(_directoriesCount, _filesCount);
                var index = SpanHelper.UpperBound(span, entry, _entryCompare);

                Entries.Insert(_directoriesCount + index, entry);

                ++_filesCount;
            }

            _entriesDict.Add(entry.NameWithExtension, entry);

            if (_removedSelectedEntries.ContainsKey(entry.NameWithExtension))
            {
                entry.IsSelected = true;

                _removedSelectedEntries.Remove(entry.NameWithExtension, out _);
            }

            TrimRemovedSelectedEntries();
        }
    }

    private void RemoveEntryInternal(Entry entry)
    {
        lock (EntitiesUpdatingLockObj)
        {
            // todo: Binary search
            var index = Entries.IndexOf(entry);

            if (index == -1)
                throw new IndexOutOfRangeException();

            Entries.RemoveAt(index);

            if (entry.IsSelected)
                _removedSelectedEntries[entry.NameWithExtension] = DateTime.Now;

            TrimRemovedSelectedEntries();

            if (entry.IsDirectory)
                --_directoriesCount;
            else
                --_filesCount;

            // if (_entriesDict.Remove(entry.NameWithExtension) == false)
            //     _Logger.Error($"RemoveEntryInternal: {Path}, {entry.NameWithExtension}");
            _entriesDict.Remove(entry.NameWithExtension);
        }
    }

    private void UpdateEntryCompare()
    {
        _entryCompare = EntryComparison.FindEntryCompare(SortMode, SortOrder);
    }

    private void SortEntries()
    {
        lock (EntitiesUpdatingLockObj)
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
    }

    private void TrimRemovedSelectedEntries()
    {
        lock (EntitiesUpdatingLockObj)
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

    protected abstract IEnumerable<Entry> EnumerateDirectories();
    protected abstract IEnumerable<Entry> EnumerateFiles();

    private Comparison<Entry> _entryCompare = EntryComparison.FindEntryCompare(SortModes.Name, SortOrders.Ascending);
    private int _directoriesCount;
    private int _filesCount;
    private readonly Dictionary<string, Entry> _entriesDict = new();
    private readonly Dictionary<string, DateTime> _removedSelectedEntries = new();

    protected readonly ILoggerUseCase _Logger;

    public virtual void Dispose()
    {
    }
}