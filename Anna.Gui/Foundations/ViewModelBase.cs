using Anna.Foundation;
using Anna.Gui.Messaging;
using Anna.UseCase;

namespace Anna.Gui.Foundations;

public class ViewModelBase : DisposableNotificationObject
{
    public InteractionMessenger Messenger => _messenger ??= _dic.GetInstance<InteractionMessenger>();

    private InteractionMessenger? _messenger;
    private readonly IServiceProviderContainer _dic;

    protected ViewModelBase(
        IServiceProviderContainer dic,
        IObjectLifetimeCheckerUseCase objectLifetimeChecker)
        : base(objectLifetimeChecker)
    {
        _dic = dic;
    }
}