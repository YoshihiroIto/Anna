using Anna.Service;
using Anna.Service.Services;
using System.Reactive.Disposables;
using IServiceProvider=Anna.Service.IServiceProvider;

namespace Anna.Foundation;

public class DisposableNotificationObject : NotificationObject, IDisposable
{
    protected readonly IServiceProvider Dic;

    private CompositeDisposable? _trashes;
    private bool _disposed;

    protected CompositeDisposable Trash =>
        LazyInitializer.EnsureInitialized(ref _trashes, () => new CompositeDisposable()) ??
        throw new NullReferenceException();

    public DisposableNotificationObject(IServiceProvider dic)
    {
        Dic = dic;

        Dic.GetInstance<IObjectLifetimeCheckerService>().Add(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
            _trashes?.Dispose();

        Dic.GetInstance<IObjectLifetimeCheckerService>().Remove(this);

        _disposed = true;
    }

    public virtual void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}

public class HasArgDisposableNotificationObject<TArg> : DisposableNotificationObject, IHasArg<TArg>
{
    protected readonly TArg Arg;

    public HasArgDisposableNotificationObject(IServiceProvider dic)
        : base(dic)
    {
        dic.PopArg(out Arg);
    }
}