using Anna.DomainModel;
using Anna.Foundation;
using Anna.Gui.Interfaces;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;

namespace Anna.Gui.Views;

public partial class DirectoryView : UserControl, IShortcutKeyReceiver
{
    public static readonly StyledProperty<int> SelectedIndexProperty =
        AvaloniaProperty.Register<DirectoryView, int>(nameof(SelectedIndex));
    
    public int SelectedIndex
    {
        get => GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }
    
    internal static readonly DirectProperty<DirectoryView, DirectoryViewLayout> DirectoryViewLayoutProperty =
        AvaloniaProperty.RegisterDirect<DirectoryView, DirectoryViewLayout>(nameof(DirectoryViewLayout), o => o.Layout);

    internal static readonly DirectProperty<DirectoryView, IntSize> ItemCellSizeProperty =
        AvaloniaProperty.RegisterDirect<DirectoryView, IntSize>(nameof(ItemCellSize), o => o.ItemCellSize);

    internal DirectoryViewLayout Layout
    {
        get => _Layout;
        private set => SetAndRaise(DirectoryViewLayoutProperty, ref _Layout, value);
    }

    internal IntSize ItemCellSize
    {
        get => _ItemCellSize;
        private set => SetAndRaise(ItemCellSizeProperty, ref _ItemCellSize, value);
    }

    private DirectoryViewLayout _Layout = new();
    private IntSize _ItemCellSize;

    public DirectoryView()
    {
        InitializeComponent();
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
        DirectoryViewViewModel.Model;

    public DirectoryViewViewModel DirectoryViewViewModel =>
        DataContext as DirectoryViewViewModel ?? throw new NotSupportedException();

    public Entry[] CollectTargetEntries()
        => DirectoryViewViewModel.CollectTargetEntries();

    private void UpdateItemCellSize()
    {
        ItemCellSize = new IntSize(
            (int)(Bounds.Width / Layout.ItemWidth),
            (int)(Bounds.Height / Layout.ItemHeight)
        );
    }
}