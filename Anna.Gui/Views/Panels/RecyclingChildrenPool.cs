using Anna.Gui.ViewModels;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using System;
using System.Collections.Generic;

namespace Anna.Gui.Views.Panels;

internal class RecyclingChildrenPool
{
    public IDataTemplate? ItemTemplate { get; set; }
    
    private readonly Stack<Control> _directoryPool = new();
    private readonly Stack<Control> _filePool = new();
    
    public Control Rent(EntryViewModel entry)
    {
        _ = ItemTemplate ?? throw new NullReferenceException();

        var pool = entry.IsDirectory ? _directoryPool : _filePool;

        var child = pool.Count != 0
            ? pool.Pop()
            : ItemTemplate.Build(entry) as Control ?? throw new NullReferenceException();

        child.DataContext = entry;
        
        return child;
    }

    public void Return(Control child)
    {
        var entry = child.DataContext as EntryViewModel ?? throw new NullReferenceException();

        var pool = entry.IsDirectory ? _directoryPool : _filePool;

        child.DataContext = null;
        
        pool.Push(child);
    }
}