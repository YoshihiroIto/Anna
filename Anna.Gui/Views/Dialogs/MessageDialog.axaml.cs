using Anna.Gui.Views.Dialogs.Base;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using System;

namespace Anna.Gui.Views.Dialogs;

public partial class MessageDialog : Window
{
    public MessageDialog()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
        
        var okButton = this.FindControl<Button>("OkButton");
        if (okButton is not null)
        {
            okButton.AttachedToVisualTree += (_, _) =>
                FocusManager.Instance?.Focus(okButton, NavigationMethod.Directional);
        }
        
        PropertyChanged += (s, e) =>
        {
            if (e.Property == DataContextProperty)
            {
                if (e.OldValue is DialogViewModel oldViewModel)
                    oldViewModel.CloseRequested -= OnCloseRequested;

                if (e.NewValue is DialogViewModel newViewModel)
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