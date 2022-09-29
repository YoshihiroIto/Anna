using Anna.Gui.Views.Panels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
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

        var directoryPanel = this.FindControl<DirectoryPanel>("DirectoryPanel");
        if (directoryPanel is not null)
        {
            directoryPanel.AttachedToVisualTree += (_, _) =>
                FocusManager.Instance?.Focus(directoryPanel, NavigationMethod.Directional);
        }

        PropertyChanged += (s, e) =>
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