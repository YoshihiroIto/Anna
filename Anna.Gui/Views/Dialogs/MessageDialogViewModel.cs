using Anna.Gui.Views.Dialogs.Base;
using Anna.UseCase;

namespace Anna.Gui.Views.Dialogs;

public class MessageDialogViewModel : HasModelDialogViewModel<(string Title, string Message)>
{
    public string Message => Model.Message;
    public string Title => Model.Title;

    public MessageDialogViewModel(
        IServiceProviderContainer dic,
        ResourcesHolder resourcesHolder,
        ILoggerUseCase logger,
        IObjectLifetimeCheckerUseCase objectLifetimeChecker)
        : base(dic, resourcesHolder, logger, objectLifetimeChecker)
    {
    }
}