using Anna.Gui.Views.Dialogs.Base;
using Avalonia;
using Avalonia.Markup.Xaml;

namespace Anna.Gui.Views.Windows;

public partial class FolderWindow : DialogBase<FolderWindowViewModel>
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