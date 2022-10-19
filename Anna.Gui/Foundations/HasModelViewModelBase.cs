using Anna.Service;

namespace Anna.Gui.Foundations;

public class HasModelViewModelBase<TModel>
    : ViewModelBase,
        IHasArg<TModel>
{
    public readonly TModel Model;

    protected HasModelViewModelBase(IServiceProvider dic)
        : base(dic)
    {
        dic.PopArg(out Model);
    }
}