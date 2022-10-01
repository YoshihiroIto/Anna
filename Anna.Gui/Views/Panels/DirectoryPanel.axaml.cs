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
using System.Reactive.Disposables;

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

    internal static readonly DirectProperty<DirectoryPanel, DirectoryPanelLayout> LayoutProperty =
        AvaloniaProperty.RegisterDirect<DirectoryPanel, DirectoryPanelLayout>(nameof(DirectoryPanelLayout),
            o => o.Layout);

    internal static readonly DirectProperty<DirectoryPanel, IntSize> ItemCellSizeProperty =
        AvaloniaProperty.RegisterDirect<DirectoryPanel, IntSize>(nameof(ItemCellSize), o => o.ItemCellSize);

    internal DirectoryPanelLayout Layout
    {
        get => _Layout;
        private set => SetAndRaise(LayoutProperty, ref _Layout, value);
    }

    internal IntSize ItemCellSize
    {
        get => _ItemCellSize;
        private set => SetAndRaise(ItemCellSizeProperty, ref _ItemCellSize, value);
    }

    private DirectoryPanelLayout _Layout = new();
    private IntSize _ItemCellSize;

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
        ItemCellSize = new IntSize(
            (int)(Bounds.Width / Layout.ItemWidth),
            (int)(Bounds.Height / Layout.ItemHeight)
        );
    }
}

internal class EntriesControl : Control
{
    private readonly CompositeDisposable _entriesObservers = new();
    private readonly Dictionary<EntryViewModel, IControl> _childrenControls = new();
    private readonly Stack<IControl> _recyclingChildrenPool = new();
    
    private IDataTemplate? _itemTemplate;
    private DirectoryPanelLayout? _layout;

    public EntriesControl()
    {
        PropertyChanged += (_, e) =>
        {
            if (e.Property == DataContextProperty)
            {
                if (e.NewValue is DirectoryPanelViewModel viewModel)
                {
                    UpdateEntriesObservers(viewModel);
                    ResetChildren(viewModel.Entries);
                }
            }
            else if (e.Property == ParentProperty)
            {
                if (e.NewValue is DirectoryPanel parent)
                {
                    _itemTemplate = parent.ItemTemplate ?? throw new NullReferenceException();
                    _layout = parent.Layout ?? throw new NullReferenceException();
                }
            }
        };

        Unloaded += (_, _) => _entriesObservers.Dispose();
    }

    private void UpdateEntriesObservers(DirectoryPanelViewModel viewModel)
    {
        _entriesObservers.Clear();

        viewModel.Entries.ObserveResetChanged()
            .Subscribe(_ => ResetChildren(viewModel.Entries))
            .AddTo(_entriesObservers);

        viewModel.Entries.ObserveAddChangedItems()
            .Subscribe(AddChildren)
            .AddTo(_entriesObservers);

        viewModel.Entries.ObserveRemoveChangedItems()
            .Subscribe(RemoveChildren)
            .AddTo(_entriesObservers);
    }

    private void AddChildren(IEnumerable<EntryViewModel> entries)
    {
        foreach (var entry in entries)
        {
            var child = CreateOrRentChild(entry);

            VisualChildren.Add(child);
            LogicalChildren.Add(child);
            _childrenControls.Add(entry, child);
        }

        InvalidateArrange();
    }

    private void RemoveChildren(IEnumerable<EntryViewModel> entries)
    {
        foreach (var entry in entries)
        {
            if (_childrenControls.TryGetValue(entry, out var child) == false)
                throw new InvalidOperationException();

            VisualChildren.Remove(child);
            LogicalChildren.Remove(child);
            _childrenControls.Remove(entry);

            ReturnChild(child);
        }

        InvalidateArrange();
    }

    private void ResetChildren(IEnumerable<EntryViewModel> entries)
    {
        foreach (var child in _childrenControls.Values)
            ReturnChild(child);

        VisualChildren.Clear();
        LogicalChildren.Clear();
        _childrenControls.Clear();

        AddChildren(entries);
    }

    private IControl CreateOrRentChild(EntryViewModel entry)
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
        _recyclingChildrenPool.Push(child);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        _ = _layout ?? throw new NullReferenceException();

        if (DataContext is not DirectoryPanelViewModel viewModel)
            throw new InvalidOperationException();

        var itemSize = new Size(_layout.ItemWidth, _layout.ItemHeight);
        var viewHeight = finalSize.Height;

        var x = 0.0;
        var y = 0.0;

        foreach (var entry in viewModel.Entries)
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