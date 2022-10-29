using Anna.Service;
using SimpleInjector;
using System.Diagnostics;
using IServiceProvider=Anna.Service.IServiceProvider;

namespace Anna.ServiceProvider;

public class ServiceProviderBase : Container, IServiceProvider
{
    private object? _argBox;

    public ServiceProviderBase()
    {
        RegisterSingleton<IServiceProvider>(() => this);

#if RELEASE
        Options.EnableAutoVerification = false;
#endif
    }
    
    public TService GetInstance<TService>(Type type) where TService : class
    {
        return (TService)GetInstance(type);
    }
    
    public THasArgService GetInstance<THasArgService, TArg>(TArg arg)
        where THasArgService : class, IHasArg<TArg>
    {
        if (arg is null)
            throw new ArgumentNullException(nameof(arg));

        Debug.Assert(_argBox is null);

        _argBox = arg;

        var instance = GetInstance<THasArgService>();

        Debug.Assert(_argBox is null);

        return instance;
    }

    public void PopArg<TArg>(out TArg arg)
    {
        Debug.Assert(_argBox is not null);

        arg = (TArg)_argBox;
        _argBox = null;
    }
}