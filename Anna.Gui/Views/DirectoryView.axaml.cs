using Anna.ViewModels.ShortcutKey;
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