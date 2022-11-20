using Anna.Constants;
using Anna.DomainModel;
using Anna.Foundation;
using Anna.Gui.Messaging;
using Anna.Gui.Interactions.Drop;
using Anna.Gui.Interactions.Hotkey;
using Anna.Gui.ViewModels;
using Anna.Service.Interfaces;
using Anna.Service.Workers;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using Entry=Anna.DomainModel.Entry;

namespace Anna.Gui.Views.Panels;

public sealed partial class FolderPanel : UserControl, IFolderPanelHotkeyReceiver, IFileDropReceiver
{
    internal static readonly StyledProperty<IDataTemplate?> ItemTemplateProperty =
        AvaloniaProperty.Register<FolderPanel, IDataTemplate?>(nameof(ItemTemplate));

    internal static readonly StyledProperty<int> SelectedEntryIndexProperty =
        AvaloniaProperty.Register<FolderPanel, int>(nameof(SelectedEntryIndex));

    internal static readonly StyledProperty<int> PageIndexProperty =
        AvaloniaProperty.Register<FolderPanel, int>(nameof(PageIndex));

    internal static readonly DirectProperty<FolderPanel, FolderPanelLayout> LayoutProperty =
        AvaloniaProperty.RegisterDirect<FolderPanel, FolderPanelLayout>(nameof(FolderPanelLayout), o => o.Layout);

    internal static readonly DirectProperty<FolderPanel, IntSize> ItemCellSizeProperty =
        AvaloniaProperty.RegisterDirect<FolderPanel, IntSize>(nameof(ItemCellSize), o => o.ItemCellSize);

    internal IDataTemplate? ItemTemplate
    {
        get => GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    internal int SelectedEntryIndex
    {
        get => GetValue(SelectedEntryIndexProperty);
        set => SetValue(SelectedEntryIndexProperty, value);
    }

    internal int PageIndex
    {
        get => GetValue(PageIndexProperty);
        set => SetValue(PageIndexProperty, value);
    }

    internal event EventHandler? PageIndexChanged;
    internal event EventHandler? ItemCellSizeChanged;

    internal IntSize ItemCellSize
    {
        get => _ItemCellSize;
        private set => SetAndRaise(ItemCellSizeProperty, ref _ItemCellSize, value);
    }

    private IntSize _ItemCellSize;

    internal FolderPanelViewModel ViewModel => _viewModel ?? throw new InvalidOperationException();
    private FolderPanelViewModel? _viewModel;

    internal FolderPanelLayout Layout { get; } = new();

    public Folder Folder => ViewModel.Model;
    public Messenger Messenger => ViewModel.Messenger;
    public Entry CurrentEntry => ViewModel.CursorEntry.Value?.Model.Entry ?? throw new InvalidOperationException();
    public IBackgroundWorker BackgroundWorker => ViewModel.Model.BackgroundWorker;
    public IEnumerable<IEntry> CollectTargetEntries() => ViewModel.CollectTargetEntries();

    public void MoveCursor(Directions dir) => ViewModel.MoveCursor(dir);
    public void ToggleSelectionCursorEntry(bool isMoveDown) => ViewModel.ToggleSelectionCursorEntry(isMoveDown);
    public void SetListMode(uint index) => ViewModel.SetListMode(index);

    static FolderPanel()
    {
        SelectedEntryIndexProperty.Changed.Subscribe(e => (e.Sender as FolderPanel)?.UpdatePageIndex(true));
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

        PropertyChanged += (_, e) =>
        {
            if (e.Property == DataContextProperty)
            {
                _viewModel = DataContext as FolderPanelViewModel ?? throw new NotSupportedException();
                Layout.ViewModel = _viewModel;
            }
        };
    }

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

    private bool UpdatePageIndex(bool isRaiseEvenWhenChanged)
    {
        var newPageIndex = SelectedEntryIndex / Math.Max(1, ItemCellSize.Width * ItemCellSize.Height);

        if (PageIndex == newPageIndex)
            return false;

        PageIndex = newPageIndex;

        if (isRaiseEvenWhenChanged)
            PageIndexChanged?.Invoke(this, EventArgs.Empty);

        return true;
    }

    private void EntriesBag_OnEntryPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        _ = sender ?? throw new NullReferenceException();

        var control = (Control)sender;
        var entryViewModel = control.DataContext as EntryViewModel ?? throw new NullReferenceException();

        var index = ViewModel.Model.Entries.IndexOf(entryViewModel.Model.Entry);
        ViewModel.CursorIndex.Value = index;
    }

    private async void EntriesBag_OnEntryPointerMoved(object? sender, PointerEventArgs e)
    {
        var isLeftButtonPressed = e.GetCurrentPoint(this).Properties.IsLeftButtonPressed;
        var isRightButtonPressed = e.GetCurrentPoint(this).Properties.IsRightButtonPressed;

        if ((isLeftButtonPressed || isRightButtonPressed) == false)
            return;

        var targets = ViewModel.CollectTargetEntries().Select(x => x.Path).ToArray();

        var dragData = new DataObject();
        dragData.Set(DataFormats.FileNames, targets);

        await DragDrop.DoDragDrop(e,
            dragData,
            isLeftButtonPressed ? DragDropEffects.Copy | DragDropEffects.Link :
            isRightButtonPressed ? DragDropEffects.Copy | DragDropEffects.Move | DragDropEffects.Link :
            throw new InvalidOperationException());
    }
}