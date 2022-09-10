using Anna.DomainModel.Interface;
using Anna.Foundation;
using System;
using System.Reactive.Disposables;
using System.Threading;

namespace Anna.Foundations;

public class ViewModelBase : NotificationObject, IDisposable
{
    protected CompositeDisposable Trash =>
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