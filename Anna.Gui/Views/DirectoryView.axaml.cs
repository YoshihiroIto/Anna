using Anna.DomainModel;
using Anna.Gui.ViewModels;
using Anna.UseCase.Interfaces;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;

namespace Anna.Gui.Views;

public partial class DirectoryView : UserControl, IShortcutKeyReceiver
{
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

    public Entry[] CollectTargetEntities()
    {
        var viewModel = DataContext as DirectoryViewViewModel ?? throw new NotSupportedException();

        return viewModel.CollectTargetEntities();
    }
}