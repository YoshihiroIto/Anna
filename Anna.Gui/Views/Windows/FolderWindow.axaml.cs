using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Anna.Gui.Views.Windows;

public partial class FolderWindow : Window
{
    public FolderWindow()
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