using Anna.Gui.UseCase.Interfaces;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Anna.Views;

public partial class DirectoryView : UserControl, IShortcutKeyReceiver
{
    public DirectoryView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}