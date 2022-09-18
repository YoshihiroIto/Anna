using Anna.DomainModel.Interfaces;

namespace Anna.DomainModel.ObjectLifetimeChecker;

public sealed class NopObjectLifetimeChecker : IObjectLifetimeChecker
{
    public void Start(Action<string> showError) { }
    public void End() { }
    public void Add(IDisposable disposable) { }
    public void Remove(IDisposable disposable) { }
}