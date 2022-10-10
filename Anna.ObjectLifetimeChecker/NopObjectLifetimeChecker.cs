using Anna.Service;

namespace Anna.ObjectLifetimeChecker;

public sealed class NopObjectLifetimeChecker : IObjectLifetimeCheckerService
{
    public void Start(Action<string> showError) {}
    public void End() {}
    public void Add(IDisposable disposable) {}
    public void Remove(IDisposable disposable) {}
}