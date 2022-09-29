using Anna.DomainModel;
using Anna.Foundation;
using Anna.Gui.Interfaces;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;

namespace Anna.Gui.Views.Panels;

public partial class DirectoryPanel : UserControl, IShortcutKeyReceiver
{
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

        PropertyChanged += (_, e) =>
        {
            if (e.Property == Control.DataContextProperty)
            {
                if (e.OldValue is DirectoryPanelViewModel oldViewModel)
                    oldViewModel.MessageDialogRequested -= OnViewModelOnMessageDialogRequested;

                if (e.NewValue is DirectoryPanelViewModel newViewModel)
                    newViewModel.MessageDialogRequested += OnViewModelOnMessageDialogRequested;
            }
        };
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);

        LayoutUpdated += (_, _) => UpdateItemCellSize();
    }

    public Window Owner
    {
        get
        {
            var parent = Parent;

            while (true)
            {
                switch (parent)
                {
                    case Window window:
                        return window;

                    case null:
                        throw new InvalidOperationException();
                }

                parent = parent.Parent;
            }
        }
    }

    public Directory Directory =>
        DirectoryPanelViewModel.Model;

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

    private async void OnViewModelOnMessageDialogRequested(object? sender, (string Title, string Message) e)
    {
        await DialogOperator.DisplayInformationAsync(
            DirectoryPanelViewModel.ServiceProviderContainer,
            Owner,
            e.Title,
            e.Message);
    }
}