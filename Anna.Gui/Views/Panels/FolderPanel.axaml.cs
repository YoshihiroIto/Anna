using Anna.Constants;
using Anna.DomainModel;
using Anna.Foundation;
using Anna.Gui.Messaging;
using Anna.Gui.Hotkey;
using Anna.Service.Workers;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;
using Entry=Anna.DomainModel.Entry;

namespace Anna.Gui.Views.Panels;

public sealed partial class FolderPanel : UserControl, IFolderPanelHotkeyReceiver
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

    public FolderPanelViewModel ViewModel => _viewModel ?? throw new InvalidOperationException();
    private FolderPanelViewModel? _viewModel;

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

        PropertyChanged += (_, e) =>
        {
            if (e.Property == DataContextProperty)
            {
                _viewModel = DataContext as FolderPanelViewModel ?? throw new NotSupportedException();
                Layout.ViewModel = _viewModel;
            }
        };
    }

    Messenger IHotkeyReceiver.Messenger => ViewModel.Messenger;
    Folder IFolderPanelHotkeyReceiver.Folder => ViewModel.Model;

    Entry IFolderPanelHotkeyReceiver.CurrentEntry =>
        ViewModel.CursorEntry.Value?.Model.Entry ?? throw new InvalidOperationException();

    IEnumerable<Entry> IFolderPanelHotkeyReceiver.CollectTargetEntries() => ViewModel.CollectTargetEntries();
    IBackgroundWorker IFolderPanelHotkeyReceiver.BackgroundWorker => ViewModel.Model.BackgroundWorker;

    void IFolderPanelHotkeyReceiver.MoveCursor(Directions dir) => ViewModel.MoveCursor(dir);
    void IFolderPanelHotkeyReceiver.ToggleSelectionCursorEntry(bool isMoveDown) =>
        ViewModel.ToggleSelectionCursorEntry(isMoveDown);
    void IFolderPanelHotkeyReceiver.SetListMode(uint index) =>
        ViewModel.SetListMode(index);

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
        var newPageIndex = SelectedIndex / (ItemCellSize.Width * ItemCellSize.Height);

        if (PageIndex == newPageIndex)
            return false;

        PageIndex = newPageIndex;

        if (isRaiseEvenWhenChanged)
            PageIndexChanged?.Invoke(this, EventArgs.Empty);

        return true;
    }
}