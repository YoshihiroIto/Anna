using Anna.Service;

namespace Anna.Gui.Views.Windows.Base;

public class HasModelWindowBaseViewModel<TModel> : WindowBaseViewModel, IHasArg<TModel>
{
    public readonly TModel Model;

    protected HasModelWindowBaseViewModel(IServiceProvider dic)
        : base(dic)
    {
        dic.PopArg(out Model);
    }
}