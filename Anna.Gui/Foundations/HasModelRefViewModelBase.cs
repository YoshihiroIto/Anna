using Anna.UseCase;

namespace Anna.Gui.Foundations;

public class HasModelRefViewModelBase<TModel> : ViewModelBase, IHasArg<TModel>
{
    public readonly TModel Model;

    protected HasModelRefViewModelBase(
        IServiceProviderContainer dic,
        IObjectLifetimeCheckerUseCase objectLifetimeChecker)
        : base(objectLifetimeChecker)
    {
        dic.PopArg(out Model);
    }
}