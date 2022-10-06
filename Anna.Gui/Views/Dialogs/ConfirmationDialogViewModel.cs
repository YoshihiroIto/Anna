using Anna.Gui.Views.Dialogs.Base;
using Anna.UseCase;

namespace Anna.Gui.Views.Dialogs;

public class ConfirmationDialogViewModel
    : HasModelDialogViewModel<(string Title, string Text, ConfirmationTypes confirmationType)>
{
    public string Title => Model.Title;
    public string Text => Model.Text;
    public bool IsYesNo => Model.confirmationType == ConfirmationTypes.YesNo;

    public ConfirmationDialogViewModel(
        IServiceProviderContainer dic,
        ResourcesHolder resourcesHolder,
        ILoggerUseCase logger,
        IObjectLifetimeCheckerUseCase objectLifetimeChecker)
        : base(dic, resourcesHolder, logger, objectLifetimeChecker)
    {
    }
}