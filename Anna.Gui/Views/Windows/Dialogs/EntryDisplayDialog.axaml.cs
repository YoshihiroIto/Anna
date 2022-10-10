using Anna.Gui.Views.Windows.Base;
using Avalonia;
using Avalonia.Markup.Xaml;

namespace Anna.Gui.Views.Windows.Dialogs;

public partial class EntryDisplayDialog : WindowBase<EntryDisplayDialogViewModel>
{
    public EntryDisplayDialog()
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