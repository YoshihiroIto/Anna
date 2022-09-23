using Anna.UseCase;

namespace Anna.DomainModel.ObjectLifetimeChecker;

public sealed class NopObjectLifetimeChecker : IObjectLifetimeCheckerUseCase
{
    public void Start(Action<string> showError) { }
    public void End() { }
    public void Add(IDisposable disposable) { }
    public void Remove(IDisposable disposable) { }
}