using Anna.Foundation;
using Anna.Gui.Messaging;
using Anna.Service;

namespace Anna.Gui.Foundations;

public class ViewModelBase : DisposableNotificationObject
{
    public InteractionMessenger Messenger => _messenger ??= Dic.GetInstance<InteractionMessenger>();

    private InteractionMessenger? _messenger;

    protected ViewModelBase(IServiceProvider dic)
        : base(dic)
    {
    }
}