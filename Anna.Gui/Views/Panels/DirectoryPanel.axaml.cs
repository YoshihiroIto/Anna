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
using System.Linq;
using System.Reactive.Disposables;
using System.Runtime.InteropServices;

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
    private readonly Dictionary<EntryViewModel, IControl> _childrenControls = new();
    private readonly Stack<IControl> _recyclingChildrenPool = new();

    private DirectoryPanel? _parent;
    private IDataTemplate? _itemTemplate;
    private DirectoryPanelLayout? _layout;

    private readonly List<EntryViewModel> _pageEntries = new();
    
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
                    _itemTemplate = newParent.ItemTemplate ?? throw new NullReferenceException();
                    _layout = newParent.Layout ?? throw new NullReferenceException();
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
        var end = Math.Min(entries.Count, entryCountPerPage * (_parent.PageIndex + 1));

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
        var deleteTargets = _childrenControls.ToDictionary(x => x.Key, y => y.Value);

        var range = CurrentPageRange(entries);
        _pageEntries.Clear();

        for (var i = range.StartIndex; i < range.EndIndex; ++i)
        {
            var entry = entries[i];
            _pageEntries.Add(entry);

            if (_childrenControls.ContainsKey(entry))
            {
                if (deleteTargets.Remove(entry) == false)
                    Debug.Assert(false);

                continue;
            }

            var child = RentChild(entry);

            VisualChildren.Add(child);
            LogicalChildren.Add(child);
            _childrenControls.Add(entry, child);
        }

        foreach (var (entry, child) in deleteTargets)
        {
            VisualChildren.Remove(child);
            LogicalChildren.Remove(child);
            _childrenControls.Remove(entry);
            ReturnChild(child);
        }

        InvalidateArrange();
    }

    private IControl RentChild(EntryViewModel entry)
    {
        _ = _itemTemplate ?? throw new NullReferenceException();

        var child = _recyclingChildrenPool.Count != 0
            ? _recyclingChildrenPool.Pop()
            : _itemTemplate.Build(entry) ?? throw new NullReferenceException();

        child.DataContext = entry;
        return child;
    }

    private void ReturnChild(IControl child)
    {
        child.DataContext = null;
        _recyclingChildrenPool.Push(child);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        _ = _layout ?? throw new NullReferenceException();

        var itemSize = new Size(_layout.ItemWidth, _layout.ItemHeight);
        var viewHeight = finalSize.Height;

        var x = 0.0;
        var y = 0.0;

        foreach (var entry in CollectionsMarshal.AsSpan(_pageEntries))
        {
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