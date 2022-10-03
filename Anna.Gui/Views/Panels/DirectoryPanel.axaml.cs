using Anna.DomainModel;
using Anna.Foundation;
using Anna.Gui.Foundations;
using Anna.Gui.Interfaces;
using Anna.Gui.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Disposables;
using Entry=Anna.DomainModel.Entry;

namespace Anna.Gui.Views.Panels;

public partial class DirectoryPanel : UserControl, IShortcutKeyReceiver
{
    public static readonly StyledProperty<IDataTemplate?> ItemTemplateProperty =
        AvaloniaProperty.Register<DirectoryPanel, IDataTemplate?>(nameof(ItemTemplate));

    public IDataTemplate? ItemTemplate
    {
        get => GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    public static readonly StyledProperty<int> SelectedIndexProperty =
        AvaloniaProperty.Register<DirectoryPanel, int>(nameof(SelectedIndex));

    public int SelectedIndex
    {
        get => GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    public static readonly StyledProperty<int> PageIndexProperty =
        AvaloniaProperty.Register<DirectoryPanel, int>(nameof(PageIndex));

    public int PageIndex
    {
        get => GetValue(PageIndexProperty);
        set => SetValue(PageIndexProperty, value);
    }

    internal event EventHandler? PageIndexChanged;
    internal event EventHandler? ItemCellSizeChanged;

    internal static readonly DirectProperty<DirectoryPanel, DirectoryPanelLayout> LayoutProperty =
        AvaloniaProperty.RegisterDirect<DirectoryPanel, DirectoryPanelLayout>(nameof(DirectoryPanelLayout),
            o => o.Layout);

    internal static readonly DirectProperty<DirectoryPanel, IntSize> ItemCellSizeProperty =
        AvaloniaProperty.RegisterDirect<DirectoryPanel, IntSize>(nameof(ItemCellSize), o => o.ItemCellSize);

    internal DirectoryPanelLayout Layout { get; } = new();

    internal IntSize ItemCellSize
    {
        get => _ItemCellSize;
        private set => SetAndRaise(ItemCellSizeProperty, ref _ItemCellSize, value);
    }

    private IntSize _ItemCellSize;

    static DirectoryPanel()
    {
        SelectedIndexProperty.Changed.Subscribe(e => (e.Sender as DirectoryPanel)?.UpdatePageIndex(true));
    }

    public DirectoryPanel()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);

        LayoutUpdated += (_, _) => UpdateItemCellSize();
    }

    public Window Owner => ControlHelper.FindOwnerWindow(this);

    public Directory Directory => DirectoryPanelViewModel.Model;

    public DirectoryPanelViewModel DirectoryPanelViewModel =>
        DataContext as DirectoryPanelViewModel ?? throw new NotSupportedException();

    public Entry[] CollectTargetEntries()
        => DirectoryPanelViewModel.CollectTargetEntries();

    private void UpdateItemCellSize()
    {
        var changed = false;

        var s = new IntSize((int)(Bounds.Width / Layout.ItemWidth), (int)(Bounds.Height / Layout.ItemHeight));

        if (s != ItemCellSize)
        {
            ItemCellSize = s;
            changed = true;
        }

        if (UpdatePageIndex(false))
            changed = true;

        if (changed)
            ItemCellSizeChanged?.Invoke(this, EventArgs.Empty);
    }

    private bool UpdatePageIndex(bool isRaiseEventIfChanged)
    {
        var p = SelectedIndex / (ItemCellSize.Width * ItemCellSize.Height);

        if (PageIndex == p)
            return false;

        PageIndex = p;

        if (isRaiseEventIfChanged)
            PageIndexChanged?.Invoke(this, EventArgs.Empty);

        return true;
    }
}

internal class EntriesControl : Control
{
    private readonly CompositeDisposable _entriesObservers = new();
    private readonly Dictionary<EntryViewModel, Control> _childrenControls = new();
    private readonly RecyclingChildrenPool _recyclingChildrenPool = new();

    private DirectoryPanel? _parent;
    private DirectoryPanelLayout? _layout;
    private IReadOnlyList<EntryViewModel>? _currentEntries;

    public EntriesControl()
    {
        PropertyChanged += (_, e) =>
        {
            if (e.Property == DataContextProperty)
            {
                if (e.NewValue is DirectoryPanelViewModel viewModel)
                {
                    UpdateEntriesObservers(viewModel);
                    UpdateChildren(viewModel.Entries);
                }
            }
            else if (e.Property == ParentProperty)
            {
                if (e.OldValue is DirectoryPanel oldParent)
                {
                    oldParent.PageIndexChanged -= OnPageIndexChanged;
                    oldParent.ItemCellSizeChanged -= OnItemCellSizeChanged;
                }

                if (e.NewValue is DirectoryPanel newParent)
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

    private void UpdateEntriesObservers(DirectoryPanelViewModel viewModel)
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
        if (DataContext is not DirectoryPanelViewModel viewModel)
            throw new InvalidOperationException();

        UpdateChildren(viewModel.Entries);
    }

    private void UpdateChildren(IReadOnlyList<EntryViewModel> entries)
    {
        _currentEntries = entries;
        
        var pageRange = CurrentPageRange(entries);
        var deletionTargets = new DeletionTargets(_childrenControls);
        var entitiesToAdd = new List<EntryViewModel>(pageRange.EndIndex - pageRange.StartIndex);

        for (var i = pageRange.StartIndex; i < pageRange.EndIndex; ++i)
        {
            var entry = entries[i];

            if (_childrenControls.ContainsKey(entry))
            {
                if (deletionTargets.Remove(entry) == false)
                    Debug.Assert(false);

                continue;
            }

            entitiesToAdd.Add(entry);
        }

        List<Control>? childrenToAdd = null;
        {
            foreach (var entry in entitiesToAdd)
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