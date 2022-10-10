using Anna.Gui.ViewModels;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Anna.Gui.Views.Panels;

internal sealed class RecyclingChildrenPool
{
    public IDataTemplate? ItemTemplate { get; set; }
    
    private readonly Stack<Control> _folderPool = new();
    private readonly Stack<Control> _filePool = new();

    public (Control Child, EntryViewModel? OldEntry, bool IsNew) Rent(
        EntryViewModel entry,
        DeletionTargets deletionTargets)
    {
        // Use if found from the deletion target.
        var fromDeleteTargets = deletionTargets.PickFromTargets(entry);
        if (fromDeleteTargets != default)
        {
            fromDeleteTargets.Child.DataContext = entry;
            return (fromDeleteTargets.Child, fromDeleteTargets.Entry, false);
        }

        // Use new child or from the recycling pool
        {
            _ = ItemTemplate ?? throw new NullReferenceException();

            var pool = FindPool(entry);

            if (pool.Count == 0)
            {
                var child = ItemTemplate.Build(entry) as Control ?? throw new NullReferenceException();

                child.DataContext = entry;
                child.IsVisible = true;

                return (child, null, true);
            }
            else
            {
                var child = pool.Pop();

                child.DataContext = entry;
                child.IsVisible = true;

                return (child, null, false);
            }
        }
    }

    public void Return(Control child)
    {
        var entry = child.DataContext as EntryViewModel ?? throw new NullReferenceException();
        
        child.IsVisible = false;
        child.DataContext = null;

        var pool = FindPool(entry);
        pool.Push(child);
    }

    private Stack<Control> FindPool(EntryViewModel entry)
    {
        return entry.IsFolder ? _folderPool : _filePool;
    }
}

internal sealed class DeletionTargets
{
    private readonly Dictionary<EntryViewModel, Control> _fileTargets = new();
    private readonly Dictionary<EntryViewModel, Control> _folderTargets = new();
    
    public IEnumerable<(EntryViewModel Entry, Control Child)> AllTargets
    {
        get
        {
            foreach (var c in _folderTargets)
                yield return (c.Key, c.Value);

            foreach (var c in _fileTargets)
                yield return (c.Key, c.Value);
        }
    }

    public DeletionTargets(Dictionary<EntryViewModel, Control> childrenControls)
    {
        foreach (var c in childrenControls)
            FindTargets(c.Key).Add(c.Key, c.Value);
    }

    public bool Remove(EntryViewModel entry)
    {
        return FindTargets(entry).Remove(entry);
    }

    public (EntryViewModel Entry, Control Child) PickFromTargets(EntryViewModel entry)
    {
        var targets = FindTargets(entry);
        if (targets.Count == 0)
            return default;

        var r = targets.First();

        Remove(r.Key);
        return (r.Key, r.Value);
    }

    private Dictionary<EntryViewModel, Control> FindTargets(EntryViewModel entry)
    {
        return entry.IsFolder ? _folderTargets : _fileTargets;
    }
}