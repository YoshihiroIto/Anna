using Anna.Gui.Views.Windows.Base;
using Avalonia;
using Avalonia.Markup.Xaml;

namespace Anna.Gui.Views.Windows.Dialogs;

public partial class ChangeEntryNameDialog : WindowBase<ChangeEntryNameDialogViewModel>
{
    public ChangeEntryNameDialog()
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