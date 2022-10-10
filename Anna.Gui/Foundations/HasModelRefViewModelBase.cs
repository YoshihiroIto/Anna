using Anna.UseCase;

namespace Anna.Gui.Foundations;

public class HasModelRefViewModelBase<TModel> : ViewModelBase, IHasArg<TModel>
{
    public readonly TModel Model;

    protected HasModelRefViewModelBase(IServiceProvider dic)
        : base(dic)
    {
        dic.PopArg(out Model);
    }
}