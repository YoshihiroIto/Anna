using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;

namespace Anna.Gui.Views.Windows;

public partial class DirectoryWindow : Window
{
    public DirectoryWindow()
    {
        InitializeComponent();

#if DEBUG
        this.AttachDevTools();
#endif

        PropertyChanged += (_, e) =>
        {
            if (e.Property == DataContextProperty)
            {
                if (e.OldValue is DirectoryWindowViewModel oldViewModel)
                    oldViewModel.CloseRequested -= OnCloseRequested;

                if (e.NewValue is DirectoryWindowViewModel newViewModel)
                    newViewModel.CloseRequested += OnCloseRequested;
            }
        };
    }
    
    private void OnCloseRequested(object? sender, EventArgs e)
    {
        Close();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}