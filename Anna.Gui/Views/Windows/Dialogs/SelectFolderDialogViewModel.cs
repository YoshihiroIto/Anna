using Anna.Gui.Views.Windows.Base;
using Anna.Service;

namespace Anna.Gui.Views.Windows.Dialogs;

public sealed class SelectFolderDialogViewModel
    : HasModelWindowBaseViewModel<(string CurrentFolderPath, int Dummy)>
{
    public string ResultPath { get; private set; } = "";

    public string CurrentFolderPath => Model.CurrentFolderPath;

    public SelectFolderDialogViewModel(IServiceProvider dic)
        : base(dic)
    {
    }
}