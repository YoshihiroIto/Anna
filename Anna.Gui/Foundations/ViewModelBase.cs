using Anna.DomainModel.Interface;
using Anna.Foundation;

namespace Anna.Foundations;

public class ViewModelBase : DisposableNotificationObject
{
    private readonly IObjectLifetimeChecker _objectLifetimeChecker;
    protected ViewModelBase(IObjectLifetimeChecker objectLifetimeChecker)
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