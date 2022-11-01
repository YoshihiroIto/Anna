using Anna.Service;

namespace Anna.Gui.Views.Windows.Base;

public class HasModelWindowBaseViewModel<TSelf, TModel> : WindowBaseViewModel, IHasArg<TModel>
{
    // for type propagation
    public static readonly TSelf T = default!;
    
    public readonly TModel Model;

    protected HasModelWindowBaseViewModel(IServiceProvider dic)
        : base(dic)
    {
        dic.PopArg(out Model);
    }
}
