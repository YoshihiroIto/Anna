using Anna.Constants;
using Anna.Gui.Views.Windows.Base;
using Anna.UseCase;

namespace Anna.Gui.Views.Windows.Dialogs;

public class SortModeAndOrderDialogViewModel : WindowViewModelBase
{
    public SortModes ResultSortMode { get; set; }
    public SortOrders ResultSortOrder { get; set; }

    public SortModeAndOrderDialogViewModel(IServiceProvider dic)
        : base(dic)
    {
    }
}