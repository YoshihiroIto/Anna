using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Anna.Gui.Views.Dialogs;

public partial class MessageDialog : Window
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