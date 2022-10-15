using Anna.Gui.Views.Windows.Base;
using Anna.Service;

namespace Anna.Gui.Views.Windows.Dialogs;

public sealed class ChangeEntryNameDialogViewModel
    : HasModelWindowBaseViewModel<(string CurrentName, int Dummy)>
{
    public string ResultName { get; private set; } = "";

    public string CurrentName => Model.CurrentName;

    public ChangeEntryNameDialogViewModel(IServiceProvider dic)
        : base(dic)
    {
    }
}