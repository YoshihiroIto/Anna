using Anna.DomainModel;
using Anna.DomainModel.Interfaces;

namespace Anna.Gui.Views.Dialogs;

public class SortModeAndOrderDialogViewModel : DialogViewModel
{
    public SortModes SortMode { get; set; }
    public SortOrders SortOrder { get; set; }
    public SortModeAndOrderDialogViewModel(IObjectLifetimeChecker objectLifetimeChecker)
        : base(objectLifetimeChecker)
    {
    }
}