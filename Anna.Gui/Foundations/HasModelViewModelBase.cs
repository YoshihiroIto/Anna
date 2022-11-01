using Anna.Service;

namespace Anna.Gui.Foundations;

public class HasModelViewModelBase<TSelf, TModel>
    : ViewModelBase, IHasArg<TModel>
{
    // for type propagation
    public static readonly TSelf T = default!;
    
    public readonly TModel Model;

    protected HasModelViewModelBase(IServiceProvider dic)
        : base(dic)
    {
        dic.PopArg(out Model);
    }
}