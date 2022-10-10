using Anna.Service;
using SimpleInjector;
using System.Diagnostics;
using IServiceProvider=Anna.Service.IServiceProvider;

namespace Anna.ServiceProvider;

public class ServiceProviderBase : Container, IServiceProvider
{
    private readonly Stack<object> _args = new();
    
    public ServiceProviderBase()
    {
        RegisterSingleton<IServiceProvider>(() => this);

#if RELEASE
        Options.EnableAutoVerification = false;
#endif
    }

    public THasArgService GetInstance<THasArgService, TArg>(TArg arg)
        where THasArgService : class, IHasArg<TArg>
    {
        if (arg is null)
            throw new ArgumentNullException(nameof(arg));

        var count = _args.Count;
        _args.Push(arg);

        var instance = GetInstance<THasArgService>();

        Debug.Assert(_args.Count == count);

        return instance;
    }

    public void PopArg<TArg>(out TArg arg)
    {
        arg = (TArg)_args.Pop();
    }
}