using Anna.DomainModel.Foundation;
using Anna.DomainModel.Interface;
using System;
using System.Reactive.Disposables;
using System.Threading;

namespace Anna.Foundation;

public class ViewModelBase : NotificationObject, IDisposable
{
    public CompositeDisposable Trash =>
        LazyInitializer.EnsureInitialized(ref _trashes, () => new CompositeDisposable()) ??
        throw new NullReferenceException();

    protected ViewModelBase(IObjectLifetimeChecker objectLifetimeChecker)
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

        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);

        _objectLifetimeChecker.Remove(this);
    }

    private CompositeDisposable? _trashes;

    private bool _disposed;
    private readonly IObjectLifetimeChecker _objectLifetimeChecker;
}