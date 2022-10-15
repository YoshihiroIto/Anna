using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Anna.Gui.Views.Panels;

public sealed partial class InfoPanel : UserControl
{
    public InfoPanel()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}