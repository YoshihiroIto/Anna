using Anna.Gui.Views.Dialogs.Base;
using Avalonia;
using Avalonia.Markup.Xaml;

namespace Anna.Gui.Views.Dialogs;

// ReSharper disable once PartialTypeWithSinglePart
public partial class MessageDialog : DialogBase
{
    public MessageDialog()
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
}