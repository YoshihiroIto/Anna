using Anna.UseCase;

namespace Anna.Gui.Views.Dialogs.Base;

public class HasModelDialogViewModel<TModel> : DialogViewModel, IHasArg<TModel>
{
    public readonly TModel Model;

    protected HasModelDialogViewModel(
        IServiceProviderContainer dic,
        ResourcesHolder resourcesHolder,
        ILoggerUseCase logger,
        IObjectLifetimeCheckerUseCase objectLifetimeChecker)
        : base(dic, resourcesHolder, logger, objectLifetimeChecker)
    {
        dic.PopArg(out Model);
    }
}