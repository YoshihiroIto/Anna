using Anna.Service;

namespace Anna.Gui.Views.Windows.Base;

public class HasModelWindowViewModelBase<TModel> : WindowBaseViewModel, IHasArg<TModel>
{
    public readonly TModel Model;

    protected HasModelWindowViewModelBase(IServiceProvider dic)
        : base(dic)
    {
        dic.PopArg(out Model);
    }
}