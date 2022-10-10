using Anna.DomainModel.Config;
using Anna.Gui.Views.Windows.Base;
using Anna.UseCase;

namespace Anna.Gui.Views.Windows.Dialogs;

public class ConfirmationDialogViewModel
    : HasModelWindowViewModelBase<(string Title, string Text, ConfirmationTypes confirmationType)>
{
    public string Title => Model.Title;
    public string Text => Model.Text;
    public bool IsYesNo => Model.confirmationType == ConfirmationTypes.YesNo;

    public ConfirmationDialogViewModel(
        IServiceProviderContainer dic,
        ResourcesHolder resourcesHolder,
        AppConfig appConfig,
        ILoggerUseCase logger,
        IObjectLifetimeCheckerUseCase objectLifetimeChecker)
        : base(dic, resourcesHolder, appConfig, logger, objectLifetimeChecker)
    {
    }
}