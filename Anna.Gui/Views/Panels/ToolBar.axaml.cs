using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Anna.Gui.Views.Panels;

public sealed partial class ToolBar : DockPanel
{
    public ToolBar()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}