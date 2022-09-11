using Anna.DomainModel.Interface;
using Anna.Foundation;
using System.Reactive.Disposables;

namespace Anna.Foundations;

public class ViewModelBase : DisposableNotificationObject
{
    protected ViewModelBase(IObjectLifetimeChecker objectLifetimeChecker)
    {
        objectLifetimeChecker.Add(this);
        
        Trash.Add(Disposable.Create(objectLifetimeChecker, c => c.Remove(this)));
    }
}