using Anna.DomainModel.Interface;

namespace Anna.Interactor;

public sealed class NopObjectLifetimeChecker : IObjectLifetimeChecker
{
    public void Start(Action<string> showError) { }
    public void End() { }
    public void Add(IDisposable disposable) { }
    public void Remove(IDisposable disposable) { }
}