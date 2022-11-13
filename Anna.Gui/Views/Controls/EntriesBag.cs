using Anna.Gui.ViewModels;
using Anna.Gui.Views.Panels;
using Avalonia;
using Avalonia.Controls;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Disposables;

namespace Anna.Gui.Views.Controls;

internal sealed class EntriesBag : Control
{
    private readonly CompositeDisposable _entriesObservers = new();
    private readonly Dictionary<EntryViewModel, Control> _childrenControls = new();
    private readonly RecyclingChildrenPool _recyclingChildrenPool = new();

    private FolderPanel? _parent;
    private FolderPanelLayout? _layout;
    private IReadOnlyList<EntryViewModel>? _currentEntries;

    public EntriesBag()
    {
        PropertyChanged += (_, e) =>
        {
            if (e.Property == DataContextProperty)
            {
                if (e.NewValue is FolderPanelViewModel viewModel)
                {
                    UpdateEntriesObservers(viewModel);
                    UpdateChildren(viewModel.Entries);
                }
            }
            else if (e.Property == ParentProperty)
            {
                if (e.OldValue is FolderPanel oldParent)
                {
                    oldParent.PageIndexChanged -= OnPageIndexChanged;
                    oldParent.ItemCellSizeChanged -= OnItemCellSizeChanged;
                }

                if (e.NewValue is FolderPanel newParent)
                {
                    newParent.PageIndexChanged += OnPageIndexChanged;
                    newParent.ItemCellSizeChanged += OnItemCellSizeChanged;

                    _parent = newParent;
                    _layout = newParent.Layout ?? throw new NullReferenceException();
                    _recyclingChildrenPool.ItemTemplate = newParent.ItemTemplate ?? throw new NullReferenceException();
                }
            }
        };

        Unloaded += (_, _) => _entriesObservers.Dispose();
    }

    private void OnPageIndexChanged(object? sender, EventArgs e)
    {
        UpdateChildren();
    }

    private void OnItemCellSizeChanged(object? sender, EventArgs e)
    {
        UpdateChildren();
    }

    private void UpdateEntriesObservers(FolderPanelViewModel viewModel)
    {
        _entriesObservers.Clear();

        viewModel.Entries.CollectionChangedAsObservable()
            .Subscribe(_ => UpdateChildren(viewModel.Entries))
            .AddTo(_entriesObservers);
    }

    private (int StartIndex, int EndIndex) CurrentPageRange(IReadOnlyCollection<EntryViewModel> entries)
    {
        _ = _parent ?? throw new NullReferenceException();

        var entryCountPerPage = _parent.ItemCellSize.Width * _parent.ItemCellSize.Height;

        var start = entryCountPerPage * _parent.PageIndex;
        var end = Math.Min(entries.Count, entryCountPerPage * (_parent.PageIndex + 1) + _parent.ItemCellSize.Height);

        return (start, end);
    }

    private void UpdateChildren()
    {
        if (DataContext is not FolderPanelViewModel viewModel)
            throw new InvalidOperationException();

        UpdateChildren(viewModel.Entries);
    }

    private void UpdateChildren(IReadOnlyList<EntryViewModel> entries)
    {
        _currentEntries = entries;

        var pageRange = CurrentPageRange(entries);
        var deletionTargets = new DeletionTargets(_childrenControls);
        var entriesToAdd = new List<EntryViewModel>(pageRange.EndIndex - pageRange.StartIndex);

        for (var i = pageRange.StartIndex; i < pageRange.EndIndex; ++i)
        {
            var entry = entries[i];

            if (_childrenControls.ContainsKey(entry))
            {
                if (deletionTargets.Remove(entry) == false)
                    Debug.Assert(false);

                continue;
            }

            entriesToAdd.Add(entry);
        }

        List<Control>? childrenToAdd = null;
        {
            foreach (var entry in entriesToAdd)
            {
                var r = _recyclingChildrenPool.Rent(entry, deletionTargets);
                if (r.IsNew)
                {
                    childrenToAdd ??= new List<Control>();
                    childrenToAdd.Add(r.Child);
                }
                else
                {
                    if (r.OldEntry is not null)
                        _childrenControls.Remove(r.OldEntry);
                }

                _childrenControls[entry] = r.Child;
            }
        }

        if (childrenToAdd is not null)
        {
            VisualChildren.AddRange(childrenToAdd);
            LogicalChildren.AddRange(childrenToAdd);
        }

        foreach (var (entry, child) in deletionTargets.AllTargets)
        {
            _childrenControls.Remove(entry);
            _recyclingChildrenPool.Return(child);
        }

        InvalidateArrange();
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        _ = _layout ?? throw new NullReferenceException();
        _ = _currentEntries ?? throw new NullReferenceException();

        var itemSize = new Size(_layout.ItemWidth, _layout.ItemHeight);
        var viewHeight = finalSize.Height;

        var x = 0.0;
        var y = 0.0;

        var pageRange = CurrentPageRange(_currentEntries);

        for (var i = pageRange.StartIndex; i < pageRange.EndIndex; ++i)
        {
            var entry = _currentEntries[i];

            if (_childrenControls.TryGetValue(entry, out var child) == false)
                throw new InvalidOperationException();

            if (y + itemSize.Height >= viewHeight)
            {
                x += itemSize.Width;
                y = 0;
            }

            child.Arrange(new Rect(new Point(x, y), itemSize));
            y += itemSize.Height;
        }

        return finalSize;
    }
}