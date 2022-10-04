using Anna.Constants;
using Anna.Gui.Views.Dialogs.Base;
using Anna.UseCase;

namespace Anna.Gui.Views.Dialogs;

public class SortModeAndOrderDialogViewModel : DialogViewModel
{
    public SortModes ResultSortMode { get; set; }
    public SortOrders ResultSortOrder { get; set; }

    public SortModeAndOrderDialogViewModel(
        IServiceProviderContainer dic,
        ResourcesHolder resourcesHolder,
        ILoggerUseCase logger,
        IObjectLifetimeCheckerUseCase objectLifetimeChecker)
        : base(dic, resourcesHolder, logger, objectLifetimeChecker)
    {
    }
}