using Anna.Foundation;
using Anna.Gui.Messaging;
using Anna.Service;

namespace Anna.Gui.Foundations;

public class ViewModelBase : DisposableNotificationObject
{
    public Messenger Messenger => _messenger ??= Dic.GetInstance<Messenger>();

    private Messenger? _messenger;

    protected ViewModelBase(IServiceProvider dic)
        : base(dic)
    {
    }
}