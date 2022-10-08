using Anna.Gui.Views.Dialogs.Base;
using Avalonia;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

namespace Anna.Gui.Views.Dialogs;

// ReSharper disable once PartialTypeWithSinglePart
public partial class ConfirmationDialog : DialogBase
{
    public ConfirmationDialog()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }
    
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        DoMoveFocus(e);
    }
}