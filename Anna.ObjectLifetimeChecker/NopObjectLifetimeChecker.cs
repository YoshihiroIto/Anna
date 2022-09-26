using Anna.UseCase;

namespace Anna.ObjectLifetimeChecker;

public sealed class NopObjectLifetimeChecker : IObjectLifetimeCheckerUseCase
{
    public void Start(Action<string> showError) {}
    public void End() {}
    public void Add(IDisposable disposable) {}
    public void Remove(IDisposable disposable) {}
}