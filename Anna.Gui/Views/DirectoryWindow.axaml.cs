using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using System;

namespace Anna.Gui.Views;

public partial class DirectoryWindow : Window
{
    public DirectoryWindow()
    {
        InitializeComponent();

#if DEBUG
        this.AttachDevTools();
#endif

        var directoryView = this.FindControl<DirectoryView>("DirectoryView");
        if (directoryView is not null)
        {
            directoryView.AttachedToVisualTree += (_, _) =>
                FocusManager.Instance?.Focus(directoryView, NavigationMethod.Directional);
        }

        PropertyChanged += (s, e) =>
        {
            if (e.Property == Control.DataContextProperty)
            {
                if (e.OldValue is DirectoryWindowViewModel oldViewModel)
                    oldViewModel.Close -= OnClose;
                
                if (e.NewValue is DirectoryWindowViewModel newViewModel)
                    newViewModel.Close += OnClose;
            }
        };
    }
    private void OnClose(object? sender, EventArgs e)
    {
        Close();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}