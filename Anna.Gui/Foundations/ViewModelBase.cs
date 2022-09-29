using Anna.Foundation;
using Anna.Gui.ViewModels.Messaging;
using Anna.UseCase;

namespace Anna.Gui.Foundations;

public class ViewModelBase : DisposableNotificationObject
{
    public InteractionMessenger Messenger => _messenger ??= new InteractionMessenger();

    private InteractionMessenger? _messenger;
    private readonly IObjectLifetimeCheckerUseCase _objectLifetimeChecker;

    protected ViewModelBase(IObjectLifetimeCheckerUseCase objectLifetimeChecker)
    {
        _objectLifetimeChecker = objectLifetimeChecker;

        _objectLifetimeChecker.Add(this);
    }

    public override void Dispose()
    {
        _objectLifetimeChecker.Remove(this);

        base.Dispose();
    }
}