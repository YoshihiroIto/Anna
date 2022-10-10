using Anna.DomainModel.Config;
using Anna.UseCase;

namespace Anna.Gui.Views.Dialogs.Base;

public class HasModelDialogViewModel<TModel> : DialogViewModel, IHasArg<TModel>
{
    public readonly TModel Model;

    protected HasModelDialogViewModel(
        IServiceProviderContainer dic,
        ResourcesHolder resourcesHolder,
        AppConfig appConfig,
        ILoggerUseCase logger,
        IObjectLifetimeCheckerUseCase objectLifetimeChecker)
        : base(dic, resourcesHolder, appConfig, logger, objectLifetimeChecker)
    {
        dic.PopArg(out Model);
    }
}