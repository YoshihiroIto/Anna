using Anna.DomainModel;
using Anna.Gui.Interfaces;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;

namespace Anna.Gui.Views;

public partial class DirectoryView : UserControl, IShortcutKeyReceiver
{
    public static readonly DirectProperty<DirectoryView, DirectoryViewLayout> DirectoryViewLayoutProperty =
        AvaloniaProperty.RegisterDirect<DirectoryView, DirectoryViewLayout>(nameof(DirectoryViewLayout), o => o.Layout);

    public DirectoryViewLayout Layout
    {
        get => _Layout;
        private set => SetAndRaise(DirectoryViewLayoutProperty, ref _Layout, value);
    }

    private DirectoryViewLayout _Layout = new();

    public DirectoryView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
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

    public Directory Directory
    {
        get
        {
            var viewModel = DataContext as DirectoryViewViewModel ?? throw new NotSupportedException();

            return viewModel.Model;
        }
    }

    public Entry[] CollectTargetEntries()
    {
        var viewModel = DataContext as DirectoryViewViewModel ?? throw new NotSupportedException();

        return viewModel.CollectTargetEntries();
    }
}