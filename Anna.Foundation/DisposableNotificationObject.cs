using Anna.UseCase;
using System.Reactive.Disposables;

namespace Anna.Foundation;

public class DisposableNotificationObject : NotificationObject, IDisposable
{
    private readonly IObjectLifetimeCheckerUseCase _objectLifetimeChecker;

    protected CompositeDisposable Trash =>
        LazyInitializer.EnsureInitialized(ref _trashes, () => new CompositeDisposable()) ??
        throw new NullReferenceException();

    public DisposableNotificationObject(IObjectLifetimeCheckerUseCase objectLifetimeChecker)
    {
        _objectLifetimeChecker = objectLifetimeChecker;
        
        _objectLifetimeChecker.Add(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
            _trashes?.Dispose();

        _objectLifetimeChecker.Remove(this);
        
        _disposed = true;
    }

    public virtual void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private CompositeDisposable? _trashes;

    private bool _disposed;
}