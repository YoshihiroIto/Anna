using Anna.Gui.Views.Windows.Base;
using Anna.UseCase;

namespace Anna.Gui.Views.Windows.Dialogs;

public class MessageDialogViewModel : HasModelWindowViewModelBase<(string Title, string Text)>
{
    public string Title => Model.Title;
    public string Text => Model.Text;

    public MessageDialogViewModel(IServiceProviderContainer dic)
        : base(dic)
    {
    }
}