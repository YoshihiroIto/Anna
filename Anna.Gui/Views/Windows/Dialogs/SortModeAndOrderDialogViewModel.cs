using Anna.Constants;
using Anna.DomainModel.Config;
using Anna.Gui.Views.Windows.Base;
using Anna.UseCase;

namespace Anna.Gui.Views.Windows.Dialogs;

public class SortModeAndOrderDialogViewModel : WindowViewModelBase
{
    public SortModes ResultSortMode { get; set; }
    public SortOrders ResultSortOrder { get; set; }

    public SortModeAndOrderDialogViewModel(
        IServiceProviderContainer dic,
        ResourcesHolder resourcesHolder,
        AppConfig appConfig,
        ILoggerUseCase logger,
        IObjectLifetimeCheckerUseCase objectLifetimeChecker)
        : base(dic, resourcesHolder, appConfig, logger, objectLifetimeChecker)
    {
    }
}