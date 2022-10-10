using Anna.DomainModel.Config;
using Anna.UseCase;

namespace Anna.Gui.Views.Windows.Base;

public class HasModelWindowViewModelBase<TModel> : WindowViewModelBase, IHasArg<TModel>
{
    public readonly TModel Model;

    protected HasModelWindowViewModelBase(
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