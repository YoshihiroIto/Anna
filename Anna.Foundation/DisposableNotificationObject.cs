using Anna.UseCase;
using System.Reactive.Disposables;

namespace Anna.Foundation;

public class DisposableNotificationObject : NotificationObject, IDisposable
{
    protected readonly IServiceProviderContainer Dic;
    
    private CompositeDisposable? _trashes;
    private bool _disposed;

    protected CompositeDisposable Trash =>
        LazyInitializer.EnsureInitialized(ref _trashes, () => new CompositeDisposable()) ??
        throw new NullReferenceException();

    public DisposableNotificationObject(IServiceProviderContainer dic)
    {
        Dic = dic;
        
        Dic.GetInstance<IObjectLifetimeCheckerUseCase>().Add(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
            _trashes?.Dispose();

        Dic.GetInstance<IObjectLifetimeCheckerUseCase>().Remove(this);
        
        _disposed = true;
    }

    public virtual void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}