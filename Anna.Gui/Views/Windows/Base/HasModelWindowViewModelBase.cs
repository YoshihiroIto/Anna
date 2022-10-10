using Anna.UseCase;

namespace Anna.Gui.Views.Windows.Base;

public class HasModelWindowViewModelBase<TModel> : WindowViewModelBase, IHasArg<TModel>
{
    public readonly TModel Model;

    protected HasModelWindowViewModelBase(IServiceProviderContainer dic)
        : base(dic)
    {
        dic.PopArg(out Model);
    }
}