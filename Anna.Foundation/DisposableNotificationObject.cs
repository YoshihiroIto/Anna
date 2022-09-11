using System.Reactive.Disposables;

namespace Anna.Foundation;

public class DisposableNotificationObject : NotificationObject, IDisposable
{
    protected CompositeDisposable Trash =>
        LazyInitializer.EnsureInitialized(ref _trashes, () => new CompositeDisposable()) ??
        throw new NullReferenceException();

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
            _trashes?.Dispose();

        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private CompositeDisposable? _trashes;

    private bool _disposed;
}