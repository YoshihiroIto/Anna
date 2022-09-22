using Anna.DomainModel;
using Anna.DomainModel.Interfaces;
using Anna.Gui.Views.Dialogs.Base;

namespace Anna.Gui.Views.Dialogs;

public class SortModeAndOrderDialogViewModel : DialogViewModel
{
    public SortModes SortMode { get; set; }
    public SortOrders SortOrder { get; set; }
    public SortModeAndOrderDialogViewModel(ILogger logger, IObjectLifetimeChecker objectLifetimeChecker)
        : base(logger, objectLifetimeChecker)
    {
    }
}