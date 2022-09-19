using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

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
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}