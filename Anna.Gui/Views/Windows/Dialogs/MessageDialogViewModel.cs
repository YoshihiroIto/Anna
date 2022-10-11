using Anna.Gui.Views.Windows.Base;
using Anna.Service;

namespace Anna.Gui.Views.Windows.Dialogs;

public sealed class MessageDialogViewModel : HasModelWindowBaseViewModel<(string Title, string Text)>
{
    public string Title => Model.Title;
    public string Text => Model.Text;

    public MessageDialogViewModel(IServiceProvider dic)
        : base(dic)
    {
    }
}