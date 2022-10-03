﻿using Anna.Gui.ViewModels;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Anna.Gui.Views.Panels;

internal class RecyclingChildrenPool
{
    public IDataTemplate? ItemTemplate { get; set; }

    public (Control Child, EntryViewModel? OldEntry) Rent(
        EntryViewModel entry,
        DeletionTargets deletionTargets)
    {
        // Use if found from the deletion target.
        var fromDeleteTargets = deletionTargets.PickFromTargets(entry);
        if (fromDeleteTargets != default)
        {
            fromDeleteTargets.Child.DataContext = entry;

            return (fromDeleteTargets.Child, fromDeleteTargets.Entry);
        }

        // Use new child or from the recycling pool
        {
            _ = ItemTemplate ?? throw new NullReferenceException();

            var pool = FindPool(entry);

            var child = pool.Count != 0
                ? pool.Pop()
                : ItemTemplate.Build(entry) as Control ?? throw new NullReferenceException();

            child.DataContext = entry;

            return (child, null);
        }
    }

    public void Return(Control child)
    {
        var entry = child.DataContext as EntryViewModel ?? throw new NullReferenceException();
        child.DataContext = null;

        var pool = FindPool(entry);
        pool.Push(child);
    }

    private Stack<Control> FindPool(EntryViewModel entry)
    {
        return entry.IsDirectory ? _directoryPool : _filePool;
    }

    private readonly Stack<Control> _directoryPool = new();
    private readonly Stack<Control> _filePool = new();
}

internal class DeletionTargets
{
    public IEnumerable<(EntryViewModel Entry, Control Child)> AllTargets
    {
        get
        {
            foreach (var c in _directoryTargets)
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
        return entry.IsDirectory ? _directoryTargets : _fileTargets;
    }

    private readonly Dictionary<EntryViewModel, Control> _fileTargets = new();
    private readonly Dictionary<EntryViewModel, Control> _directoryTargets = new();
}