using Anna.Constants;
using Anna.DomainModel;
using Anna.Foundation;
using Anna.Gui.Foundations;
using Anna.Gui.Messaging;
using Anna.Gui.ShortcutKey;
using Anna.Gui.ViewModels;
using Anna.Service;
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

public sealed partial class FolderPanel : UserControl, IFolderPanelShortcutKeyReceiver
{
    public static readonly StyledProperty<IDataTemplate?> ItemTemplateProperty =
        AvaloniaProperty.Register<FolderPanel, IDataTemplate?>(nameof(ItemTemplate));

    public IDataTemplate? ItemTemplate
    {
        get => GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    public static readonly StyledProperty<int> SelectedIndexProperty =
        AvaloniaProperty.Register<FolderPanel, int>(nameof(SelectedIndex));

    public int SelectedIndex
    {
        get => GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    public static readonly StyledProperty<int> PageIndexProperty =
        AvaloniaProperty.Register<FolderPanel, int>(nameof(PageIndex));

    public int PageIndex
    {
        get => GetValue(PageIndexProperty);
        set => SetValue(PageIndexProperty, value);
    }

    internal event EventHandler? PageIndexChanged;
    internal event EventHandler? ItemCellSizeChanged;

    internal static readonly DirectProperty<FolderPanel, FolderPanelLayout> LayoutProperty =
        AvaloniaProperty.RegisterDirect<FolderPanel, FolderPanelLayout>(nameof(FolderPanelLayout),
            o => o.Layout);

    internal static readonly DirectProperty<FolderPanel, IntSize> ItemCellSizeProperty =
        AvaloniaProperty.RegisterDirect<FolderPanel, IntSize>(nameof(ItemCellSize), o => o.ItemCellSize);

    internal FolderPanelLayout Layout { get; } = new();

    internal IntSize ItemCellSize
    {
        get => _ItemCellSize;
        private set => SetAndRaise(ItemCellSizeProperty, ref _ItemCellSize, value);
    }

    private IntSize _ItemCellSize;

    static FolderPanel()
    {
        SelectedIndexProperty.Changed.Subscribe(e => (e.Sender as FolderPanel)?.UpdatePageIndex(true));
    }

    public FolderPanel()
    {
        InitializeComponent();
        
        LayoutUpdated += (_, _) => UpdateItemCellSize();
        
        PropertyChanged += (_, e) =>
        {
            if (e.Property == FontFamilyProperty)
                Layout.FontFamily = FontFamily;
            else if (e.Property == FontSizeProperty)
                Layout.FontSize = FontSize;
        };
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    Window IShortcutKeyReceiver.Owner => ControlHelper.FindOwnerWindow(this);
    InteractionMessenger IShortcutKeyReceiver.Messenger => ViewModel.Messenger;
    Folder IFolderPanelShortcutKeyReceiver.Folder => ViewModel.Model;
    Entry IFolderPanelShortcutKeyReceiver.CurrentEntry => ViewModel.CursorEntry.Value?.Model ?? throw new InvalidOperationException();
    Entry[] IFolderPanelShortcutKeyReceiver.TargetEntries => ViewModel.CollectTargetEntries();
    IBackgroundWorker IFolderPanelShortcutKeyReceiver.BackgroundWorker => ViewModel.Model.BackgroundWorker;
    
    void IFolderPanelShortcutKeyReceiver.MoveCursor(Directions dir) => ViewModel.MoveCursor(dir);
    void IFolderPanelShortcutKeyReceiver.ToggleSelectionCursorEntry(bool isMoveDown) => ViewModel.ToggleSelectionCursorEntry(isMoveDown);
    
    private FolderPanelViewModel ViewModel =>
        DataContext as FolderPanelViewModel ?? throw new NotSupportedException();

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

internal sealed class EntriesControl : Control
{
    private readonly CompositeDisposable _entriesObservers = new();
    private readonly Dictionary<EntryViewModel, Control> _childrenControls = new();
    private readonly RecyclingChildrenPool _recyclingChildrenPool = new();

    private FolderPanel? _parent;
    private FolderPanelLayout? _layout;
    private IReadOnlyList<EntryViewModel>? _currentEntries;

    public EntriesControl()
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