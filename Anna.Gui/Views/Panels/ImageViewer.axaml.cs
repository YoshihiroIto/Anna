using Anna.Gui.Interactions.Hotkey;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Anna.Gui.Views.Panels;

public sealed partial class ImageViewer : UserControl, IImageViewerHotkeyReceiver
{
    public ImageViewer()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}