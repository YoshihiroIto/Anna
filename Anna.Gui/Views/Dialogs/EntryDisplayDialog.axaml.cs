using Anna.Gui.Views.Dialogs.Base;
using Avalonia;
using Avalonia.Markup.Xaml;

namespace Anna.Gui.Views.Dialogs;

public partial class EntryDisplayDialog : DialogBase<EntryDisplayDialogViewModel>
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