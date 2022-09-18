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
}