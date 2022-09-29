﻿using Anna.DomainModel;
using Anna.Foundation;
using Anna.Gui.Foundations;
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