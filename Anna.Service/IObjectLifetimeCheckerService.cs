namespace Anna.Service;

public interface IObjectLifetimeCheckerService
{
    void Start(Action<string> showError);
    void End();
    void Add(IDisposable disposable);
    void Remove(IDisposable disposable);
}